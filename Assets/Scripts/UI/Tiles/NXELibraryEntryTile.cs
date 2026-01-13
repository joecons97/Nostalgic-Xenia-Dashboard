using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NXELibraryEntryTile : NXETile
{
    [SerializeField] private RawImage image;
    [SerializeField] private Text text;

    public void SetLibraryEntry(LibraryEntry entry)
    {
        text.text = entry.Name;
        _ = LoadImageAsync(entry.CoverImagePath).ContinueWith(t => image.texture = t);
    }

    private async UniTask<Texture2D> LoadImageAsync(string imgPath)
    {
        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imgPath);
        await uwr.SendWebRequest();

        return DownloadHandlerTexture.GetContent(uwr);
    }
}
