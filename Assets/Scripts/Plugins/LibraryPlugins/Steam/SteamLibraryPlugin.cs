using Cysharp.Threading.Tasks;
using NXD.Plugins.Libraries;
using System.Collections.Generic;
using System.Threading;

namespace SteamLibraryPlugin
{
    public class SteamLibraryPlugin : LibraryPlugin
    {
        public override string Name => "Steam";

        public override string Description => "Steam";

        public override string IconPath => "steam.png";

        public override async UniTask<List<LibraryEntry>> GetEntriesAsync(CancellationToken cancellationToken)
        {
            return await SteamLocalService.GetInstalledGamesAsync(cancellationToken);
        }
    }
}
