using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LibrariesManager : MonoBehaviour
{
    [SerializeField] private PluginLoader loader;
    [SerializeField] private DatabaseManager databaseManager;

    public Library[] Libraries { get; private set; }

    public event Action<Library> OnLibraryImportBegin;
    public event Action<Library> OnLibraryImportCancelled;
    public event Action<Library> OnLibraryImportEnd;

    public Progress ImportProgress { get; private set; } = new Progress();

    public Library ActivelyImportingLibrary { get; private set; }

    private void Start()
    {
        Libraries = loader.LoadLibraryPlugins();
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

                var artwork = await library.Plugin.GetArtworkCollection(entry.EntryId, token);

                var paths = await UniTask.WhenAll(
                    DownloadImage(artwork.Cover, Path.Combine(entry.EntryId, "CoverImage"), token),
                    DownloadImage(artwork.Icon, Path.Combine(entry.EntryId, "Icon"), token),
                    DownloadImage(artwork.Banner, Path.Combine(entry.EntryId, "BannerImage"), token));

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
                    CoverImagePath = paths.Item1,
                    IconPath = paths.Item2,
                    BannerImagePath = paths.Item3,
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
        if (string.IsNullOrEmpty(url))
            return "";

        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
        await uwr.SendWebRequest().WithCancellation(token);

        var tex = DownloadHandlerTexture.GetContent(uwr);

        var outputPath = Path.Combine(DatabaseManager.DatabasePath, "Images", $"{outputName}.png");
        var directory = Path.GetDirectoryName(outputPath);
        if (Directory.Exists(directory) == false)
            Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(outputPath, tex.EncodeToPNG(), token);

        return outputPath;
    }
}
