using NXD.Plugins.Libraries;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Library
{
    public string Name { get; }
    public string Description { get; }
    public string IconPath { get; }
    public LibraryEntry[] Entries { get; }
    private Sprite icon;

    public Library(string name, string description, string iconPath, LibraryEntry[] entries)
    {
        Name = name;
        Description = description;
        IconPath = iconPath;
        Entries = entries;
    }

    public IEnumerator GetIconLazy(Action<Sprite> onComplete)
    {
        if (icon != null)
        {
            onComplete?.Invoke(icon);
            yield break;
        }
        else
        {
            Debug.Log($"Loading icon from path: {IconPath}");
            using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(IconPath);
            yield return uwr.SendWebRequest();

            var tex = DownloadHandlerTexture.GetContent(uwr);

            icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            onComplete?.Invoke(icon);
        }
    }
}