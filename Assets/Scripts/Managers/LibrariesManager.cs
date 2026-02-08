using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using LiteDB;
using Loadables;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LibrariesManager : MonoBehaviour, ILoadable
{
    [SerializeField] private PluginLoader loader;
    [SerializeField] private DatabaseManager databaseManager;

    public Library[] Libraries { get; private set; }

    public event Action<Library> OnLibraryImportBegin;
    public event Action<Library> OnLibraryImportCancelled;
    public event Action<Library> OnLibraryImportEnd;
    public event Action<ILoadable> OnLoadComplete;

    private static readonly HttpClient httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

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

            var imageData = await httpClient.GetByteArrayAsync(url);

            var extension = GetImageExtensionFromUrl(url, imageData);
            var outputPath = Path.Combine(DatabaseManager.DatabasePath, "Images", $"{outputName}.{extension}");
            var directory = Path.GetDirectoryName(outputPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(outputPath, imageData, token);

            return outputPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to download image from {url}: {ex}");
            throw;
        }
    }

    private string GetImageExtensionFromUrl(string url, byte[] imageData)
    {
        // Try to get extension from URL first
        var uri = new Uri(url);
        var extension = Path.GetExtension(uri.LocalPath).TrimStart('.');

        if (!string.IsNullOrEmpty(extension) &&
            (extension == "png" || extension == "jpg" || extension == "jpeg" || extension == "webp"))
        {
            return extension;
        }

        // Fallback: detect from image data (magic bytes)
        if (imageData.Length >= 4)
        {
            if (imageData[0] == 0x89 && imageData[1] == 0x50) return "png";
            if (imageData[0] == 0xFF && imageData[1] == 0xD8) return "jpg";
            if (imageData[0] == 0x52 && imageData[1] == 0x49) return "webp";
        }

        return "png"; // default fallback
    }
}
