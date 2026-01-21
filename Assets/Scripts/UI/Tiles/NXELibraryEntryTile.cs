using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LibraryPlugin;
using LiteDB;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using LibraryEntry = Assets.Scripts.PersistentData.Models.LibraryEntry;

public class NXELibraryEntryTile : NXETile
{
    [SerializeField] private RawImage image;
    [SerializeField] private Text text;
    [SerializeField] private GameObject installedIcon;
    [SerializeField] private NXEModal gameDetailsBladePrefab;

    private NXEModal currentModal;
    private GameActionsManager gameActionsManager;

    private static Queue<NXELibraryEntryTile> artworkRequestQueue = new Queue<NXELibraryEntryTile>();
    private static UniTask activeQueueTask = UniTask.CompletedTask;

    private LibraryEntry libraryEntry;
    private bool isOperant;
    private string activeModalId;

    public void SetLibraryEntry(LibraryEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("SetLibraryEntry null?");
            return;
        }

        gameActionsManager = FindFirstObjectByType<GameActionsManager>();
        
        libraryEntry = entry;
        
        SetIsOperant(gameActionsManager.IsEntryOperant(libraryEntry));

        installedIcon.SetActive(string.IsNullOrEmpty(libraryEntry.Path));

        text.text = libraryEntry.Name;
        artworkRequestQueue.Enqueue(this);
        
        gameActionsManager.OnInstallationBegin += GameActionsManagerOnOnInstallationBegin;
        gameActionsManager.OnUninstallationBegin += GameActionsManagerOnOnInstallationBegin;
        gameActionsManager.OnInstallationCompleteOrCancelled += ActionManagerOnOnInstallationCompleteOrCancelled;
        gameActionsManager.OnUninstallationCompleteOrCancelled += ActionManagerOnOnInstallationCompleteOrCancelled;
    }

    private void GameActionsManagerOnOnInstallationBegin(ObjectId obj)
    {
        if (libraryEntry.Id == obj)
            SetIsOperant(true);
    }

    private void OnDestroy()
    {
        gameActionsManager.OnUninstallationCompleteOrCancelled -= ActionManagerOnOnInstallationCompleteOrCancelled;
        gameActionsManager.OnInstallationCompleteOrCancelled -= ActionManagerOnOnInstallationCompleteOrCancelled;
        gameActionsManager.OnUninstallationBegin -= GameActionsManagerOnOnInstallationBegin;
        gameActionsManager.OnInstallationBegin -= GameActionsManagerOnOnInstallationBegin;
    }

    void Update()
    {
        if (activeQueueTask.Status != UniTaskStatus.Pending && artworkRequestQueue.TryDequeue(out var result))
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
        catch (Exception e)
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
        if (currentModal && currentModal.TryGetComponent(out NXEBlade blade))
            blade.Select();
        else if (string.IsNullOrEmpty(libraryEntry.Path))
        {
            if (isOperant == false)
            {
                gameActionsManager.TryInstallLibraryEntry(libraryEntry);
                SetIsOperant(true);
            }
            else
            {
                var root = new GameObject("Root", typeof(RectTransform), typeof(VerticalLayoutGroup));
                root.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
                root.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

                var textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
                textObj.transform.SetParent(root.transform, false);
                var text = textObj.GetComponent<Text>();
                text.font = Resources.Load<Font>("NXD");
                text.fontSize = 26;
                text.alignment = TextAnchor.UpperCenter;
                text.text = $"{libraryEntry.Name} is currently being installed via {libraryEntry.Source}.\n\nTo view progress, please visit the third-party client.";

                activeModalId = FindFirstObjectByType<ModalServiceManager>().RequestCreateModal(new CreateModalArgs()
                {
                    CanBeClosed = true,
                    Name = $"Installing {libraryEntry.Name}",
                    ChildrenRoot = root
                });
            }
        }
        else
        {
            gameActionsManager.LaunchLibraryEntry(libraryEntry);
        }
    }

    void SetIsOperant(bool value)
    {
        isOperant = value;
        if (isOperant)
            installedIcon.transform
                .DOScale(1.5f, 1)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false);
        else
            installedIcon.transform.DOKill(complete: true);
    }

    public override void OnSelectAlt()
    {
        if (currentModal && currentModal.TryGetComponent(out NXEBlade blade))
            blade.SelectAlt();
        else
        {
            currentModal = NXEModal.CreateAndShow(gameDetailsBladePrefab);
            if (currentModal.TryGetComponent(out blade))
            {
                blade.Focus(animate: false);
                currentModal.GetComponentInChildren<NXELibraryEntryDetailsTile>().SetLibraryEntry(libraryEntry);
            }
        }
    }

    public override void OnCancel()
    {
        if (string.IsNullOrEmpty(activeModalId) == false)
            FindFirstObjectByType<ModalServiceManager>().RequestCloseModal(activeModalId);
        else if (currentModal != null)
        {
            if (currentModal.Close() == NXEModalCloseResult.NormalClose)
                currentModal = null;
        }
    }

    public override void OnMoveLeft(float speed = 1)
    {
        if (currentModal && currentModal.TryGetComponent(out NXEBlade blade))
            blade.MoveLeft(speed);
    }

    public override void OnMoveRight(float speed = 1)
    {
        if (currentModal && currentModal.TryGetComponent(out NXEBlade blade))
            blade.MoveRight(speed);
    }

    private void ActionManagerOnOnInstallationCompleteOrCancelled(ObjectId obj)
    {
        if (libraryEntry.Id == obj)
        {
            var entry = FindFirstObjectByType<DatabaseManager>().LibraryEntries.FindOne(x => x.Id == obj);
            
            SetLibraryEntry(entry);
        }
    }
}