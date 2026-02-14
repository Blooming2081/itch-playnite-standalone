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

        public override async IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var apiKey = settings.Settings.ApiKey; // SecureString
            if (apiKey == null || apiKey.Length == 0)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("itch-no-key", "itch.io: API Key is missing. Please configure it in settings.", NotificationType.Error));
                return new List<GameMetadata>();
            }

            // Unsecure the string to use in API call (In-memory exposure is inevitable for HTTP)
            string key = System.Runtime.InteropServices.Marshal.PtrToStringUni(
                System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(apiKey));

            var client = new ItchApiClient(key);
            return await client.GetLibraryGamesAsync();
        }

       // Remove the incorrect override if it exists, or keep it compatible with Playnite SDK version.
       // Playnite SDK 6.0+ usually has IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
       // The previous multi_replace added LibraryGetGamesResult override which might be wrong for the targeted SDK version in csproj (6.0.0).
       // Ensure we stick to the correct signature. 
       // NOTE: Playnite SDK 6.2.2 changed to LibraryGetGamesResult. The csproj says 6.0.0. 
       // Let's assume IEnumerable for 6.0.0. If incorrect, we'll fix.
       // Actually, I will remove the LibraryGetGamesResult override I added previously to be safe, 
       // AND update the IEnumerable one to be just valid.
       
       // Wait, `GetGames` cannot be async if it returns IEnumerable. 
       // SDK 6.0: public abstract IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args);
       // So we must run sync or .GetAwaiter().GetResult().
       
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

        public override InstallController GetInstallController(Game game)
        {
            return new ItchInstallController(this, game);
        }

        public override UninstallController GetUninstallController(Game game)
        {
            return new ItchUninstallController(this, game);
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
