using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NXELibraryEntryTile : NXETile
{
    [SerializeField] private RawImage image;
    [SerializeField] private Text text;

    private LibraryEntry libraryEntry;

    public void SetLibraryEntry(LibraryEntry entry)
    {
        if(entry == null)
        {
            Debug.LogWarning("SetLibraryEntry null?");
            return;
        }

        libraryEntry = entry;

        text.text = libraryEntry.Name;
        _ = LoadImageAsync(libraryEntry.CoverImagePath)
            .ContinueWith(t =>
            {
                if (t)
                {
                    image.GetComponent<AspectRatioFitter>().aspectRatio = (float)t.width / (float)t.height;
                    image.texture = t;
                    text.enabled = false;
                }
            });
    }

    private async UniTask<Texture2D> LoadImageAsync(string imgPath)
    {
        if (string.IsNullOrEmpty(imgPath))
        {
            return null;
        }
        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imgPath);
        await uwr.SendWebRequest();

        return DownloadHandlerTexture.GetContent(uwr);
    }

    public override void OnSelect()
    {
        FindFirstObjectByType<GameActionsManager>().LaunchLibraryEntry(libraryEntry);
    }
}
