using System.IO;
using ReactiveUI;

namespace MusicInfoCompletion.WindowsClient
{
    public class MusicFileViewModel : ReactiveObject
    {
        public MusicFileViewModel(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            TagInfo = TagLib.File.Create(fileInfo.FullName);
        }

        public FileInfo FileInfo { get; }
        public TagLib.File TagInfo { get; }
        public string Singers => string.Join(", ", TagInfo.Tag.Performers);
    }
}
