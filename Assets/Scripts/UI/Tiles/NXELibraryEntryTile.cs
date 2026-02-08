using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gilzoide.FlexUi;
using LibraryPlugin;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
    private LibrariesManager librariesManager;

    private static Queue<NXELibraryEntryTile> artworkRequestQueue = new();
    private static UniTask activeQueueTask = UniTask.CompletedTask;

    private Library library;
    private LibraryEntry libraryEntry;
    private bool isOperant;
    private string activeModalId;

    private bool requiresImageDownload => libraryEntry.HasSearchedForArtwork == false || 
                (libraryEntry.CoverImagePath != null && File.Exists(libraryEntry.CoverImagePath) == false) ||
                (libraryEntry.IconPath != null && File.Exists(libraryEntry.IconPath) == false) ||
                (libraryEntry.BannerImagePath != null && File.Exists(libraryEntry.BannerImagePath) == false);


    private void Awake()
    {
        gameActionsManager = FindFirstObjectByType<GameActionsManager>();
        librariesManager = FindFirstObjectByType<LibrariesManager>();
    }

    public void SetLibraryEntry(LibraryEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("SetLibraryEntry null?");
            return;
        }

        libraryEntry = entry;

        if (gameActionsManager == null)
            gameActionsManager = FindFirstObjectByType<GameActionsManager>();

        SetIsOperant(gameActionsManager.IsEntryOperant(libraryEntry));

        installedIcon.SetActive(string.IsNullOrEmpty(libraryEntry.Path));

        text.text = libraryEntry.Name;

        if(requiresImageDownload)
            artworkRequestQueue.Enqueue(this);
        else
        {
            UniTask.Create(async () =>
            {
                var texture = await LoadImageAsync(libraryEntry.CoverImagePath, destroyCancellationToken);
                ApplyTextureTo(this, texture);
            }).Forget();
        }
    }

    private void Start()
    {
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
                ApplyTextureTo(result, texture);
            });
        }
    }

    private static void ApplyTextureTo(NXELibraryEntryTile tile, Texture2D texture)
    {
        if (texture)
        {
            tile.image.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / (float)texture.height;
            tile.image.texture = texture;
            tile.text.enabled = false;
        }
    }

    private static async UniTask<Texture2D> GetArtworkAsync(LibraryEntry libraryEntry, CancellationToken cancellationToken)
    {
        try
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
                    var cover = await libraryManager.DownloadImage(artwork.Cover, Path.Combine(libraryEntry.Source, libraryEntry.SourceId, "CoverImage"), cancellationToken);
                    var icon = await libraryManager.DownloadImage(artwork.Icon, Path.Combine(libraryEntry.Source, libraryEntry.SourceId, "Icon"), cancellationToken);
                    var banner = await libraryManager.DownloadImage(artwork.Banner, Path.Combine(libraryEntry.Source, libraryEntry.SourceId, "BannerImage"), cancellationToken);

                    libraryEntry.CoverImagePath = cover;
                    libraryEntry.IconPath = icon;
                    libraryEntry.BannerImagePath = banner;
                }
            }

            if (cancellationToken.IsCancellationRequested)
                return null;

            libraryEntry.HasSearchedForArtwork = true;
            databaseManager.LibraryEntries.Update(libraryEntry);

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
            }
            else
            {
                var buttonObj = Resources.Load<GameObject>("EmptyButton");
                var root = new GameObject("Root", typeof(RectTransform), typeof(FlexLayout));
                //root.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
                //root.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

                var textObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(FlexLayout));
                textObj.transform.SetParent(root.transform, false);
                var text = textObj.GetComponent<Text>();
                text.font = Resources.Load<Font>("NXD");
                text.fontSize = 26;
                text.alignment = TextAnchor.UpperCenter;
                text.text = $"{libraryEntry.Name} is currently being installed via {libraryEntry.Source}.\n\nTo view progress, please visit the third-party client.";

                buttonObj = Instantiate(buttonObj, root.transform);
                buttonObj.GetComponentInChildren<Text>().text = "Open Library Client";
                var button = buttonObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    gameActionsManager.OpenLibrary(libraryEntry, LibraryLocation.Downloads);
                });

                activeModalId = FindFirstObjectByType<ModalServiceManager>().RequestCreateModal(new CreateModalArgs()
                {
                    CanBeClosed = true,
                    DisplaySelectAction = true,
                    Name = $"Installing {libraryEntry.Name}",
                    ChildrenRoot = root
                });

                _ = UniTask.WaitForSeconds(0.5f).ContinueWith(() =>
                {
                    if (button)
                        button.Select();
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
                .ChangeStartValue(Vector3.one)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false);
        else
        {
            installedIcon.transform.DOKill(complete: true);
            installedIcon.transform.localScale = Vector3.one;
        }
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

                library ??= librariesManager.Libraries.FirstOrDefault(x => x.Name == libraryEntry.Source);

                if (library != null)
                {
                    UniTask.Create(async () =>
                    {
                        try
                        {
                            var token = this.GetCancellationTokenOnDestroy();
                            var data = await library.Plugin.GetAdditionalMetadata(libraryEntry.SourceId, token);

                            var infoTile = currentModal.GetComponentInChildren<NXELibraryEntryInfoTile>();
                            if (infoTile)
                                infoTile.SetLibraryMetadataEntry(data);

                            var descriptionTile = currentModal.GetComponentInChildren<NXELibraryEntryDescriptionTile>();
                            if (descriptionTile)
                                descriptionTile.SetLibraryMetadataEntry(data);

                            var imagesTile = currentModal.GetComponentInChildren<NXELibraryEntryImagesTile>();
                            if (imagesTile)
                                imagesTile.SetLibraryMetadataEntry(data);
                        }
                        catch(OperationCanceledException)
                        {

                        }
                        catch(Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }).Forget();
                }
            }
        }
    }

    public override void OnCancel()
    {
        if (string.IsNullOrEmpty(activeModalId) == false)
        {
            FindFirstObjectByType<ModalServiceManager>().RequestCloseModal(activeModalId);
            activeModalId = null;
        }
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