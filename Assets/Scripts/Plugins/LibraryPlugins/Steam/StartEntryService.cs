using Cysharp.Threading.Tasks;
using LibraryPlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SteamLibraryPlugin
{
    public class StartEntryService
    {
        private HashSet<string> _cachedExecutables;

        public GameActionResult StartEntry(SteamLibraryPlugin plugin, LibraryEntry entry, CancellationToken cancellationToken)
        {
            _cachedExecutables = null;
            Process.Start(new ProcessStartInfo
            {
                FileName = $"steam://rungameid/{entry.EntryId}",
                UseShellExecute = true
            });

            _ = UniTask.RunOnThreadPool(async () =>
            {
                await MonitorGameDirectoryAsync(plugin, entry, cancellationToken);
            }, cancellationToken: cancellationToken);

            return GameActionResult.Success;
        }

        private async UniTask MonitorGameDirectoryAsync(SteamLibraryPlugin plugin, LibraryEntry entry, CancellationToken cancellationToken)
        {
            var installDir = entry.Path;
            if (string.IsNullOrEmpty(installDir))
            {
                if(plugin.OnEntryProcessEnded != null)
                    await plugin.OnEntryProcessEnded(entry.EntryId, plugin);
                
                return;
            }

            Debug.Log($"Monitoring {installDir}");

            // Wait for game to start (with polling interval)
            while (!HasProcessesInDirectory(installDir))
            {
                await UniTask.Delay(1000, cancellationToken: cancellationToken); // Check every second
                if (cancellationToken.IsCancellationRequested) return;
            }

            Debug.Log($"Game Started");

            //Wait a couple of seconds incase the game launched from a low launcher (like skyrim)
            await UniTask.Delay(2000, cancellationToken: cancellationToken);

            // Monitor until all processes close (with polling interval)
            while (HasProcessesInDirectory(installDir))
            {
                await UniTask.Delay(1000, cancellationToken: cancellationToken); // Check every second
                if (cancellationToken.IsCancellationRequested) return;
            }

            Debug.Log($"Finished Monitoring {installDir}");

            try
            {
                if(plugin.OnEntryProcessEnded != null)
                    await plugin.OnEntryProcessEnded(entry.EntryId, plugin);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in OnEntryProcessEnded: entry.EntryId: {entry?.EntryId}, Plugin: {plugin}");
                Debug.LogException(ex);
            }
        }

        private bool HasProcessesInDirectory(string gameDirectory)
        {
            // Get known executable names from the directory
            var knownExes = GetKnownExecutables(gameDirectory);

            foreach (var exeName in knownExes)
            {
                UnityEngine.Debug.Log($"Checking if {exeName} is running");
                try
                {
                    // This is MUCH faster than GetProcesses() + MainModule
                    var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeName));

                    foreach (var process in processes)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                string processPath = process.MainModule?.FileName;
                                if (!string.IsNullOrEmpty(processPath))
                                {
                                    string processDir = Path.GetDirectoryName(processPath);
                                    if (processDir != null &&
                                        processDir.StartsWith(gameDirectory, System.StringComparison.OrdinalIgnoreCase))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }

        private HashSet<string> GetKnownExecutables(string gameDirectory)
        {
            if (_cachedExecutables != null)
                return _cachedExecutables;

            _cachedExecutables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var exeFiles = Directory.GetFiles(gameDirectory, "*.exe", SearchOption.AllDirectories);
                foreach (var exe in exeFiles)
                {
                    _cachedExecutables.Add(Path.GetFileName(exe));
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error scanning directory: {ex.Message}");
            }

            return _cachedExecutables;
        }
    }
}
