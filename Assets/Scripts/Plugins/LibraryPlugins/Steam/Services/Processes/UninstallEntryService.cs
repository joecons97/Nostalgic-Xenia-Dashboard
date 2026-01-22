using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LibraryPlugin;

namespace SteamLibraryPlugin
{
    public class UninstallEntryService
    {
        public GameActionResult UninstallEntry(SteamLibraryPlugin plugin, LibraryEntry entry, CancellationToken cancellationToken)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Steam.ClientExecPath,
                    Arguments = $"-silent \"steam://uninstall/{entry.EntryId}\"",
                    UseShellExecute = true
                });

                _ = UniTask.RunOnThreadPool(async () => { await MonitorGameUninstallation(plugin, entry, cancellationToken); }, cancellationToken: cancellationToken);

                return GameActionResult.Success;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                return GameActionResult.Fail;
            }
        }

        private async UniTask MonitorGameUninstallation(SteamLibraryPlugin plugin, LibraryEntry entry, CancellationToken cancellationToken)
        {
            LibraryEntry game;
            while (TryGetGame(entry.EntryId, out game))
            {
                await UniTask.Delay(1000);

                if (cancellationToken.IsCancellationRequested)
                {
                    if(plugin.OnEntryUninstallationCancelled != null)
                        await plugin.OnEntryUninstallationCancelled(entry.EntryId, plugin);
                    
                    return;
                }
            }
            
            if(plugin.OnEntryUninstallationComplete != null)
                await plugin.OnEntryUninstallationComplete(entry.EntryId, plugin);
        }

        private bool TryGetGame(string entryId, out LibraryEntry game)
        {
            game = SteamLocalService.GetInstalledGamesAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult()
                .FirstOrDefault(x => x.EntryId == entryId);

            if (game == null)
            {
                return false;
            }

            return true;
        }
    }
}