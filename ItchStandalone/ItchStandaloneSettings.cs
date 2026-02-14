using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ItchStandalone
{
    public class ItchStandaloneSettings : ObservableObject
    {
        private string butlerPath = string.Empty;
        private string installPath = string.Empty;
        private bool importUninstalledGames = false;

        public string ButlerPath { get => butlerPath; set => SetValue(ref butlerPath, value); }
        public string InstallPath { get => installPath; set => SetValue(ref installPath, value); }
        public bool ImportUninstalledGames { get => importUninstalledGames; set => SetValue(ref importUninstalledGames, value); }
        
        // SecureString for API Key (Not serialized directly, handled manually)
        [DontSerialize]
        public SecureString ApiKey { get; set; }
    }

    public class ItchStandaloneSettingsViewModel : ObservableObject, ISettings
    {
        private readonly ItchStandalone plugin;
        private ItchStandaloneSettings editingClone { get; set; }

        private ItchStandaloneSettings settings;
        public ItchStandaloneSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ItchStandaloneSettingsViewModel(ItchStandalone plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves settings to a location based on your plugin ID.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ItchStandaloneSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ItchStandaloneSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
            // Handle SecureString manually if needed, or keeping it in memory
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // EXECUTE before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        // Helper to handle portable paths
        public string GetAbsoluteButlerPath()
        {
            if (string.IsNullOrWhiteSpace(Settings.ButlerPath))
            {
                 // Default check logic could go here
                 return string.Empty;
            }

            if (Settings.ButlerPath.Contains("{PlayniteDir}"))
            {
                return Settings.ButlerPath.Replace("{PlayniteDir}", plugin.PlayniteApi.Paths.ApplicationPath);
            }
            return Settings.ButlerPath;
        }
        
        public string GetAbsoluteInstallPath()
        {
             if (string.IsNullOrWhiteSpace(Settings.InstallPath))
            {
                 return string.Empty;
            }

            if (Settings.InstallPath.Contains("{PlayniteDir}"))
            {
                return Settings.InstallPath.Replace("{PlayniteDir}", plugin.PlayniteApi.Paths.ApplicationPath);
            }
            return Settings.InstallPath;
        }

        // Use this when saving paths from UI
        public string ConvertToPortablePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            
            string appPath = plugin.PlayniteApi.Paths.ApplicationPath;
            if (path.StartsWith(appPath, StringComparison.OrdinalIgnoreCase))
            {
                return path.Replace(appPath, "{PlayniteDir}");
            }
            return path;
        }
    }
}
