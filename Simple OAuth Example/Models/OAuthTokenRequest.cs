using Newtonsoft.Json;
using System.Collections.Generic;

namespace BattleNet_Simple_OAuth_Example.Models
{
    public class OAuthTokenRequest
    {
        [JsonProperty(PropertyName = "redirect_uri")]
        public string RedirectUri { get; set; }
        [JsonProperty(PropertyName = "grant_type")]
        public string GrantType { get; set; }
        [JsonProperty(PropertyName = "scope")]
        public string Scopes { get; set; }
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }
    }
}
