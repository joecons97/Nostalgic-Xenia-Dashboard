using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using LiteDB;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LibrariesManager : MonoBehaviour
{
    [SerializeField] private PluginLoader loader;
    [SerializeField] private DatabaseManager databaseManager;

    public Library[] Libraries { get; private set; }

    public event Action<Library> OnLibraryImportBegin;
    public event Action<Library> OnLibraryImportCancelled;
    public event Action<Library> OnLibraryImportEnd;

    public Library ActivelyImportingLibrary { get; private set; }

    private void Start()
    {
        Libraries = loader.LoadLibraryPlugins();
    }

    public async UniTask ImportLibraryAsync(Library library, CancellationToken token = default)
    {
        if (token == default)
            token = this.GetCancellationTokenOnDestroy();

        ActivelyImportingLibrary = library;
        OnLibraryImportBegin?.Invoke(library);

        await UniTask.WaitForSeconds(1, cancellationToken: token);

        try
        {
            var existingEntries = databaseManager.LibraryEntries
                .Find(e => e.Source == library.Name);

            foreach (var entry in existingEntries)
            {
                databaseManager.LibraryEntries.Delete(entry.Id);
                await UniTask.WaitForEndOfFrame(token);
            }

            foreach (var entry in library.Entries)
            {
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
                    Source = library.Name,
                    CoverImagePath = entry.CoverImagePath,
                    IconPath = entry.IconPath,
                    BannerImagePath = entry.BannerImagePath
                };

                databaseManager.LibraryEntries.Insert(libraryEntry);
                await UniTask.WaitForEndOfFrame(token);
            }

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
            Debug.LogError($"Error importing library '{library.Name}': {ex.Message}");

            OnLibraryImportEnd?.Invoke(library);
            ActivelyImportingLibrary = null;
        }
    }
}
