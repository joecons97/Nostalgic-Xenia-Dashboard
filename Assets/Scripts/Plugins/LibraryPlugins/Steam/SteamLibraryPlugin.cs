using Cysharp.Threading.Tasks;
using LibraryPlugin;
using System.Collections.Generic;
using System.Threading;

namespace SteamLibraryPlugin
{
    public class SteamLibraryPlugin : LibraryPlugin.LibraryPlugin
    {
        public override string Name => "Steam";

        public override string Description => "Steam";

        public override string IconPath => "steam.png";

        private ArtworkService artworkService = new();

        public override async UniTask<ArtworkCollection> GetArtworkCollection(string entryId, CancellationToken cancellationToken)
        {
            var collection = await artworkService.GetArtworkAsync(entryId, cancellationToken);

            return collection;
        }

        public override async UniTask<List<LibraryEntry>> GetEntriesAsync(CancellationToken cancellationToken)
        {
            return await SteamLocalService.GetInstalledGamesAsync(cancellationToken);
        }
    }
}
