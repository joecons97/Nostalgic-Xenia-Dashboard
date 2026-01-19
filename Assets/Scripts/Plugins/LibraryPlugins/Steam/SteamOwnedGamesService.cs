using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LibraryPlugin;

namespace SteamLibraryPlugin
{
    public class SteamOwnedGamesService
    {
        private SteamAuthService steamAuthService;
        private PlayerServiceClient playerServiceClient = new();

        public SteamOwnedGamesService(SteamAuthService steamAuthService)
        {
            this.steamAuthService = steamAuthService;
        }

        public async UniTask<List<LibraryEntry>> GetOwnedGamesAsync(CancellationToken cancellationToken)
        {
            var result = await playerServiceClient.GetOwnedGames(steamAuthService.LoadValidToken(), true, cancellationToken);

            return result.Response.Games.Select(x => new LibraryEntry() { Name = x.Name, EntryId = x.AppId.ToString() }).ToList();
        }
    }
}