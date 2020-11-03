using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;

namespace MusicInfoCompletion.WindowsClient
{
    public class FixMusicFileNamesViewModel : BaseViewModel
    {
        string musicPath;

        public FixMusicFileNamesViewModel()
        {
            DoGetFiles = new CommandHandler(async () => await GetMusicFiles(CancellationToken.None), () => !IsLoading);

            this.WhenAnyValue(u => u.MusicPath)
                .Where(u => !string.IsNullOrEmpty(u))
                .Throttle(TimeSpan.FromMilliseconds(800))
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(DoGetFiles);

            FileNameFormatHint = $"like: {Title} {Singer} {Album}";

            SaveResults = new CommandHandler(SaveResultToFile, () => !IsSaving);
        }

        void SaveResultToFile()
        {
            if (string.IsNullOrEmpty(FileNameSpliter))
            {
                LoggerHelper.Logger.Info(nameof(FileNameSpliter) + " should not be null or empty");
                return;
            }

            if (string.IsNullOrEmpty(FileNameFormat))
            {
                LoggerHelper.Logger.Info(nameof(FileNameFormat) + " should not be null or empty");
                return;
            }

            IsSaving = true;

            foreach (var music in MusicFiles)
            {
                try
                {
                    var fileName = music.FileInfo.Name.Remove(music.FileInfo.Name.LastIndexOf("."));
                    fileName.Split("-");

                    // TODO: Finish
                }
                catch (Exception ex)
                {
                    LoggerHelper.Logger.Error(ex, "Error occur in {method}", nameof(SaveResultToFile));
                }
            }

            IsSaving = false;
        }

        const string Title = "{title}";
        const string Singer = "{singer}";
        const string Album = "{album}";

        static string[] musicExtension = new[] { ".mp3", ".aac", ".flac", ".ape", ".wav" };

        public string FileNameFormatHint { get; set; }

        public bool IsSaving { get; set; }

        public string FileNameFormat { get; set; }
        public string FileNameSpliter { get; set; }
        public string FileNameSpliterHint { get; set; } = "like: - or :";

        public ICommand SaveResults { get; set; }

        public ObservableCollection<MusicFileViewModel> MusicFiles { get; set; } = new ObservableCollection<MusicFileViewModel>();

        async Task GetMusicFiles(CancellationToken token)
        {
            MusicFiles.Clear();

            var models = await Task.Run(() =>
            {
                var files = Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories);
                return files.Select(u => new FileInfo(u)).Where(u => musicExtension.Contains(u.Extension.ToLower())).Select(u => new MusicFileViewModel(u)).ToArray();
            }, token);

            foreach (var model in models)
            {
                MusicFiles.Add(model);
            }
        }

        public string MusicPath
        {
            get => musicPath;
            set
            {
                this.RaiseAndSetIfChanged(ref musicPath, value);
            }
        }

        public ICommand DoGetFiles { get; }

        bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyPropertyChange();
            }
        }
    }
}
