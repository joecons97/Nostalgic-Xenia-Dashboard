using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace SteamLibraryPlugin
{
    public class AuthenticationServiceClient
    {
        public async UniTask<BeginAuthSessionViaQRResponse> BeginAuthSessionViaQRAsync(CancellationToken cancellationToken = default)
        {
            WWWForm form = new WWWForm();
            form.AddField("device_friendly_name", SystemInfo.deviceName);
            form.AddField("website_id", Application.productName);
            
            using UnityWebRequest request = UnityWebRequest.Post("https://api.steampowered.com/IAuthenticationService/BeginAuthSessionViaQR/v1/", form);
            await request.SendWebRequest().WithCancellation(cancellationToken);

            var json = request.downloadHandler.text;
            Debug.Log(request.downloadHandler.text);
            var response = JsonConvert.DeserializeObject<BeginAuthSessionViaQRResponse>(json);

            return response;
        }

        public async UniTask<PollAuthSessionStatusResponse> PollAuthSessionStatusAsync(string clientId, string requestId, CancellationToken cancellationToken = default)
        {
            WWWForm form = new WWWForm();
            form.AddField("client_id", clientId);
            form.AddField("request_id", requestId);
            using UnityWebRequest request = UnityWebRequest.Post("https://api.steampowered.com/IAuthenticationService/PollAuthSessionStatus/v1/", form);
            await request.SendWebRequest().WithCancellation(cancellationToken);

            var json = request.downloadHandler.text;
            Debug.Log(request.downloadHandler.text);
            
            var response = JsonConvert.DeserializeObject<PollAuthSessionStatusResponse>(json);

            return response;
        }
    }
}