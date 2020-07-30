using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReactiveUI;

namespace MusicInfoCompletion.WindowsClient
{
    /// <summary>
    /// MusicFile.xaml 的交互逻辑
    /// </summary>
    public partial class MusicFiles : ReactiveUserControl<MusicFilesViewModel>
    {
        public MusicFiles()
        {
            InitializeComponent();
            ViewModel = new MusicFilesViewModel();

            this.WhenActivated(disposableRegistration =>
            {
                this.Bind(ViewModel,
                    viewModel => viewModel.MusicPath,
                    view => view.MusicPath.Text)
                 .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                viewModel => viewModel.MusicFiles,
                view => view.MusicFilesList.ItemsSource)
                 .DisposeWith(disposableRegistration);
            });
        }
    }
}
