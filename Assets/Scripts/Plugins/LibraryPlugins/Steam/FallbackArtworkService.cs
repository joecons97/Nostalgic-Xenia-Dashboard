using Cysharp.Threading.Tasks;
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

            if (string.IsNullOrEmpty(collection.Cover))
            {
                collection.Cover = await GetFirstResultAsync(COVER_URL, appId, cancellationToken);
            }

            if (string.IsNullOrEmpty(collection.Banner))
            {
                collection.Banner = await GetFirstResultAsync(BANNER_URL, appId, cancellationToken);
            }

            if (string.IsNullOrEmpty(collection.Icon))
            {
                collection.Icon = await GetFirstResultAsync(ICON_URL, appId, cancellationToken);
            }

            return collection;
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
