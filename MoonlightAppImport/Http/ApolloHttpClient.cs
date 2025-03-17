using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using MoonlightAppImport.Models;
using Newtonsoft.Json;

namespace MoonlightAppImport.Http
{
    public class ApolloHttpClient : SunshineHttpClient
    {
        public ApolloHttpClient(MoonlightAppImportSettings moonlightAppImportSettings) : base(moonlightAppImportSettings)
        {
        }

        public async override Task<MoonlightApps> GetGamesAsync()
        {
            bool authenticated = await AuthenticateWithCookieAuthAsync();
            if (!authenticated)
                throw new AuthenticationException("Could not authenticate with the provided username & password!");

            return await GetGamesInternalAsync();
        }

        private async Task<bool> AuthenticateWithCookieAuthAsync()
        {
            // Clear any existing authentication headers
            _sunshinePrivateApiClient.DefaultRequestHeaders.Authorization = null;

            // Create login form content
            var loginData = new
            {
                username = _settings.SunshineUsername,
                password = _settings.SunshinePassword
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

            // Post to login endpoint
            var response = await _sunshinePrivateApiClient.PostAsync("/api/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // Cookies are automatically handled by the CookieContainer
                return true;
            }

            return false;
        }
    }
}
