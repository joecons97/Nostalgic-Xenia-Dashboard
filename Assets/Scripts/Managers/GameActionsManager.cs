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
    [SerializeField] private DatabaseManager databaseManager;
    [SerializeField] private DashboardEntriesBuilder dashboardEntriesBuilder;


    private int activeEntries;

    public void LaunchLibraryEntry(Assets.Scripts.PersistentData.Models.LibraryEntry entry)
    {
        if (entry == null)
            return;

        var lib = libraryManager.Libraries.FirstOrDefault(x => x.Name == entry.Source);
        if (lib == null)
            return;

        entry.LastPlayed = DateTimeOffset.Now;
        databaseManager.LibraryEntries.Update(entry);

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

        _ = UniTask.WaitForSeconds(2).ContinueWith(() =>
        {
            dashboardEntriesBuilder.RebuildDashboardEntry(lib.Name);
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
