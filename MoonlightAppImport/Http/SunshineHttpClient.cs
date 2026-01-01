using MoonlightAppImport.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MoonlightAppImport.Http
{
    public class SunshineHttpClient : IHttpClient
    {
        #region Fields
        protected readonly HttpClient _sunshinePrivateApiClient;
        protected readonly HttpClient _sunshinePublicInfoClient;
        protected readonly MoonlightAppImportSettings _settings;
        protected readonly ILogger _logger = LogManager.GetLogger();

        protected bool disposedValue;
        #endregion

        #region Constructors
        public SunshineHttpClient(MoonlightAppImportSettings moonlightAppImportSettings)
        {
            _settings = moonlightAppImportSettings;
            CheckSettings();
            var handler = new WebRequestHandler();

            handler.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                // If the sunshine server does not have a valid certificate and the user chose to bypass it...
                if (_settings.SkipCertificateValidation)
                    return true;

                // If no errors, return true
                if (sslPolicyErrors == SslPolicyErrors.None)
                    return true;

                // If there are errors, throw a custom exception to inform the user
                _logger.Error($"There was no valid certificate found for the server \"{_settings.SunshineHost}\". Check the \"Skip certificate validation\" in the plugin settings menu!");
                throw new HttpRequestException($"There was no valid certificate found for the server \"{_settings.SunshineHost}\". Check the \"Skip certificate validation\" in the plugin settings menu!");
            };

            _sunshinePrivateApiClient = new HttpClient(handler);
            _sunshinePrivateApiClient.BaseAddress = new Uri($"https://{_settings.SunshineHost}:47990");

            _sunshinePublicInfoClient = new HttpClient();
            _sunshinePublicInfoClient.BaseAddress = new Uri($"http://{_settings.SunshineHost}:47989");
        }
        #endregion

        #region Methods
        public virtual async Task<MoonlightApps> GetGamesAsync()
        {
            AuthenticateWithBasicAuth();
            return await GetGamesInternalAsync();
        }

        public async Task<string> GetServerHostnameAsync()
        {
            _logger.Info($"Trying to retrieve the server hostname using {_sunshinePublicInfoClient.BaseAddress}/serverinfo");
            var response = await _sunshinePublicInfoClient.GetAsync("/serverinfo");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            // Parse the XML response
            var xml = XDocument.Parse(content);
            var hostname = xml.Root.Element("hostname")?.Value;

            return hostname ?? throw new HttpRequestException($"There was no hostname provided under the public Sunshine API \"{_sunshinePublicInfoClient.BaseAddress}/serverinfo\"!");
        }

        public async Task<bool> IsServerOnlineAsync()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(_settings.SunshineHost);

                    // Check if the ping was successful
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error pinging server: {ex.Message}");
                return false;
            }
        }

        protected async Task<MoonlightApps> GetGamesInternalAsync()
        {
            var response = await _sunshinePrivateApiClient.GetAsync("/api/apps");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MoonlightApps>(json);
        }

        protected virtual void CheckSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.SunshineHost))
                throw new ArgumentNullException(nameof(_settings.SunshineHost));

            if (string.IsNullOrWhiteSpace(_settings.SunshineUsername))
                throw new ArgumentNullException(nameof(_settings.SunshineHost));

            if (string.IsNullOrWhiteSpace(_settings.SunshinePassword))
                throw new ArgumentNullException(nameof(_settings.SunshineHost));

            if (string.IsNullOrWhiteSpace(_settings.MoonlightPath))
                throw new ArgumentNullException(nameof(_settings.SunshineHost));

            bool result = IPValidator.ValidateAndResolve(_settings.SunshineHost);
            if (!result)
                throw new ArgumentException("The sunshine host address was invalid!");

            result = File.Exists(_settings.MoonlightPath) && Path.GetFileName(_settings.MoonlightPath).Equals("Moonlight.exe", StringComparison.OrdinalIgnoreCase);
            if (!result)
                throw new ArgumentException("The moonlight path was invalid! Must point to a \"Moonlight.exe\".");

            _logger.Info($"Settings are valid:\nHost = {_settings.SunshineHost}\nUsername = {_settings.SunshineUsername}\nPassword = A password with {_settings.SunshinePassword.Length} length...\nMoonlight Path = {_settings.MoonlightPath}\nSkip Certificate Validation = {_settings.SkipCertificateValidation}");
        }

        private void AuthenticateWithBasicAuth()
        {
            _logger.Info($"Starting to authenticate to Sunshine using basic authentication.\nHost = {_settings.SunshineHost}\nUsername = {_settings.SunshineUsername}\nPassword = A password with {_settings.SunshinePassword.Length} length...");

            // Clear any existing authentication headers
            _sunshinePrivateApiClient.DefaultRequestHeaders.Authorization = null;

            // Set up basic authentication
            string authString = $"{_settings.SunshineUsername}:{_settings.SunshinePassword}";
            string base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(authString));
            _sunshinePrivateApiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", base64Auth);
        }
        #endregion

        #region Disposing
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _sunshinePrivateApiClient?.Dispose();
                    _sunshinePublicInfoClient?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
