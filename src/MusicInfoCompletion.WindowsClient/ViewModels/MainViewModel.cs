namespace MusicInfoCompletion.WindowsClient
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            MusicFilesViewModel = new MusicFilesViewModel();
            FixByFileNamesViewModel = new FixMusicFileNamesViewModel();
        }

        public MusicFilesViewModel MusicFilesViewModel { get; }

        public FixMusicFileNamesViewModel FixByFileNamesViewModel { get; }
    }
}
