using System.Windows;

namespace MusicInfoCompletion.WindowsClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            NLog.LogManager.LoadConfiguration("nlog.WindowsClient.config");
        }
    }
}
