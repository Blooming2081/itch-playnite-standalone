using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ItchStandalone
{
    public class ItchUninstallController : UninstallController
    {
        private readonly ItchStandalone plugin;
        private readonly ButlerWrapper butler;

        public ItchUninstallController(ItchStandalone plugin, Game game) : base(game)
        {
            this.plugin = plugin;
            var settingsVM = plugin.GetSettings(false) as ItchStandaloneSettingsViewModel;
            this.butler = new ButlerWrapper(settingsVM);
            Name = "Uninstall with Butler";
        }

        public override void Uninstall(UninstallActionArgs args)
        {
             // Uninstall logic
             // Run in background if needed, but SDK expects InvokeOnUninstalled when done.
             
             Task.Run(async () =>
             {
                 try
                 {
                     if (string.IsNullOrEmpty(Game.InstallDirectory) || !System.IO.Directory.Exists(Game.InstallDirectory))
                     {
                         throw new Exception("Install directory not found.");
                     }

                     // Show progress dialog or just run
                     // Butler clean is usually fast but better to be safe
                     await butler.UninstallAsync(Game.InstallDirectory);

                     InvokeOnUninstalled(new GameUninstalledEventArgs());
                 }
                 catch (Exception)
                 {
                     InvokeOnUninstalled(new GameUninstalledEventArgs());
                 }
             });
        }
    }
}
