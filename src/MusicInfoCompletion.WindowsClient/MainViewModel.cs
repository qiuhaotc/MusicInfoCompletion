using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace MusicInfoCompletion.WindowsClient
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            SearchCommand = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.MessageBox.Show("Test");
            });

            MusicFilesViewModel = new MusicFilesViewModel();
        }

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        public MusicFilesViewModel MusicFilesViewModel { get; }
    }
}
