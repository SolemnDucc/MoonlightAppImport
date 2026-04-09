using MoonlightAppImport.Http;
using MoonlightAppImport.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace MoonlightAppImport
{
    /// <summary>
    /// Represents the main library plugin for importing Moonlight applications into Playnite.
    /// </summary>
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
        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            yield return new MyInstallController(args.Game);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            yield return new MyUninstallController(args.Game);
        }

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

                MoonlightApps newGames = httpClient.GetGamesAsync().GetAwaiter().GetResult();
                List<GameMetadata> metadata = new List<GameMetadata>();

                // Cache the resolved GameIds to ensure we don't encounter null references during LINQ comparisons
                var parsedNewGames = newGames.apps.Select(app => new
                {
                    App = app,
                    GameId = app.uuid ?? $"{hostname}-{app.name}"
                }).ToList();

                _logger.Info($"The server returned a total of {parsedNewGames.Count} games.");

                // 1. Add all apps from the server and mark them as installed
                foreach (var parsed in parsedNewGames)
                {
                    var gameMetadata = new GameMetadata()
                    {
                        Name = parsed.App.name,
                        GameId = parsed.GameId,
                        GameActions = new List<GameAction>()
                        {
                            new GameAction()
                            {
                                Name = parsed.App.name,
                                IsPlayAction = true,
                                Type = GameActionType.File,
                                Path = _settings.Settings.MoonlightPath,
                                Arguments = $"stream \"{hostname}\" \"{parsed.App.name}\""
                            }
                        },
                        InstallDirectory = $"{_settings.Settings.ServerType} server {hostname}",
                        IsInstalled = true
                    };

                    // Add metadata if configured
                    if (_settings.Settings.AddMetadata)
                    {
                        gameMetadata.Icon = new MetadataFile(_settings.Settings.MoonlightPath);
                        gameMetadata.Description = $"This is an App that was automatically added by the plugin \"Moonlight App Import\" at {DateTime.Now}. It is installed on the {_settings.Settings.ServerType} server \"{hostname}\".";
                        gameMetadata.BackgroundImage = new MetadataFile(@"https://cdn2.steamgriddb.com/grid/6ca7ef116c25226eb528620dcecbadce.png");
                    }

                    metadata.Add(gameMetadata);
                    _logger.Info($"Added App \"{parsed.App.name}\" ({parsed.App.uuid}) from the {_settings.Settings.ServerType} server \"{hostname}\" to the import list.");
                }

                // 2. Process games that are in the Playnite database but no longer exist on the server
                if (_settings.Settings.RemoveGames)
                {
                    // Retrieve all installed games from the Playnite database that were imported by this plugin
                    var oldGames = PlayniteApi.Database.Games
                        .Where(g => g.PluginId == Id)
                        .ToList();

                    var gamesNoLongerOnServer = oldGames.Where(old => !parsedNewGames.Any(n => n.GameId == old.GameId)).ToList();

                    if (gamesNoLongerOnServer.Count > 0)
                    {
                        if (_settings.Settings.RemoveType == RemoveType.Uninstall)
                        {
                            // To uninstall cleanly during sync, return the game with IsInstalled set to false.
                            // Playnite will process this without triggering background uninstall tasks.
                            _logger.Info($"The setting to uninstall games is enabled. Games that are no longer present on the {_settings.Settings.ServerType} server \"{hostname}\" will be marked as uninstalled.");
                            foreach (var oldGame in gamesNoLongerOnServer.Where(g => g.IsInstalled))
                            {
                                metadata.Add(new GameMetadata
                                {
                                    GameId = oldGame.GameId,
                                    Name = oldGame.Name,
                                    IsInstalled = false
                                });
                                _logger.Info($"Game \"{oldGame.Name}\" is no longer present on the server. Marked as uninstalled.");
                            }
                        }
                        else if (_settings.Settings.RemoveType == RemoveType.Remove)
                        {
                            // Use BufferedUpdate to prevent massive UI stuttering when deleting multiple items at once
                            _logger.Info($"The setting to remove games is enabled. Games that are no longer present on the {_settings.Settings.ServerType} server \"{hostname}\" will be removed from Playnite.");
                            using (PlayniteApi.Database.BufferedUpdate())
                            {
                                foreach (var oldGame in gamesNoLongerOnServer)
                                {
                                    PlayniteApi.Database.Games.Remove(oldGame.Id);
                                    _logger.Info($"Game \"{oldGame.Name}\" is no longer present on the server. Removed completely from DB.");
                                }
                            }
                        }
                    }
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

    /// <summary>
    /// Handles the pseudo-installation process for Moonlight remote applications.
    /// </summary>
    public class MyInstallController : InstallController
    {
        public MyInstallController(Game game) : base(game)
        {
        }

        public override void Install(InstallActionArgs args)
        {
            // Triggers Playnites internal events to properly update the UI and state
            InvokeOnInstalled(new GameInstalledEventArgs());
        }
    }

    /// <summary>
    /// Handles the pseudo-uninstallation process for Moonlight remote applications.
    /// </summary>
    public class MyUninstallController : UninstallController
    {
        public MyUninstallController(Game game) : base(game)
        {
        }

        public override void Uninstall(UninstallActionArgs args)
        {
            // Triggers Playnites internal events to properly update the UI and state
            InvokeOnUninstalled(new GameUninstalledEventArgs());
        }
    }
}