using Cysharp.Threading.Tasks;
using LibraryPlugin;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SteamLibraryPlugin
{

    public class SteamArtworkService
    {
        private readonly string[] IMAGE_URLS = new string[]
        {
            "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/library_600x900.jpg",
            "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/library_hero.jpg",
            "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/capsule_231x87.jpg",
        };

        public async UniTask<ArtworkCollection> GetValidSteamArtAsync(string appId, CancellationToken cancellationToken = default)
        {
            var cover = string.Format(IMAGE_URLS[0], appId);
            var banner = string.Format(IMAGE_URLS[1], appId);
            var icon = string.Format(IMAGE_URLS[2], appId);

            (bool isCoverValid, bool isBannerValid, bool isIconValid) results = await UniTask.WhenAll(
                IsValidSteamImageAsync(cover, cancellationToken),
                IsValidSteamImageAsync(banner, cancellationToken),
                IsValidSteamImageAsync(icon, cancellationToken));

            return new ArtworkCollection
            {
                Cover = results.isCoverValid ? cover : "",
                Banner = results.isBannerValid ? banner : "",
                Icon = results.isIconValid ? icon : "",
            };
        }

        private async UniTask<bool> IsValidSteamImageAsync(string url, CancellationToken cancellationToken = default)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);

            Debug.Log($"GET {url}");
            
            // Only download first 50 KB
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Range", "bytes=0-51200");

            try
            {
                await request.SendWebRequest().WithCancellation(cancellationToken);

                byte[] data = request.downloadHandler.data;

                // Try to load partial image
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(data))
                {
                    bool isValid = !IsPlaceholderImage(texture);
                    return isValid;
                }
                else
                {
                    return true;
                }
            }
            catch(UnityWebRequestException)
            {
                return false;
            }
        }

        private bool IsPlaceholderImage(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            if (pixels.Length == 0) return true;

            float variance = 0;
            int sampleSize = Mathf.Min(1000, pixels.Length);
            int step = Mathf.Max(1, pixels.Length / sampleSize);

            long sumR = 0, sumG = 0, sumB = 0;
            for (int i = 0; i < pixels.Length; i += step)
            {
                sumR += pixels[i].r;
                sumG += pixels[i].g;
                sumB += pixels[i].b;
            }

            float avgR = sumR / (float)sampleSize;
            float avgG = sumG / (float)sampleSize;
            float avgB = sumB / (float)sampleSize;

            for (int i = 0; i < pixels.Length; i += step)
            {
                variance += Mathf.Pow(pixels[i].r - avgR, 2);
                variance += Mathf.Pow(pixels[i].g - avgG, 2);
                variance += Mathf.Pow(pixels[i].b - avgB, 2);
            }

            variance /= sampleSize * 3;
            return variance < 100f;
        }
    }
}
