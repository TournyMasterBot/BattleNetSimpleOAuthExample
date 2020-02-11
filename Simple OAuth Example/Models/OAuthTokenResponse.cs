using Newtonsoft.Json;
using System.Collections.Generic;

namespace BattleNet_Simple_OAuth_Example.Models
{
    public class OAuthTokenResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }
        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty(PropertyName = "scope")]
        public string Scopes { get; set; }
    }
}
