using System;
using System.Collections.Generic;
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

namespace MusicInfoCompletion.WindowsClient
{
    /// <summary>
    /// FixFileNames.xaml 的交互逻辑
    /// </summary>
    public partial class FixByFileNames : UserControl
    {
        public FixByFileNames()
        {
            InitializeComponent();

            WPFLogTarget.LogMessageToPage2 = log => LogMessage(log);
            WPFLogTarget.ClearLogForPage2 = () => ClearMessage();
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

        void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            WPFLogTarget.ClearLogForPage2?.Invoke();
        }
    }
}
