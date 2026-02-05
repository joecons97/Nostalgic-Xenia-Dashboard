using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(RawImage), typeof(AspectRatioFitter))]
public class AsyncImageLoader : MonoBehaviour
{
    [SerializeField, ReadOnly] private RawImage image;
    [SerializeField, ReadOnly] private NXEThrobber throbber;
    [SerializeField, ReadOnly] private AspectRatioFitter aspectRatioFitter;

    public bool HasImage => image.texture != null;

    private string src;

    private void OnValidate()
    {
        image = GetComponent<RawImage>();
        aspectRatioFitter = GetComponent<AspectRatioFitter>();
        throbber = GetComponentInChildren<NXEThrobber>();
    }
    
    private void Awake()
    {
        if(throbber)
            throbber.gameObject.SetActive(false);

        image.enabled = false;
    }

    public void SetSource(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        src = url;
        
        UniTask.Create(LoadImageAsync).Forget();
    }

    private async UniTask LoadImageAsync()
    {
        image.enabled = false;

        if (throbber)
            throbber.gameObject.SetActive(true);
        
        using var request = UnityWebRequestTexture.GetTexture(src);

        try
        {
            await request.SendWebRequest().WithCancellation(this.GetCancellationTokenOnDestroy());
            if(request.result == UnityWebRequest.Result.Success)
            {
                var texture = DownloadHandlerTexture.GetContent(request);
                image.texture = texture;
                image.enabled = true;

                aspectRatioFitter.aspectRatio = (float)texture.width / texture.height;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load image from {src}: {e.Message}");
            Debug.LogException(e);
        }
        finally
        {
            if (throbber)
                throbber.gameObject.SetActive(false);
        }
    }
}
