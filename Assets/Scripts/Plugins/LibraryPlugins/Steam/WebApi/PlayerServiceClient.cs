using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace SteamLibraryPlugin
{
    public class PlayerServiceClient
    {
        public async UniTask<GetOwnedGamesResponse> GetOwnedGames(SteamToken token, bool includeAppInfo, CancellationToken cancellationToken = default)
        {
            if(token == null)
                return new();
            
            using UnityWebRequest request = UnityWebRequest.Get("https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?access_token=" + token.AccessToken + "&steamid=" + token.SteamId + "&include_appinfo=" + includeAppInfo);
            await request.SendWebRequest().WithCancellation(cancellationToken);

            var json = request.downloadHandler.text;
            var response = JsonConvert.DeserializeObject<GetOwnedGamesResponse>(json);

            return response;
        }
    }
}