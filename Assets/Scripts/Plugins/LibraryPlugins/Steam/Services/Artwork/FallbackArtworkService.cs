using Cysharp.Threading.Tasks;
using LibraryPlugin;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace SteamLibraryPlugin
{
    public class FallbackArtworkService
    {
        private const string KEY = "eac60eb72325b5e1fc6a16466f0a041c";
        private const string COVER_URL = "https://www.steamgriddb.com/api/v2/grids/steam/{0}";
        private const string BANNER_URL = "https://www.steamgriddb.com/api/v2/heroes/steam/{0}";
        private const string ICON_URL = "https://www.steamgriddb.com/api/v2/icons/steam/{0}";

        public async UniTask<ArtworkCollection> FillMissingArtworkAsync(ArtworkCollection collection, string appId, CancellationToken cancellationToken = default)
        {
            if (collection.IsComplete)
                return collection;

            var (cover, banner, icon) = await UniTask.WhenAll(
                GetArtworkAsync(collection.Cover, COVER_URL, appId, cancellationToken),
                GetArtworkAsync(collection.Banner, BANNER_URL, appId, cancellationToken),
                GetArtworkAsync(collection.Icon, ICON_URL, appId, cancellationToken)
            );

            collection.Cover = cover;
            collection.Banner = banner;
            collection.Icon = icon;

            return collection;
        }

        private async UniTask<string> GetArtworkAsync(string originalUrl, string url, string appId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(originalUrl) == false)
                return originalUrl;

            return await GetFirstResultAsync(url, appId, cancellationToken);
        }

        private async UniTask<string> GetFirstResultAsync(string url, string appId, CancellationToken cancellationToken = default)
        {
            url = string.Format(url, appId);

            using UnityWebRequest request = UnityWebRequest.Get(url);

            Debug.Log($"GET {url}");

            request.SetRequestHeader("Authorization", $"Bearer {KEY}");
            await request.SendWebRequest();

            try
            {
                var jObject = JObject.Parse(request.downloadHandler.text);
                if (jObject["data"] is not JArray dataArray)
                    return "";

                var first = dataArray[0];
                var firstUrl = first["url"];

                return firstUrl.Value<string>();
            }
            catch (UnityWebRequestException)
            {
                return "";
            }
        }
    }
}
