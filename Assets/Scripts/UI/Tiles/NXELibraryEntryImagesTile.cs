using LibraryPlugin;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class NXELibraryEntryImagesTile : MonoBehaviour
{
    private AdditionalMetadata metadata;

    [SerializeField] private AsyncImageLoader asyncImagePrefab;
    [SerializeField] private ScrollSnap scrollSnap;
    [SerializeField] private Transform imagesParent;

    private List<AsyncImageLoader> imageLoaders = new();

    private void Start()
    {
        scrollSnap.onPageChange += ScrollSnap_onPageChange;
    }

    public void SetLibraryMetadataEntry(AdditionalMetadata data)
    {
        if (data == null)
        {
            Debug.LogWarning("SetLibraryMetadataEntry null?");
            return;
        }

        metadata = data;

        if (data.ScreenshotUrls == null || data.ScreenshotUrls.Length == 0)
            return;

        foreach (var loader in imageLoaders)
        {
            if (loader != null)
                Destroy(loader.gameObject);
        }

        imageLoaders.Clear();

        foreach (var metadataScreenshotUrl in metadata.ScreenshotUrls)
        {
            var image = Instantiate(asyncImagePrefab, imagesParent);
            imageLoaders.Add(image);
        }

        scrollSnap.UpdateListItemsSize();
        scrollSnap.UpdateListItemPositions();
        scrollSnap.ChangePage(scrollSnap.CurrentPage());
    }

    private void OnDestroy()
    {
        scrollSnap.onPageChange -= ScrollSnap_onPageChange;
    }

    private void ScrollSnap_onPageChange(int page)
    {
        if (page > imageLoaders.Count - 1)
            return;

        var img = imageLoaders[page];
        if (img.HasImage == false)
            img.SetSource(metadata.ScreenshotUrls[page]);
    }
}
