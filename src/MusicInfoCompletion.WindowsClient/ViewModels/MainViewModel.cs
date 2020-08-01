namespace MusicInfoCompletion.WindowsClient
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            MusicFilesViewModel = new MusicFilesViewModel();
        }

        public MusicFilesViewModel MusicFilesViewModel { get; }
    }
}
