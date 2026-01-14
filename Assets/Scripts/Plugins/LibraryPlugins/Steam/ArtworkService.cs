using Cysharp.Threading.Tasks;
using System.Threading;

namespace SteamLibraryPlugin
{
    public class ArtworkService
    {
        SteamArtworkService steamArtworkService = new();
        FallbackArtworkService fallbackArtworkService = new();

        public async UniTask<ArtworkCollection> GetArtworkAsync(string appId, CancellationToken cancellationToken = default)
        {
            var steamArt = await steamArtworkService.GetValidSteamArtAsync(appId, cancellationToken);
            if(steamArt.IsComplete == false)
                steamArt = await fallbackArtworkService.FillMissingArtworkAsync(steamArt, appId, cancellationToken);


            return steamArt;
        }
    }
}
