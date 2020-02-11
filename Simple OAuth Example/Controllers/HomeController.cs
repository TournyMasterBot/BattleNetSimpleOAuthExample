using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BattleNet_Simple_OAuth_Example.Models;
using BattleNet_Simple_OAuth_Example;
using System.Web;
using System.Text;
using Newtonsoft.Json;

namespace Simple_OAuth_Example.Controllers
{
    public class HomeController : Controller
    {        
        // This is NOT a safe way to do this in a real app. This needs to be LOCALIZED TO THE USER -- This is for DEMO CONVENIENCE ONLY
        private static string tokenEndpoint { get; set; }
        private static string authorizationEndpoint { get; set; }
        private static string apiEndpoint { get; set; }
        private static string redirectUri { get; set; }
        private static string clientID { get; set; }
        private static string clientSecret { get; set; }
        private static string scopes { get; set; }
        private static string state { get; set; }
        private static string user { get; set; }

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("request-token-auth")]
        [ValidateAntiForgeryToken()]
        public void RequestTokenAuth(IFormCollection form)
        {
            if (!form.ContainsKey("token-endpoint") || string.IsNullOrEmpty(form["token-endpoint"]))
            {
                throw new ArgumentNullException("token-endpoint");
            }
            if (!form.ContainsKey("authorization-endpoint") || string.IsNullOrEmpty(form["authorization-endpoint"]))
            {
                throw new ArgumentNullException("authorization-endpoint");
            }
            if (!form.ContainsKey("api-endpoint") || string.IsNullOrEmpty(form["api-endpoint"]))
            {
                throw new ArgumentNullException("api-endpoint");
            }
            if (!form.ContainsKey("client-id") || string.IsNullOrEmpty(form["client-id"]))
            {
                throw new ArgumentNullException("client-id");
            }
            if (!form.ContainsKey("client-secret") || string.IsNullOrEmpty(form["client-secret"]))
            {
                throw new ArgumentNullException("client-secret");
            }
            if (!form.ContainsKey("redirect-uri") || string.IsNullOrEmpty(form["redirect-uri"]))
            {
                throw new ArgumentNullException("redirect-uri");
            }
            if (!form.ContainsKey("scopes") /* Empty scope is OK, no error there. */)
            {
                throw new ArgumentNullException("scopes");
            }
            if(!form.ContainsKey("user") || string.IsNullOrWhiteSpace(form["user"]))
            {
                throw new ArgumentNullException("user");
            }

            tokenEndpoint = form["token-endpoint"];
            authorizationEndpoint = form["authorization-endpoint"];
            apiEndpoint = form["api-endpoint"];
            clientID = form["client-id"];
            clientSecret = form["client-secret"];
            redirectUri = form["redirect-uri"];
            scopes = form["scopes"];
            user = form["user"];
            state = Guid.NewGuid().ToString("d");

            // Build URL that we will redirect the users browser to -- this goes to BLIZZARD website to allow user to authorize scope requests.
            var url = $"{authorizationEndpoint}?client_id={clientID}&scope={scopes}&state={state}&redirect_uri={redirectUri}&response_type=code";
            Response.Redirect(url);
        }

        /// <summary>
        /// This is called by BLIZZARD as part of the redirect uri - they pass 'code' which can be used in subsequent requests 
        /// </summary>
        [HttpGet("blizzard-callback")]
        public async Task<JsonResult> BlizzardOAuthCallback(string code)
        {            
            var builder = new StringBuilder();

            var clientCredentials = await HeavyLifter.BlizzardOAuthApplicationCredentials(tokenEndpoint, clientID, clientSecret, HttpContext);
            builder.AppendLine("Client Credentials");
            builder.AppendLine(JsonConvert.SerializeObject(clientCredentials));
            
            var authorizationResult = await HeavyLifter.BlizzardOAuthTokenRequest(tokenEndpoint, clientID, clientSecret, redirectUri, scopes, code, state, HttpContext);
            builder.AppendLine("Authorization Result");
            builder.AppendLine(JsonConvert.SerializeObject(authorizationResult));

            // WIP
            //builder.AppendLine("Api User Result");
            //var url = $"{apiEndpoint}?sc2/metadata/profile/1/1/{user}?access_token={authorizationResult.AccessToken}&locale=en_US";
            //var apiResult = await HeavyLifter.GetDataAsync<string>(url, clientID, bearer: clientCredentials.AccessToken);
            //builder.AppendLine(apiResult);
            return new JsonResult(builder);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
