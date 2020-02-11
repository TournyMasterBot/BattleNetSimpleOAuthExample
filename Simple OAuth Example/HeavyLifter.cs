using BattleNet_Simple_OAuth_Example.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BattleNet_Simple_OAuth_Example
{
    public static class HeavyLifter
    {
        private static HttpClient client = new HttpClient();

        /// <summary>
        /// Battle.Net uses kind of a weird way to auth user/password for client/secret
        /// Built in .NET oauth response - perfectly acceptable to use but usually overkill for mini-apps. Microsoft.AspNetCore.Authentication.OAuth.OAuthTokenResponse
        /// </summary>
        public static async Task<Models.OAuthTokenResponse> BlizzardOAuthApplicationCredentials(string tokenEndpoint, string clientID, string clientSecret, HttpContext context)
        {
            string authInfo = clientID + ":" + clientSecret;
            var request = WebRequest.Create($"{tokenEndpoint}?grant_type=client_credentials");
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = $"Basic {authInfo}";
            string data = null;
            try
            {
                var response = await request.GetResponseAsync();
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        data = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //Lazy way, it's better to inspect data for an actual valid object
            Models.OAuthTokenResponse token = null;
            try
            {
                token = JsonConvert.DeserializeObject<Models.OAuthTokenResponse>(data);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return token;
        }

        public static async Task<Models.OAuthTokenResponse> BlizzardOAuthTokenRequest(string tokenEndpoint, string clientID, string clientSecret, string redirectUri, string scopes, string code, string state, HttpContext context)
        {
            client.DefaultRequestHeaders.Clear();

            string authInfo = clientID + ":" + clientSecret;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

            var values = new Dictionary<string, string>
            {                
                { "redirect_uri", redirectUri },
                { "scope", scopes },
                { "grant_type", "authorization_code" },
                { "code", code },
            };

            var content = new FormUrlEncodedContent(values);

            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authInfo}");
            var response = await client.PostAsync(tokenEndpoint, content);

            string data = null;
            try
            {
                data = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //Lazy way, it's better to inspect data for an actual valid object
            Models.OAuthTokenResponse token = null;
            try
            {
                token = JsonConvert.DeserializeObject<Models.OAuthTokenResponse>(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return token;
        }


        public static async Task<string> GetDataAsync<T>(string url, string clientID, int timeout = 86400, string bearer = null, HttpContext context = null)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.AllowAutoRedirect = true;
                httpClientHandler.MaxAutomaticRedirections = 3;
                httpClientHandler.AutomaticDecompression = System.Net.DecompressionMethods.GZip;

                if (url.StartsWith("https://"))
                {
                    httpClientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    // The line below disables SSL chain validation - this should be a debug item only
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    // The line above disables SSL chain validation
                }

                using (var client = new HttpClient(httpClientHandler))
                {
                    if (context != null)
                    {
                        foreach (var header in context.Request.Headers)
                        {
                            try
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, (string)header.Value);
                            }
                            catch (Exception) { }
                        }
                    }
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!string.IsNullOrWhiteSpace(clientID))
                    {
                        client.DefaultRequestHeaders.Add("ClientID", clientID);
                    }
                    if (!string.IsNullOrWhiteSpace(bearer))
                    {
                        client.DefaultRequestHeaders.Add("Bearer", bearer);
                    }
                    var response = await client.GetAsync(url);
                    try
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                    catch (WebException ex)
                    {
                        using (var exResponse = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            return exResponse.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }
            }
        }

        public static async Task<string> PostDataAsync<T>(string url, T data, string clientID, int timeout = 86400, string bearer = null, HttpContext context = null)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.AllowAutoRedirect = true;
                httpClientHandler.MaxAutomaticRedirections = 3;
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip;

                if (url.StartsWith("https://"))
                {
                    httpClientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    // The line below disables SSL chain validation - this should be a debug item only
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    // The line above disables SSL chain validation
                }
                using (var client = new HttpClient(httpClientHandler))
                {
                    if (context != null)
                    {
                        foreach (var header in context.Request.Headers)
                        {
                            try
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, (string)header.Value);
                            }
                            catch (Exception) { }
                        }
                    }
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (!string.IsNullOrWhiteSpace(clientID))
                    {
                        client.DefaultRequestHeaders.Add("ClientID", clientID);
                    }
                    if (!string.IsNullOrWhiteSpace(bearer))
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearer}");
                    }
                    try
                    {
                        var response = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));

                        var result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                    catch (WebException ex)
                    {
                        using (var exResponse = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            return exResponse.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }
            }
        }
    }
}
