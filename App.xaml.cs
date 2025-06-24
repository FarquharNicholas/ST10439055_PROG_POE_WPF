using System.Windows;

namespace ST10439055_PROG_POE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Any application-wide initialization can go here
            // For example, setting up global exception handling
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}",
                           "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Mark the exception as handled to prevent the application from crashing
            e.Handled = true;
        }
    }
}