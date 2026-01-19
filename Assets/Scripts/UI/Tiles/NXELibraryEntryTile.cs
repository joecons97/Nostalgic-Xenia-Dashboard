using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NXELibraryEntryTile : NXETile
{
    [SerializeField] private RawImage image;
    [SerializeField] private Text text;
    [SerializeField] private GameObject installedIcon;

    private static Queue<NXELibraryEntryTile> artworkRequestQueue = new Queue<NXELibraryEntryTile>();
    private static UniTask activeQueueTask = UniTask.CompletedTask;

    private LibraryEntry libraryEntry;

    public void SetLibraryEntry(LibraryEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("SetLibraryEntry null?");
            return;
        }

        libraryEntry = entry;
        
        installedIcon.SetActive(string.IsNullOrEmpty(libraryEntry.Path));

        text.text = libraryEntry.Name;
        artworkRequestQueue.Enqueue(this);
    }

    void Update()
    {
        if(activeQueueTask.Status != UniTaskStatus.Pending && artworkRequestQueue.TryDequeue(out var result))
        {
            if (result == null)
                return;
            
            var token = result.destroyCancellationToken;
            activeQueueTask = UniTask.RunOnThreadPool(async () =>
            {
                var texture = await GetArtworkAsync(result.libraryEntry, token);
                if (texture)
                {
                    result.image.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / (float)texture.height;
                    result.image.texture = texture;
                    result.text.enabled = false;
                }
            });
        }
    }

    private static async UniTask<Texture2D> GetArtworkAsync(LibraryEntry libraryEntry, CancellationToken cancellationToken)
    {
        try
        {
            if (libraryEntry.HasSearchedForArtwork == false)
            {
                await UniTask.SwitchToMainThread();
                Debug.Log("Requesting Artwork for " + libraryEntry.Name);
                var libraryManager = FindFirstObjectByType<LibrariesManager>();
                var databaseManager = FindFirstObjectByType<DatabaseManager>();
                var lib = libraryManager.Libraries.FirstOrDefault(x => x.Name == libraryEntry.Source);
                if (lib != null)
                {
                    var artwork = await lib.Plugin.GetArtworkCollection(libraryEntry.SourceId, cancellationToken);
                    if (artwork != null)
                    {
                        var cover = await libraryManager.DownloadImage(artwork.Cover, Path.Combine(libraryEntry.SourceId, "CoverImage"), cancellationToken);
                        var icon = await libraryManager.DownloadImage(artwork.Icon, Path.Combine(libraryEntry.SourceId, "Icon"), cancellationToken);
                        var banner = await libraryManager.DownloadImage(artwork.Banner, Path.Combine(libraryEntry.SourceId, "BannerImage"), cancellationToken);

                        libraryEntry.CoverImagePath = cover;
                        libraryEntry.IconPath = icon;
                        libraryEntry.BannerImagePath = banner;
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                    return null;

                libraryEntry.HasSearchedForArtwork = true;
                databaseManager.LibraryEntries.Update(libraryEntry);
            }

            return await LoadImageAsync(libraryEntry.CoverImagePath, cancellationToken);
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    private static async UniTask<Texture2D> LoadImageAsync(string imgPath, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return null;

        if (string.IsNullOrEmpty(imgPath))
            return null;

        await UniTask.SwitchToMainThread();

        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imgPath);
        await uwr.SendWebRequest().WithCancellation(cancellationToken);

        return DownloadHandlerTexture.GetContent(uwr);
    }

    public override void OnSelect()
    {
        FindFirstObjectByType<GameActionsManager>().LaunchLibraryEntry(libraryEntry);
    }
}
