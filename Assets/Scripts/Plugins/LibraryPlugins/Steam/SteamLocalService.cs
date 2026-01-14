using Cysharp.Threading.Tasks;
using NXD.Plugins.Libraries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SteamLibraryPlugin
{
    class SteamLocalService
    {
        private static ArtworkService artworkService = new();

        internal static LibraryEntry GetInstalledGameFromFile(string path)
        {
            var kv = new KeyValue();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                kv.ReadAsText(fs);
            }

            if (!string.IsNullOrEmpty(kv["StateFlags"].Value) && Enum.TryParse<AppStateFlags>(kv["StateFlags"].Value, out var appState))
            {
                if (!appState.HasFlag(AppStateFlags.FullyInstalled))
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            var name = string.Empty;
            if (string.IsNullOrEmpty(kv["name"].Value))
            {
                if (kv["UserConfig"]["name"].Value != null)
                {
                    name = kv["UserConfig"]["name"].Value;
                }
            }
            else
            {
                name = kv["name"].Value;
            }

            var gameId = kv["appID"].AsUnsignedInteger();
            var installDir = Path.Combine((new FileInfo(path)).Directory.FullName, "common", kv["installDir"].Value);
            if (!Directory.Exists(installDir))
            {
                installDir = Path.Combine((new FileInfo(path)).Directory.FullName, "music", kv["installDir"].Value);
                if (!Directory.Exists(installDir))
                {
                    installDir = string.Empty;
                }
            }

            var game = new LibraryEntry()
            {
                EntryId = gameId.ToString(),
                Name = name.Trim(),
                Path = installDir,
                IsInstalled = true
            };

            return game;
        }

        internal static List<LibraryEntry> GetInstalledGamesFromFolder(string path)
        {
            var games = new List<LibraryEntry>();

            foreach (var file in Directory.GetFiles(path, @"appmanifest*"))
            {
                if (file.EndsWith("tmp", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var game = GetInstalledGameFromFile(Path.Combine(path, file));
                    if (game == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(game.Path) || game.Path.Contains(@"steamapps\music"))
                    {
                        //logger.Info($"Steam game {game.Name} is not properly installed or it's a soundtrack, skipping.");
                        continue;
                    }

                    games.Add(game);
                }
                catch (Exception exc)
                {
                    // Steam can generate invalid acf file according to issue #37
                    //logger.Error(exc, $"Failed to get information about installed game from: {file}");
                }
            }

            return games;
        }

        internal static HashSet<string> GetLibraryFolders()
        {
            var dbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Steam.InstallationPath };
            var configPath = Path.Combine(Steam.InstallationPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(configPath))
            {
                return dbs;
            }
            try
            {
                using (var fs = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var kv = new KeyValue();
                    kv.ReadAsText(fs);
                    foreach (var dir in GetLibraryFolders(kv))
                    {
                        if (Directory.Exists(dir))
                        {
                            dbs.Add(dir);
                        }
                        else
                        {
                            //logger.Warn($"Found external Steam directory, but path doesn't exists: {dir}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //logger.Error(e, "Failed to get additional Steam library folders.");
            }
            return dbs;
        }

        internal static List<string> GetLibraryFolders(KeyValue foldersData)
        {
            var dbs = new List<string>();
            foreach (var child in foldersData.Children)
            {
                if (int.TryParse(child.Name, out int _))
                {
                    if (!string.IsNullOrEmpty(child.Value))
                    {
                        dbs.Add(child.Value);
                    }
                    else if (child.Children.Count != 0)
                    {
                        var path = child.Children.FirstOrDefault(a => a.Name?.Equals("path", StringComparison.OrdinalIgnoreCase) == true);
                        if (!string.IsNullOrEmpty(path.Value))
                        {
                            dbs.Add(path.Value);
                        }
                    }
                }
            }

            return dbs;
        }


        internal static async UniTask<List<LibraryEntry>> GetInstalledGamesAsync(CancellationToken cancellationToken, bool includeMods = true)
        {
            var games = new Dictionary<string, LibraryEntry>();
            if (!Steam.IsInstalled)
            {
                throw new Exception("Steam installation not found.");
            }

            foreach (var folder in GetLibraryFolders())
            {
                var libFolder = Path.Combine(folder, "steamapps");
                if (Directory.Exists(libFolder))
                {
                    var installedGames = GetInstalledGamesFromFolder(libFolder);
                    foreach (var a in installedGames)
                    {
                        // Ignore redist
                        if (a.EntryId == "228980")
                        {
                            continue;
                        }

                        var collection = await artworkService.GetArtworkAsync(a.EntryId, cancellationToken);
                        a.CoverImagePath = collection.Cover;
                        a.IconPath = collection.Icon;
                        a.BannerImagePath = collection.Banner;

                        if (!games.ContainsKey(a.EntryId))
                        {
                            games.Add(a.EntryId, a);
                        }
                    }
                }
                else
                {
                    //logger.Warn($"Steam library {libFolder} not found.");
                }
            }

            //if (includeMods)
            //{
            //    try
            //    {
            //        // In most cases, this will be inside the folder where Half-Life is installed.
            //        var modInstallPath = Steam.ModInstallPath;
            //        if (!string.IsNullOrEmpty(modInstallPath) && Directory.Exists(modInstallPath))
            //        {
            //            GetInstalledGoldSrcModsFromFolder(Steam.ModInstallPath).ForEach(a =>
            //            {
            //                if (!games.ContainsKey(a.GameId))
            //                {
            //                    games.Add(a.GameId, a);
            //                }
            //            });
            //        }

            //        // In most cases, this will be inside the library folder where Steam is installed.
            //        var sourceModInstallPath = Steam.SourceModInstallPath;
            //        if (!string.IsNullOrEmpty(sourceModInstallPath) && Directory.Exists(sourceModInstallPath))
            //        {
            //            GetInstalledSourceModsFromFolder(Steam.SourceModInstallPath).ForEach(a =>
            //            {
            //                if (!games.ContainsKey(a.GameId))
            //                {
            //                    games.Add(a.GameId, a);
            //                }
            //            });
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        logger.Error(e, "Failed to import Steam mods.");
            //    }
            //}

            return games
                .Select(x => x.Value)
                .ToList();
        }

        [Flags]
        private enum AppStateFlags
        {
            Invalid = 0,
            Uninstalled = 1,
            UpdateRequired = 2,
            FullyInstalled = 4,
            Encrypted = 8,
            Locked = 16,
            FilesMissing = 32,
            AppRunning = 64,
            FilesCorrupt = 128,
            UpdateRunning = 256,
            UpdatePaused = 512,
            UpdateStarted = 1024,
            Uninstalling = 2048,
            BackupRunning = 4096,
            Reconfiguring = 65536,
            Validating = 131072,
            AddingFiles = 262144,
            Preallocating = 524288,
            Downloading = 1048576,
            Staging = 2097152,
            Committing = 4194304,
            UpdateStopping = 8388608
        }
    }
}
