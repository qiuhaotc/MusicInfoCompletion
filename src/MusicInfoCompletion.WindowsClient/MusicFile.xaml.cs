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
    public partial class MusicFile : ReactiveUserControl<MusicFileViewModel>
    {
        public MusicFile()
        {
            InitializeComponent();

            this.WhenActivated(disposableRegistration =>
            {
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.FileInfo.Name,
                    view => view.FileName.Content)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.FileInfo.Extension,
                    view => view.Extension.Content)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Singers,
                    view => view.Singers.Content)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.TagInfo.Tag.Album,
                    view => view.Album.Content)
                    .DisposeWith(disposableRegistration);
            });
        }
    }
}
