using Cysharp.Threading.Tasks;
using LibraryPlugin;
using UnityEngine;
using UnityEngine.Networking;

public class NXELibraryEntryImagesTiles : MonoBehaviour
{
    private AdditionalMetadata metadata;
    
    public void SetLibraryMetadataEntry(AdditionalMetadata data)
    {
        if (data == null)
        {
            Debug.LogWarning("SetLibraryMetadataEntry null?");
            return;
        }

        metadata = data;

        foreach (var metadataScreenshotUrl in metadata.ScreenshotUrls)
        {
            UniTask.RunOnThreadPool(async () =>
            {
                var image = await LoadImageAsync(metadataScreenshotUrl);
                Debug.Log($"Loaded image {metadataScreenshotUrl}");
            }).Forget();
        }
    }

    private async UniTask<Texture2D> LoadImageAsync(string url)
    {
        await UniTask.SwitchToMainThread();
        
        var request = UnityWebRequest.Get(url);
        await request.SendWebRequest().WithCancellation(this.GetCancellationTokenOnDestroy());
        var texture = DownloadHandlerTexture.GetContent(request);
        
        await UniTask.SwitchToThreadPool();
        return texture;
    }
}
