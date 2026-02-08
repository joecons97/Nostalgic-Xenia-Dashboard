using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using LiteDB;
using Loadables;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LibrariesManager : MonoBehaviour, ILoadable
{
    [SerializeField] private PluginLoader loader;
    [SerializeField] private DatabaseManager databaseManager;

    public Library[] Libraries { get; private set; }

    public event Action<Library> OnLibraryImportBegin;
    public event Action<Library> OnLibraryImportCancelled;
    public event Action<Library> OnLibraryImportEnd;
    public event Action<ILoadable> OnLoadComplete;

    public Progress ImportProgress { get; private set; } = new Progress();

    public Library ActivelyImportingLibrary { get; private set; }

    private async UniTaskVoid Start()
    {
        Libraries = await loader.LoadLibraryPluginsAsync();
        OnLoadComplete?.Invoke(this);
    }

    public async UniTask ImportLibraryAsync(Library library, CancellationToken token = default)
    {
        if (token == default)
            token = this.GetCancellationTokenOnDestroy();

        ImportProgress.ReportProgress(0, "Initialising Import");

        ActivelyImportingLibrary = library;
        OnLibraryImportBegin?.Invoke(library);

        try
        {
            ImportProgress.ReportProgress(0, "Clearing Old Entries");

            var existingEntries = databaseManager.LibraryEntries
                .Find(e => e.Source == library.Name);

            foreach (var entry in existingEntries)
            {
                databaseManager.LibraryEntries.Delete(entry.Id);
            }

            ImportProgress.ReportProgress(0, "Gathering Entries");

            var entries = await library.Plugin.GetEntriesAsync(token);
            var index = 0;
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.EntryId))
                    continue;

                ImportProgress.ReportProgress((float)index / entries.Count, $"({index}/{entries.Count}) Importing {entry.Name}");

                var libraryEntry = new LibraryEntry()
                {
                    Id = ObjectId.NewObjectId(),
                    Name = entry.Name,
                    Developer = entry.Developer,
                    Publisher = entry.Publisher,
                    Description = entry.Description,
                    Path = entry.Path,
                    Genre = entry.Genre,
                    Rating = entry.Rating,
                    SourceId = entry.EntryId,
                    Source = library.Name,
                    CoverImagePath = null,
                    IconPath = null,
                    BannerImagePath = null,
                    LastPlayed = entry.LastPlayed
                };

                databaseManager.LibraryEntries.Insert(libraryEntry);
                index++;
            }

            ImportProgress.ReportProgress(1, "Finalising Import");

            OnLibraryImportEnd?.Invoke(library);
            ActivelyImportingLibrary = null;
        }
        catch (TaskCanceledException)
        {
            OnLibraryImportCancelled?.Invoke(library);
            ActivelyImportingLibrary = null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error importing library '{library.Name}': {ex}");

            OnLibraryImportEnd?.Invoke(library);
            ActivelyImportingLibrary = null;
        }
    }

    public async UniTask<string> DownloadImage(string url, string outputName, CancellationToken token)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
                return "";

            using UnityWebRequest uwr = UnityWebRequest.Get(url);
            await uwr.SendWebRequest().WithCancellation(token);

            // Get raw image bytes directly - no decode/encode needed!
            byte[] imageData = uwr.downloadHandler.data;

            var outputPath = Path.Combine(DatabaseManager.DatabasePath, "Images", $"{outputName}.{GetImageExtension(uwr)}");
            var directory = Path.GetDirectoryName(outputPath);

            // Switch to thread pool for I/O operations
            await UniTask.SwitchToThreadPool();

            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(outputPath, imageData, token);

            await UniTask.SwitchToMainThread();

            return outputPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to download image from {url}: {ex}");
            throw;
        }
    }


    private string GetImageExtension(UnityWebRequest uwr)
    {
        string contentType = uwr.GetResponseHeader("Content-Type");
        if (contentType?.Contains("jpeg") == true || contentType?.Contains("jpg") == true)
            return ".jpg";
        if (contentType?.Contains("png") == true)
            return ".png";

        // Fallback: detect from bytes
        byte[] data = uwr.downloadHandler.data;
        if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xD8)
            return ".jpg";
        if (data.Length >= 4 && data[0] == 0x89 && data[1] == 0x50)
            return ".png";

        return ".png"; // default
    }
}
