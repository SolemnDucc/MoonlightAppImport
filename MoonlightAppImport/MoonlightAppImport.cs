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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MoonlightAppImport
{
    public class MoonlightAppImport : LibraryPlugin
    {
        #region Fields
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly string _iconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icon.png");
        private MoonlightAppImportSettingsViewModel settings { get; set; }
        #endregion

        #region Properties
        public override Guid Id { get; } = Guid.Parse("60ea0079-bf4b-417c-a1f3-d5470ec5e96b");
        public override string Name => "Moonlight App Import";
        public override string LibraryIcon => _iconPath;
        #endregion

        #region Constructors
        public MoonlightAppImport(IPlayniteAPI api) : base(api)
        {
            settings = new MoonlightAppImportSettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }
        #endregion

        #region Methods
        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            IHttpClient httpClient = null;
            try
            {
                // Get Games from Sunshine server
                if (settings.Settings.IsApollo)
                    httpClient = new ApolloHttpClient(settings.Settings);
                else
                    httpClient = new SunshineHttpClient(settings.Settings);

                bool online = httpClient.IsServerOnlineAsync().GetAwaiter().GetResult();
                if (!online)
                    throw new TimeoutException("The Sunshine server is not online or the host address is wrong!");

                string hostname = httpClient.GetServerHostnameAsync().GetAwaiter().GetResult();
                MoonlightApps response = httpClient.GetGamesAsync().GetAwaiter().GetResult();
                List<GameMetadata> metadata = new List<GameMetadata>();

                foreach (App app in response.apps)
                {
                    metadata.Add(new GameMetadata()
                    {
                        Name = app.name,
                        GameId = app.uuid ?? $"{hostname}-{app.name}",
                        GameActions = new List<GameAction>()
                        {
                            new GameAction()
                            {
                                Name = app.name,
                                IsPlayAction = true,
                                Type = GameActionType.File,
                                Path = settings.Settings.MoonlightPath,
                                Arguments = $"stream \"{hostname}\" \"{app.name}\""
                            }
                        },
                        InstallDirectory = $"Sunshine server {hostname}",
                        Description = $"This is an App that was automatically added by the plugin \"Moonlight App Import\" at {DateTime.Now}. It is installed on the Sunshine server \"{hostname}\".",
                        IsInstalled = true,
                        Icon = new MetadataFile(settings.Settings.MoonlightPath),
                        BackgroundImage = new MetadataFile(@"https://cdn2.steamgriddb.com/grid/6ca7ef116c25226eb528620dcecbadce.png")
                    });
                }

                return metadata;
            }
            finally
            {
                httpClient?.Dispose();
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new MoonlightAppImportSettingsView();
        }
        #endregion
    }
}