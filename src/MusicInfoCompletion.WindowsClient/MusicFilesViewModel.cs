using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

namespace MusicInfoCompletion.WindowsClient
{
    public class MusicFilesViewModel : ReactiveObject
    {

        public MusicFilesViewModel()
        {
            musicFiles = this.WhenAnyValue(u => u.MusicPath)
                .Throttle(TimeSpan.FromMilliseconds(800))
                .Select(path => path?.Trim())
                .DistinctUntilChanged()
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .SelectMany(GetMusicFiles)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.MusicFiles);
        }

        readonly ObservableAsPropertyHelper<IEnumerable<MusicFileViewModel>> musicFiles;
        public IEnumerable<MusicFileViewModel> MusicFiles => musicFiles.Value;

        string musicPath;
        public string MusicPath
        {
            get => musicPath;
            set => this.RaiseAndSetIfChanged(ref musicPath, value);
        }

        static string[] musicExtension = new[] { ".mp3", ".aac", ".flac" };

        async Task<IEnumerable<MusicFileViewModel>> GetMusicFiles(string path, CancellationToken token)
        {
            IEnumerable<MusicFileViewModel> musicFiles = Enumerable.Empty<MusicFileViewModel>();

            await Task.Run(() =>
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    musicFiles = files.Select(u => new FileInfo(u)).Where(u => musicExtension.Contains(u.Extension.ToLower())).Select(u => new MusicFileViewModel(u));
                }
            }, token);

            return musicFiles;
        }
    }
}
