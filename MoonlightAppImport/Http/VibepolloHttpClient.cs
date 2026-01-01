using MoonlightAppImport.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace MoonlightAppImport.Http
{
    public class VibepolloHttpClient : SunshineHttpClient
    {
        public VibepolloHttpClient(MoonlightAppImportSettings moonlightAppImportSettings) : base(moonlightAppImportSettings)
        {
        }

        public override async Task<MoonlightApps> GetGamesAsync()
        {
            bool authenticated = AuthenticateWithBearerToken();
            if (!authenticated)
                throw new AuthenticationException("Could not authenticate with the provided API key!");

            return await GetGamesInternalAsync();
        }

        protected override void CheckSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.SunshineHost))
                throw new ArgumentNullException(nameof(_settings.SunshineHost));

            if (string.IsNullOrWhiteSpace(_settings.MoonlightPath))
                throw new ArgumentNullException(nameof(_settings.MoonlightPath));

            if (string.IsNullOrWhiteSpace(_settings.VibepolloApiKey))
                throw new ArgumentNullException(nameof(_settings.VibepolloApiKey));

            bool result = IPValidator.ValidateAndResolve(_settings.SunshineHost);
            if (!result)
                throw new ArgumentException("The sunshine host address was invalid!");

            result = File.Exists(_settings.MoonlightPath) && Path.GetFileName(_settings.MoonlightPath).Equals("Moonlight.exe", StringComparison.OrdinalIgnoreCase);
            if (!result)
                throw new ArgumentException("The moonlight path was invalid! Must point to a \"Moonlight.exe\".");

            _logger.Info($"Settings are valid:\nHost = {_settings.SunshineHost}\nMoonlight Path = {_settings.MoonlightPath}\nApi Key = An API key with a length of... {_settings.VibepolloApiKey.Length}\nSkip Certificate Validation = {_settings.SkipCertificateValidation}");
        }

        private bool AuthenticateWithBearerToken()
        {
            _logger.Info($"Starting to authenticate to Vibepollo using bearer token authentication.\nHost = {_settings.SunshineHost}\nApiKey = An API key with {_settings.VibepolloApiKey.Length} length...");

            if (string.IsNullOrEmpty(_settings.VibepolloApiKey))
            {
                _logger.Error("Vibepollo API Key is not set.");
                return false;
            }

            // Clear any existing authentication headers
            _sunshinePrivateApiClient.DefaultRequestHeaders.Authorization = null;

            // Set up bearer token authentication
            _sunshinePrivateApiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.VibepolloApiKey);

            _logger.Info("Successfully authenticated to Vibepollo using bearer token authentication.");
            return true;
        }
    }
}