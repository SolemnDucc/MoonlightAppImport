using MoonlightAppImport.Http;
using MoonlightAppImport.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MoonlightAppImport
{
    public class MoonlightAppImport : LibraryPlugin
    {
        #region Fields
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly string _iconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icon.png");
        private readonly MoonlightAppImportSettingsViewModel _settings;
        #endregion

        #region Properties
        public override Guid Id { get; } = Guid.Parse("60ea0079-bf4b-417c-a1f3-d5470ec5e96b");
        public override string Name => "Moonlight App Import";
        public override string LibraryIcon => _iconPath;
        #endregion

        #region Constructors
        public MoonlightAppImport(IPlayniteAPI api) : base(api)
        {
            _settings = new MoonlightAppImportSettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
            _logger.Info("MoonlightAppImport initialized!");
        }
        #endregion

        #region Methods
        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            // If the Addon is not enabled, return empty list.
            if (!_settings.Settings.IsEnabled)
                return new List<GameMetadata>();

            _logger.Info("Getting Games from Moonlight...");
            IHttpClient httpClient = null;
            try
            {
                // Select the server type
                switch (_settings.Settings.ServerType)
                {
                    case ServerType.Sunshine:
                        _logger.Info("Sunshine server was chosen.");
                        httpClient = new SunshineHttpClient(_settings.Settings);
                        break;
                    case ServerType.Apollo:
                        _logger.Info("Apollo server was chosen.");
                        httpClient = new ApolloHttpClient(_settings.Settings);
                        break;
                    case ServerType.Vibepollo:
                        _logger.Info("Vibepollo server was chosen.");
                        httpClient = new VibepolloHttpClient(_settings.Settings);
                        break;
                }

                if (_settings.Settings.PingHost)
                {
                    bool online = httpClient.IsServerOnlineAsync().GetAwaiter().GetResult();
                    if (!online)
                    {
                        _logger.Error($"Tried to ping the Sunshine server \"{_settings.Settings.SunshineHost}\" but failed. The Sunshine server is not online or the host address is wrong!");
                        throw new TimeoutException($"Tried to ping the Sunshine server \"{_settings.Settings.SunshineHost}\" but failed. The Sunshine server is not online or the host address is wrong!");
                    }
                }

                string hostname = httpClient.GetServerHostnameAsync().GetAwaiter().GetResult();
                _logger.Info($"Successfully retrieved the hostname: {hostname}");

                MoonlightApps response = httpClient.GetGamesAsync().GetAwaiter().GetResult();
                List<GameMetadata> metadata = new List<GameMetadata>();

                foreach (App app in response.apps)
                {
                    var gameMetadata = new GameMetadata()
                    {
                        Name = app.name,
                        GameId = app.uuid ?? $"{hostname}-{app.name}",
                        GameActions =
                            new List<GameAction>()
                            {
                                new GameAction()
                                {
                                    Name = app.name,
                                    IsPlayAction = true,
                                    Type = GameActionType.File,
                                    Path = _settings.Settings.MoonlightPath,
                                    Arguments = $"stream \"{hostname}\" \"{app.name}\""
                                }
                            },
                        InstallDirectory = $"Sunshine server {hostname}",
                        IsInstalled = true,
                    };

                    // Add metadata if configured
                    if (_settings.Settings.AddMetadata)
                    {
                        gameMetadata.Icon = new MetadataFile(_settings.Settings.MoonlightPath);
                        gameMetadata.Description = $"This is an App that was automatically added by the plugin \"Moonlight App Import\" at {DateTime.Now}. It is installed on the {_settings.Settings.ServerType} server \"{hostname}\".";
                        gameMetadata.BackgroundImage = new MetadataFile(@"https://cdn2.steamgriddb.com/grid/6ca7ef116c25226eb528620dcecbadce.png");
                    }

                    metadata.Add(gameMetadata);
                    _logger.Info($"Added App \"{app.name}\" from Sunshine server \"{hostname}\" to the import list.");
                }

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Getting Games from Moonlight failed!");
                throw;
            }
            finally
            {
                httpClient?.Dispose();
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new MoonlightAppImportSettingsView();
        }
        #endregion
    }
}