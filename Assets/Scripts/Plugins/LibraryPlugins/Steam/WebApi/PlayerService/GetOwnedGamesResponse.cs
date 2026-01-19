using Newtonsoft.Json;

namespace SteamLibraryPlugin
{
    public class GetOwnedGamesResponse
    {
        [JsonProperty("response")]
        public ResponseData Response { get; set; }

        public class ResponseData
        {
            [JsonProperty("game_count")]
            public int GameCount { get; set; }

            [JsonProperty("games")]
            public Game[] Games { get; set; }
        }
        
        public class Game
        {
            [JsonProperty("appid")]
            public int AppId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}