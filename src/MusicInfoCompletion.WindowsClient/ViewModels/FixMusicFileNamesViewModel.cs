using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MusicInfoCompletion.Common;
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

            if (string.IsNullOrEmpty(SingerNameSpliter))
            {
                LoggerHelper.Logger.Info(nameof(SingerNameSpliter) + " should not be null or empty");
                return;
            }

            IsSaving = true;

            try
            {
                var formarts = FileNameFormat.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                var parts = GetParts(formarts);

                foreach (var music in MusicFiles)
                {
                    try
                    {
                        var fileName = music.FileInfo.Name.Remove(music.FileInfo.Name.LastIndexOf("."));
                        var nameParts = fileName.Split(FileNameSpliter, StringSplitOptions.RemoveEmptyEntries).Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()).ToArray();
                        if (parts.Count == nameParts.Length)
                        {
                            music.SelectedSongInfo = new SongDocument();

                            for (int i = 0; i < nameParts.Length; i++)
                            {
                                switch (parts[i])
                                {
                                    case Part.Title:
                                        if (OverrideRawValue || string.IsNullOrWhiteSpace(music.TagInfo.Tag.Title))
                                        {
                                            music.SelectedSongInfo.SongTitle = nameParts[i];
                                        }
                                        break;
                                    case Part.Singer:
                                        if (OverrideRawValue || music.TagInfo.Tag.Performers.Length == 0)
                                        {

                                            music.SelectedSongInfo.SingerNames = string.Join(
                                                Constants.Separater,
                                                nameParts[i]
                                                .Split(SingerNameSpliter, StringSplitOptions.RemoveEmptyEntries)
                                                .Where(u => !string.IsNullOrEmpty(u)));
                                        }
                                        break;
                                    case Part.Album:
                                        if (OverrideRawValue || string.IsNullOrWhiteSpace(music.TagInfo.Tag.Album))
                                        {
                                            music.SelectedSongInfo.Album = nameParts[i];
                                        }
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }

                            music.SaveResultToFileCommand.Execute(null);
                        }
                        else
                        {
                            LoggerHelper.Logger.Warn("Skip file {File} due to not meet the formats", music.FileInfo.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Logger.Error(ex, "Error occur in {method}", nameof(SaveResultToFile));
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Logger.Error(ex, "Error occur in {method}", nameof(SaveResultToFile));
            }

            IsSaving = false;
        }

        List<Part> GetParts(string[] formarts)
        {
            var parts = new List<Part>();

            foreach (var format in formarts)
            {
                switch (format)
                {
                    case Title:
                        parts.Add(Part.Title);
                        break;
                    case Singer:
                        parts.Add(Part.Singer);
                        break;
                    case Album:
                        parts.Add(Part.Album);
                        break;
                    default:
                        throw new ArgumentException("Wrong format {Format}", format);
                }
            }

            return parts;
        }

        const string Title = "title";
        const string Singer = "singer";
        const string Album = "album";

        enum Part
        {
            Title,
            Singer,
            Album
        }

        static string[] musicExtension = new[] { ".mp3", ".aac", ".flac", ".ape", ".wav" };

        public string FileNameFormatHint { get; set; } = $"like: {Title} {Singer} {Album}";

        public bool IsSaving { get; set; }

        public string FileNameFormat { get; set; } = "title singer";
        public string FileNameSpliter { get; set; } = "-";
        public string FileNameSpliterHint { get; set; } = "like: - or :";
        public string SingerNameSpliter { get; set; } = "、";

        public ICommand SaveResults { get; set; }

        public bool OverrideRawValue { get; set; }

        public ObservableCollection<MusicFileViewModel> MusicFiles { get; set; } = new ObservableCollection<MusicFileViewModel>();

        async Task GetMusicFiles(CancellationToken token)
        {
            IsLoading = true;

            try
            {
                MusicFiles.Clear();

                var models = await Task.Run(() =>
                {
                    var tempMusicPath = MusicPath;

                    if (!Directory.Exists(tempMusicPath))
                    {
                        return Array.Empty<MusicFileViewModel>();
                    }

                    var files = Directory.GetFiles(tempMusicPath, "*", SearchOption.AllDirectories);
                    return files.Select(u => new FileInfo(u)).Where(u => musicExtension.Contains(u.Extension.ToLower())).Select(u => new MusicFileViewModel(u)).ToArray();
                }, token);

                foreach (var model in models)
                {
                    MusicFiles.Add(model);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Logger.Error(ex, "Error occur in {method}", nameof(GetMusicFiles));
            }
            finally
            {
                IsLoading = false;
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
