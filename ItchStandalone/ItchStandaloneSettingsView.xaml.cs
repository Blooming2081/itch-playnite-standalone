using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ItchStandalone
{
    public partial class ItchStandaloneSettingsView : UserControl
    {
        public ItchStandaloneSettingsView()
        {
            InitializeComponent();
        }

        private void ButtonSelectButlerPath_Click(object sender, RoutedEventArgs e)
        {
            var file = Playnite.SDK.API.Instance.Dialogs.SelectFile("Butler Executable|butler.exe");
            if (!string.IsNullOrEmpty(file))
            {
                // ViewModel을 통해 포터블 경로 변환 로직 수행 필요
                // 여기서는 ViewModel에 직접 접근하기 어려울 수 있으므로 DataContext를 활용
                if (DataContext is ItchStandaloneSettingsViewModel viewModel)
                {
                    viewModel.Settings.ButlerPath = viewModel.ConvertToPortablePath(file);
                }
            }
        }

        private void ButtonCheckVersion_Click(object sender, RoutedEventArgs e)
        {
             // TODO: Call ButlerWrapper via ViewModel or direct invocation ifWrapper is static/singleton
             // For now, show a placeholder message
             Playnite.SDK.API.Instance.Dialogs.ShowMessage("Butler check logic not implemented yet.", "Version Check");
        }
        
        private void ButtonSelectInstallPath_Click(object sender, RoutedEventArgs e)
        {
            var folder = Playnite.SDK.API.Instance.Dialogs.SelectFolder();
            if(!string.IsNullOrEmpty(folder))
            {
                 if (DataContext is ItchStandaloneSettingsViewModel viewModel)
                {
                    viewModel.Settings.InstallPath = viewModel.ConvertToPortablePath(folder);
                }
            }
        }

        private void PasswordBoxApiKey_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ItchStandaloneSettingsViewModel viewModel)
            {
                 if (sender is PasswordBox passwordBox)
                 {
                     // SecureString handling
                     viewModel.Settings.ApiKey = passwordBox.SecurePassword;
                 }
            }
        }
    }
}
