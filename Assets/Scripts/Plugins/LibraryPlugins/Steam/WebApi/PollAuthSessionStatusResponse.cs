using Newtonsoft.Json;

namespace SteamLibraryPlugin
{
    public class PollAuthSessionStatusResponse
    {
        [JsonProperty("response")]
        public ResponseData Response { get; set; }
    
    
        public class ResponseData
        {
            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("had_remote_interaction")]
            public bool HadRemoteInteraction { get; set; }

            [JsonProperty("account_name")]
            public string AccountName { get; set; }

            [JsonProperty("new_client_id")]
            public string NewClientId { get; set; }
            
            [JsonProperty("new_challenge_url")]
            public string NewChallengeUrl { get; set; }
        }

    }
}