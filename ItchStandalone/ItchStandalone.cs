using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ItchStandalone
{
    public class ItchStandalone : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private ItchStandaloneSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("A3D7E8B1-C2F4-4D5E-9F6A-7B8C9D0E1F2A");

        public override string Name => "itch.io Standalone";

        public override LibraryClient Client { get; } = new ItchStandaloneClient();

        public ItchStandalone(IPlayniteAPI api) : base(api)
        {
            settings = new ItchStandaloneSettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }


       
        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var apiKey = settings.Settings.ApiKey;
            if (apiKey == null || apiKey.Length == 0)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("itch-no-key", "itch.io: API Key is missing. Please configure it in settings.", NotificationType.Error));
                return new List<GameMetadata>();
            }

            string key = string.Empty;
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(apiKey);
                key = System.Runtime.InteropServices.Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }

            var client = new ItchApiClient(key);
            // Sync call
            return client.GetLibraryGamesAsync().GetAwaiter().GetResult();
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
                yield break;

            yield return new ItchInstallController(this, args.Game);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
                yield break;

            yield return new ItchUninstallController(this, args.Game);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ItchStandaloneSettingsView();
        }
    }

    public class ItchStandaloneClient : LibraryClient
    {
        public override bool IsInstalled => false;

        public override void Open()
        {
            // Do nothing
        }
    }
}
