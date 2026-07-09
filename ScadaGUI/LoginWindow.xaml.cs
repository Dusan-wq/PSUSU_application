using System;
using System.IO;
using System.Linq;
using System.Windows;
using DataConcentrator;

namespace ScadaGUI
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var user = UsernameBox.Text?.Trim() ?? string.Empty;
            var pass = PasswordBox.Password ?? string.Empty;
            if (user == "Dusan" && pass == "Dusan")
            {
                try
                {
                    Logger.Log("Login", user);
                }
                catch { }
                DialogResult = true;
                Close();
                return;
            }

            string msg = Localization.LocalizationManager.Instance["LoginFailed"];
            MessageBox.Show(this, msg, Localization.LocalizationManager.Instance["LoginFailedTitle"], MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
