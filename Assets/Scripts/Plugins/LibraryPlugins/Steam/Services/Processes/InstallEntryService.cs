using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LibraryPlugin;

namespace SteamLibraryPlugin
{
    public class InstallEntryService
    {
        public GameActionResult InstallEntry(SteamLibraryPlugin plugin, LibraryEntry entry, CancellationToken cancellationToken)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Steam.ClientExecPath,
                Arguments = $"-silent \"steam://install/{entry.EntryId}\"",
                UseShellExecute = true
            });
            
            _ = UniTask.RunOnThreadPool(async () =>
            {
                await MonitorGameInstallation(plugin, entry, cancellationToken);
            }, cancellationToken: cancellationToken);
            
            return GameActionResult.Success;
        }

        private async UniTask MonitorGameInstallation(SteamLibraryPlugin plugin, LibraryEntry entry, CancellationToken cancellationToken)
        {
            LibraryEntry game;
            while (TryGetGame(entry.EntryId, out game) == false)
            {
                await UniTask.Delay(1000);

                if (cancellationToken.IsCancellationRequested)
                {
                    if(plugin.OnEntryInstallationCancelled != null)
                        await plugin.OnEntryInstallationCancelled(entry.EntryId, plugin);
                    
                    return;
                }
            }
            
            if(plugin.OnEntryInstallationComplete != null)
                await plugin.OnEntryInstallationComplete(entry.EntryId, game.Path, plugin);
        }

        private bool TryGetGame(string entryId, out LibraryEntry game)
        {
            game = SteamLocalService.GetInstalledGamesAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult()
                .FirstOrDefault(x => x.EntryId == entryId);

            if (game == null)
            {
                var installing  = SteamLocalService.GetLibraryUpdateProgress(CancellationToken.None)
                    .FirstOrDefault(x => x.EntryId == entryId);
                
                if (installing != null)
                    UnityEngine.Debug.Log($"{installing.Progress}%");
                
                return false;
            }

            
            return true;
        }
    }
}