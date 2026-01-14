using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LibraryPlugin;
using System;
using System.Linq;
using UnityEngine;

public class GameActionsManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] fadeCanvasGroups;
    [SerializeField] private NXEVerticalLayoutGroup layoutGroup;
    [SerializeField] private LibrariesManager libraryManager;

    private int activeEntries;

    public void LaunchLibraryEntry(Assets.Scripts.PersistentData.Models.LibraryEntry entry)
    {
        if (entry == null)
            return;

        var lib = libraryManager.Libraries.FirstOrDefault(x => x.Name == entry.Source);
        if (lib == null)
            return;

        var pluginEntry = new LibraryPlugin.LibraryEntry()
        {
            EntryId = entry.SourceId,
            Path = entry.Path
        };

        var delay = 0.0f;
        foreach(var canvas in fadeCanvasGroups)
        {
            var fadeTo = canvas.alpha == 0 ? 1 : 0;
            canvas.DOFade(fadeTo, 0.5f).SetDelay(delay);
            delay += 0.5f;
        }

        lib.Plugin.OnEntryProcessEnded += OnEntryProcessEnded;

        _ = lib.Plugin.TryStartEntryAsync(pluginEntry, this.GetCancellationTokenOnDestroy()).ContinueWith(result =>
        {
            Debug.Log(result);
            if (result == GameActionResult.Success)
            {
                layoutGroup.enabled = false;
                activeEntries++;
            }
        });
    }

    private void OnEntryProcessEnded(string entryId, LibraryPlugin.LibraryPlugin plugin)
    {
        plugin.OnEntryProcessEnded -= OnEntryProcessEnded;

        activeEntries--;
        if(activeEntries <= 0)
        {
            activeEntries = 0;
            layoutGroup.enabled = true;
        }

        var delay = 0.0f;
        foreach (var canvas in fadeCanvasGroups.Reverse())
        {
            var fadeTo = canvas.alpha == 0 ? 1 : 0;
            canvas.DOFade(fadeTo, 0.5f).SetDelay(delay);
            delay += 0.5f;
        }
    }
}
