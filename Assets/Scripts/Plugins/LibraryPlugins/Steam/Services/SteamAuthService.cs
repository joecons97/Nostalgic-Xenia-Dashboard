using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SteamLibraryPlugin
{
    public class SteamToken
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public string SteamId { get; set; }
        public string Username { get; set; }
    }

    public class SteamAuthService
    {
        private readonly AuthenticationServiceClient authenticationServiceClient = new();

        public async UniTask<BeginAuthSessionViaQRResponse> BeginLoginAsync(CancellationToken cancellationToken = default)
        {
            var beginLoginResponse = await authenticationServiceClient.BeginAuthSessionViaQRAsync(cancellationToken);

            return beginLoginResponse;
        }

        public async UniTask<SteamToken> AwaitLoginCompletionAsync(BeginAuthSessionViaQRResponse beginLoginResponse, Action<PollAuthSessionStatusResponse> onNewChallengeReceived, CancellationToken cancellationToken = default)
        {
            var tokenResponse = new SteamToken();

            while (cancellationToken.IsCancellationRequested == false)
            {
                await UniTask.WaitForSeconds(beginLoginResponse.Response.Interval, cancellationToken: cancellationToken);

                var pollResponse = await authenticationServiceClient.PollAuthSessionStatusAsync(beginLoginResponse.Response.ClientId, beginLoginResponse.Response.RequestId, cancellationToken);
                if (string.IsNullOrEmpty(pollResponse.Response.AccessToken) == false)
                {
                    tokenResponse.AccessToken = pollResponse.Response.AccessToken;
                    tokenResponse.RefreshToken = pollResponse.Response.RefreshToken;
                    tokenResponse.Username = pollResponse.Response.AccountName;

                    break;
                }
                else if (string.IsNullOrEmpty(pollResponse.Response.NewChallengeUrl) == false)
                {
                    beginLoginResponse.Response.ChallengeUrl = pollResponse.Response.NewChallengeUrl;
                    beginLoginResponse.Response.ClientId = pollResponse.Response.NewClientId;
                    onNewChallengeReceived?.Invoke(pollResponse);
                }
            }

            return tokenResponse;
        }
        
        public void SaveToken(SteamToken token)
        {
            var payload = JObject.Parse(GetPayload(token.AccessToken));
            var id = payload["sub"].Value<string>();

            token.SteamId = id;

            var json = JsonConvert.SerializeObject(token);
            PlayerPrefs.SetString("SteamToken", json);
        }

        public SteamToken LoadValidToken()
        {
            if(PlayerPrefs.HasKey("SteamToken") == false) return null;

            var json = PlayerPrefs.GetString("SteamToken");
            var token = JsonConvert.DeserializeObject<SteamToken>(json);

            var payload = JObject.Parse(GetPayload(token.AccessToken));
            var exp = payload["exp"].Value<long>();

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (now > exp)
            {
                Debug.LogWarning("Access Token is expired but no refresh implemented");
                return null;
            }

            return token;
        }

        private string GetPayload(string jwt)
        {
            var encodedPayload = jwt.Split('.')[1];

            //Convert from bs jwt base64 to actual base64
            encodedPayload = encodedPayload.Replace('-', '+').Replace('_', '/');

            // Add padding if needed
            switch (encodedPayload.Length % 4)
            {
                case 2: encodedPayload += "=="; break;
                case 3: encodedPayload += "="; break;
            }

            byte[] data = Convert.FromBase64String(encodedPayload);
            return System.Text.Encoding.UTF8.GetString(data);
        }
    }
}