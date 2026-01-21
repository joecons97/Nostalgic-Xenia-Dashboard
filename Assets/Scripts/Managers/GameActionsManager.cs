using Cysharp.Threading.Tasks;
using DG.Tweening;
using LibraryPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using UnityEngine;

public class GameActionsManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] fadeCanvasGroups;
    [SerializeField] private NXEVerticalLayoutGroup layoutGroup;
    [SerializeField] private LibrariesManager libraryManager;
    [SerializeField] private DatabaseManager databaseManager;
    [SerializeField] private DashboardEntriesBuilder dashboardEntriesBuilder;

    public event Action<ObjectId> OnInstallationBegin;
    public event Action<ObjectId> OnInstallationCompleteOrCancelled;

    private bool isGameActive;
    private List<ObjectId> installingGames = new();
    private bool layoutGroupReturnEnabled;
    
    public bool IsEntryInstalling(Assets.Scripts.PersistentData.Models.LibraryEntry entry) 
        => installingGames.Contains(entry.Id);

    public void LaunchLibraryEntry(Assets.Scripts.PersistentData.Models.LibraryEntry entry)
    {
        if (isGameActive)
            return;
            
        if (entry == null)
            return;

        if (string.IsNullOrEmpty(entry.Path))
            return;

        var lib = libraryManager.Libraries.FirstOrDefault(x => x.Name == entry.Source);
        if (lib == null)
            return;

        entry.LastPlayed = DateTimeOffset.Now;
        databaseManager.LibraryEntries.Update(entry);

        var pluginEntry = new LibraryEntry()
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
                layoutGroupReturnEnabled = layoutGroup.enabled;
                layoutGroup.enabled = false;
                isGameActive = true;
            }
        });

        _ = UniTask.WaitForSeconds(2).ContinueWith(() =>
        {
            dashboardEntriesBuilder.RebuildDashboardEntry(lib.Name);
        });
    }

    private async UniTask OnEntryProcessEnded(string entryId, LibraryPlugin.LibraryPlugin plugin)
    {
        await UniTask.SwitchToMainThread();
        
        plugin.OnEntryProcessEnded -= OnEntryProcessEnded;

        isGameActive = false;
        layoutGroup.enabled = layoutGroupReturnEnabled;

        var delay = 0.0f;
        foreach (var canvas in fadeCanvasGroups.Reverse())
        {
            var fadeTo = canvas.alpha == 0 ? 1 : 0;
            canvas.DOFade(fadeTo, 0.5f).SetDelay(delay);
            delay += 0.5f;
        }
    }

    public void TryInstallLibraryEntry(Assets.Scripts.PersistentData.Models.LibraryEntry entry)
    {
        if (isGameActive)
            return;
            
        if (entry == null)
            return;

        if (string.IsNullOrEmpty(entry.Path) == false)
            return;

        if (installingGames.Contains(entry.Id))
            return;

        var lib = libraryManager.Libraries.FirstOrDefault(x => x.Name == entry.Source);
        if (lib == null)
            return;
        
        var pluginEntry = new LibraryEntry()
        {
            EntryId = entry.SourceId,
            Path = entry.Path
        };
        
        _ = lib.Plugin.TryInstallEntryAsync(pluginEntry, this.GetCancellationTokenOnDestroy()).ContinueWith(result =>
        {
            lib.Plugin.OnEntryInstallationComplete += OnEntryInstallationComplete;
            lib.Plugin.OnEntryInstallationCancelled += OnEntryInstallationCancelled;

            if (result == GameActionResult.Success)
            {
                installingGames.Add(entry.Id);
                OnInstallationBegin?.Invoke(entry.Id);
            }
            else
                OnInstallationCompleteOrCancelled?.Invoke(entry.Id);
        });
    }

    private async UniTask OnEntryInstallationCancelled(string entryId, LibraryPlugin.LibraryPlugin libraryPlugin)
    {
        var entry = databaseManager.LibraryEntries
            .Query()
            .Where(x => x.SourceId == entryId && x.Source == libraryPlugin.Name)
            .FirstOrDefault();

        if (entry == null || installingGames.Contains(entry.Id) == false)
            return;
        
        await UniTask.SwitchToMainThread();
        
        Debug.Log($"{entry.Name} Installation cancelled");
        
        installingGames.Remove(entry.Id);
        
        libraryPlugin.OnEntryInstallationCancelled -= OnEntryInstallationCancelled;
        
        _ = UniTask.WaitForEndOfFrame().ContinueWith(() =>
        {
            OnInstallationCompleteOrCancelled?.Invoke(entry.Id);
        });
    }

    private async UniTask OnEntryInstallationComplete(string entryId, string path, LibraryPlugin.LibraryPlugin libraryPlugin)
    {
        var entry = databaseManager.LibraryEntries
            .Query()
            .Where(x => x.SourceId == entryId && x.Source == libraryPlugin.Name)
            .FirstOrDefault();

        if (entry == null || installingGames.Contains(entry.Id) == false)
            return;

        await UniTask.SwitchToMainThread();
        
        Debug.Log($"{entry.Name} Installed");
        
        entry.Path = path;
        databaseManager.LibraryEntries.Update(entry);
        databaseManager.DbContext.Checkpoint();
        
        installingGames.Remove(entry.Id);
        
        libraryPlugin.OnEntryInstallationComplete -= OnEntryInstallationComplete;
        _ = UniTask.WaitForEndOfFrame().ContinueWith(() =>
        {
            OnInstallationCompleteOrCancelled?.Invoke(entry.Id);
        });
    }
}
