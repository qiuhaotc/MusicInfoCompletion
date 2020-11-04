using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MusicInfoCompletion.WindowsClient
{
    /// <summary>
    /// MusicFile.xaml 的交互逻辑
    /// </summary>
    public partial class MusicFiles : UserControl
    {
        public MusicFiles()
        {
            InitializeComponent();

            WPFLogTarget.LogMessageToPage1 = log => LogMessage(log);
            WPFLogTarget.ClearLogForPage1 = () => ClearMessage();
        }

        void ClearMessage()
        {
            OutputLogs.Text = string.Empty;
        }

        async void LogMessage(string log)
        {
            await Task.Run(() => {
                OutputLogs.Dispatcher.Invoke(() => {
                    OutputLogs.Text += log + Environment.NewLine;
                    if (Scroller.VerticalOffset == Scroller.ScrollableHeight)
                    {
                        Scroller.ScrollToEnd();
                    }
                });
            });
        }
    }
}
