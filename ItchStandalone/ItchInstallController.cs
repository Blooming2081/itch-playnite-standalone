using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ItchStandalone
{
    public class ItchInstallController : InstallController
    {
        private readonly ItchStandalone plugin;
        private readonly ButlerWrapper butler;
        private CancellationTokenSource cts;

        public ItchInstallController(ItchStandalone plugin, Game game) : base(game)
        {
            this.plugin = plugin;
            // ItchStandaloneSettingsViewModel is accessible via plugin.Settings currently
             // But plugin.GetSettings returns ISettings, we might need cast or refactor plugin to expose ViewModel
             // For now assuming plugin.Settings is the ViewModel or we access it
            var settingsVM = plugin.GetSettings(false) as ItchStandaloneSettingsViewModel;
            this.butler = new ButlerWrapper(settingsVM); 
            Name = "Install with Butler";
        }

        public override void Install(InstallActionArgs args)
        {
            // Install process should be async but Install method is void.
            // We run it in a separate task.
            cts = new CancellationTokenSource();
            
            Task.Run(async () => 
            {
                try
                {
                    // 0. Prepare
                    var gameUrl = Game.Links?.FirstOrDefault(l => l.Name == "Itch.io")?.Url;
                    if(string.IsNullOrEmpty(gameUrl))
                    {
                         // Temporary fallback: try to use Game.GameId if it holds URL-like info or handle error
                         // For independent plugin, GameId might be the ID, URL could be stored elsewhere.
                         // Assuming Game.Source.Name is "itch.io" and we have valid URL in Links or Description for now.
                         // Or construct url if possible.
                         gameUrl = Game.GameId; // Fallback? Need proper URL handling in Library Plugin
                    }

                    if (string.IsNullOrEmpty(gameUrl))
                    {
                         throw new Exception("No game URL found.");
                    }

                    // 1. Fetch Channels
                    // Check for cancellation
                    if (cts.IsCancellationRequested) return;

                    var channels = await butler.FetchChannelsAsync(gameUrl);
                    string selectedChannel = null;

                    if (channels.Count == 1)
                    {
                        selectedChannel = channels[0];
                    }
                    else if (channels.Count > 1)
                    {
                        // Smart selection
                        if (Environment.Is64BitOperatingSystem && channels.Contains("windows-64"))
                            selectedChannel = "windows-64";
                        else if (channels.Contains("windows-32"))
                            selectedChannel = "windows-32";
                        else if (channels.Contains("pcnix")) // Linux/Cross platform maybe?
                            selectedChannel = "pcnix";
                        
                        // Fallback or User Selection
                        if (selectedChannel == null)
                        {
                            // Ask user
                            var selection = plugin.PlayniteApi.Dialogs.SelectString("Select Install Channel", "Multiple channels found", channels.FirstOrDefault(), channels);
                            if (selection.Result)
                            {
                                selectedChannel = selection.SelectedString;
                            }
                            else
                            {
                                // User cancelled
                                InvokeOnInstalled(new InstallControllerActionArgs { Error = new Exception("Installation cancelled by user.") });
                                return;
                            }
                        }
                    }

                    // 2. Determine Install Path
                    // Use Default Install Path from settings
                    var settingsVM = plugin.GetSettings(false) as ItchStandaloneSettingsViewModel;
                    var installBase = settingsVM.GetAbsoluteInstallPath();
                    if (string.IsNullOrEmpty(installBase))
                    {
                         // Fallback relative to Playnite
                         installBase = Path.Combine(plugin.PlayniteApi.Paths.ApplicationPath, "Games");
                    }
                    
                    var installDir = Path.Combine(installBase, Game.Name.Replace(":", "").Trim()); // Simple sanitization

                    // 3. Install
                     // Register global progress if possible, but InstallController doesn't have direct global progress
                     // We can use PlayniteApi.Dialogs.ActivateGlobalProgress logic if we want modal blocking
                     // OR just update Game.IsInstalling state which Playnite handles.
                     // But user asked for "GlobalProgressOptions" usage.
                     
                     // NOTE: InstallController runs in background. Playnite shows progress automatically if we don't block UI?
                     // Actually Playnite doesn't show detailed progress bar for InstallController unless we use GlobalProgress.
                     // But GlobalProgress blocks UI.
                     // A better way for library plugins: Just run and let Playnite show "Installing..."
                     // If we want detailed percentage, we might need to update Game.InstallProgress? (Not available in SDK Models directly like that)
                     // Ah, Playnite 9+ / 10 doesn't have per-game progress bar in Grid view easily exposed.
                     // However, the request specifically asked for "GlobalProgressOptions". 
                     // This implies a blocking progress dialog or a non-blocking one.
                     
                    var progressOptions = new GlobalProgressOptions("Installing " + Game.Name, true)
                    {
                        Cancelable = true
                    };

                    await plugin.PlayniteApi.Dialogs.ActivateGlobalProgress(async (a) =>
                    {
                        a.ProgressMaxValue = 100;
                        await butler.InstallAsync(gameUrl, installDir, selectedChannel, a.CancelToken, (p) => 
                        {
                            a.CurrentProgressValue = p;
                            a.Text = $"Installing {Game.Name} ({p:F0}%)";
                        });
                    }, progressOptions);

                    // 4. Finalize
                    var installInfo = new GameInstallationData
                    {
                        InstallDirectory = installDir
                    };

                    InvokeOnInstalled(new InstallControllerActionArgs { Installed = true, InstallationData = installInfo });
                }
                catch (Exception ex)
                {
                    InvokeOnInstalled(new InstallControllerActionArgs { Installed = false, Error = ex });
                }
            });
        }
        
        // Should implement Dispose to cancel cts
        public override void Dispose()
        {
            cts?.Cancel();
            base.Dispose();
        }
    }
}
