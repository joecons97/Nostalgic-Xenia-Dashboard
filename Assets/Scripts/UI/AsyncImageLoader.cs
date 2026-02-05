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
        src = url;
        
        UniTask.Create(LoadImageAsync).Forget();
    }

    private async UniTask LoadImageAsync()
    {
        image.enabled = false;

        if (throbber)
            throbber.gameObject.SetActive(true);
        
        var request = UnityWebRequestTexture.GetTexture(src);
        await request.SendWebRequest().WithCancellation(this.GetCancellationTokenOnDestroy());

        image.texture = DownloadHandlerTexture.GetContent(request);
        image.enabled = true;

        aspectRatioFitter.aspectRatio = (float)image.texture.width / image.texture.height;

        if (throbber)
            throbber.gameObject.SetActive(false);
    }
}
