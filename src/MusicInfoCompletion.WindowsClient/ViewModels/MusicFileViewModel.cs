using System.IO;

namespace MusicInfoCompletion.WindowsClient
{
    public class MusicFileViewModel : BaseViewModel
    {
        string searchResults;

        public MusicFileViewModel(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            TagInfo = TagLib.File.Create(fileInfo.FullName);
        }

        public FileInfo FileInfo { get; }
        public TagLib.File TagInfo { get; }
        public string Path => FileInfo.FullName;
        public string Title => TagInfo.Tag.Title;
        public string Album => TagInfo.Tag.Album;
        public string Genres => string.Join(", ", TagInfo.Tag.Genres);
        public string Singers => string.Join(", ", TagInfo.Tag.Performers);
        public string SearchResults { get => searchResults; set { searchResults = value; NotifyPropertyChange(); } }
    }
}
