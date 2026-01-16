using System.Collections.Generic;
using Newtonsoft.Json;

namespace SteamLibraryPlugin
{
    public class BeginAuthSessionViaQRResponse
    {
        [JsonProperty("response")]
        public ResponseData Response { get; set; }

        public class AllowedConfirmation
        {
            [JsonProperty("confirmation_type")]
            public int ConfirmationType { get; set; }
        }

        public class ResponseData
        {
            [JsonProperty("client_id")]
            public string ClientId { get; set; }

            [JsonProperty("challenge_url")]
            public string ChallengeUrl { get; set; }

            [JsonProperty("request_id")]
            public string RequestId { get; set; }

            [JsonProperty("interval")]
            public int Interval { get; set; }

            [JsonProperty("allowed_confirmations")]
            public List<AllowedConfirmation> AllowedConfirmations { get; set; }

            [JsonProperty("version")]
            public int Version { get; set; }
        }
    }
}