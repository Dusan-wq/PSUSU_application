using System;
using System.Windows;
using System.Windows.Threading;
using DataConcentrator;

namespace ScadaGUI
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += App_AppDomainUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;


                UISync.Capture();

                DataConcentratorManager.Instance.LoadTagsFromDatabase();


                var login = new LoginWindow();
                bool? ok = login.ShowDialog();
                if (ok != true)
                {
                    Shutdown();
                    return;
                }


                var main = new MainWindow();
                this.MainWindow = main;
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                main.Show();
            }
            catch (Exception ex)
            {
                ShowFatalError("Greska prilikom pokretanja aplikacije (OnStartup)", ex);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            PLC.StopSimulator();
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowFatalError("Neuhvacen izuzetak (UI thread)", e.Exception);
            e.Handled = true;
        }

        private void App_AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowFatalError("Neuhvacen izuzetak (background thread)", e.ExceptionObject as Exception);
        }

        private void ShowFatalError(string context, Exception ex)
        {
            string message = ex == null
                ? context
                : $"{context}:\n\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}";

            if (ex?.InnerException != null)
            {
                message += $"\n\n--- Inner exception ---\n{ex.InnerException.GetType().Name}: {ex.InnerException.Message}\n\n{ex.InnerException.StackTrace}";
            }

            MessageBox.Show(message, "Fatalna greska", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
