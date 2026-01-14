using Cysharp.Threading.Tasks;
using NXD.Plugins.Libraries;
using UnityEngine;
using UnityEngine.Networking;

public class Library
{
    public string Name { get; }
    public string Description { get; }
    public string IconPath { get; }
    public string AssemblyPath { get; }
    public LibraryPlugin Plugin { get; }
    private Sprite icon;

    public Library(string name, string description, string iconPath, string assemblyPath, LibraryPlugin libraryPlugin)
    {
        Name = name;
        Description = description;
        IconPath = iconPath;
        Plugin = libraryPlugin;
        AssemblyPath = assemblyPath;
    }

    public async UniTask<Sprite> GetIconAsync()
    {
        if (icon != null)
        {
            return icon;
        }
        else
        {
            Debug.Log($"Loading icon from path: {IconPath}");
            using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(IconPath);
            await uwr.SendWebRequest();

            var tex = DownloadHandlerTexture.GetContent(uwr);

            icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            return icon;
        }
    }
}