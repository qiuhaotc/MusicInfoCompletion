﻿using System;
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
    public class MusicFilesViewModel : BaseViewModel
    {
        public MusicFilesViewModel()
        {
            DoGetFiles = new CommandHandler(async () => await GetMusicFiles(CancellationToken.None), () => !IsLoading);
            SearchResults = new CommandHandler(async () => await SearchMusicInfo(CancellationToken.None), () => !IsLoading);

            this.WhenAnyValue(u => u.MusicPath)
                .Where(u => !string.IsNullOrEmpty(u))
                .Throttle(TimeSpan.FromMilliseconds(800))
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(DoGetFiles);
        }

        public ObservableCollection<MusicFileViewModel> MusicFiles { get; set; } = new ObservableCollection<MusicFileViewModel>();

        public ICommand DoGetFiles { get; }

        public ICommand SearchResults { get; }

        string musicPath;
        public string MusicPath
        {
            get => musicPath;
            set
            {
                this.RaiseAndSetIfChanged(ref musicPath, value);
            }
        }

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

        public string SearchEndPoint { get => searchEndPoint; set { searchEndPoint = value; NotifyPropertyChange(); } }

        static string[] musicExtension = new[] { ".mp3", ".aac", ".flac", ".ape", ".wav" };
        string searchEndPoint = "https://music.qhnetdisk.tk";

        async Task GetMusicFiles(CancellationToken token)
        {
            MusicFiles.Clear();

            if (!string.IsNullOrEmpty(MusicPath) && Directory.Exists(MusicPath))
            {
                var results = await Task.Run(async () =>
                {
                    try
                    {
                        IsLoading = true;

                        var files = Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories);
                        return await Task.FromResult(files.Select(u => new FileInfo(u)).Where(u => musicExtension.Contains(u.Extension.ToLower())).Select(u => new MusicFileViewModel(u)).ToArray());
                    }
                    catch(Exception ex)
                    {
                        LoggerHelper.Logger.Error(ex, "Error occur in {method}", nameof(GetMusicFiles));

                        return Array.Empty<MusicFileViewModel>();
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }, token);

                foreach (var result in results)
                {
                    MusicFiles.Add(result);
                }
            }
        }

        async Task SearchMusicInfo(CancellationToken token)
        {
            WPFLogTarget.ClearLogForPage1?.Invoke();

            var client = new MusicServiceClient(new System.Net.Http.HttpClient())
            {
                BaseUrl = SearchEndPoint
            };

            IsLoading = true;

            foreach (var item in MusicFiles)
            {
                item.SongInfos = Array.Empty<SongDocument>();
            }

            foreach (var request in GetBatchSearchRequests())
            {
                try
                {
                    var result = await client.ApiV1MusicGetsonginfosAsync(request);

                    if (result.ResultCode == ResultCode.Successful)
                    {
                        foreach (var item in result.SearchResults)
                        {
                            var file = MusicFiles.FirstOrDefault(u => u.Path == item?.SearchRequestItem.Path);
                            if (file != null)
                            {
                                file.SongInfos = item.SongInfos;
                                LoggerHelper.Logger.Info("Find song info for {file}, info {songInfo}", file.Path, file.SearchResults);
                            }
                            else
                            {
                                file.SongInfos = Array.Empty<SongDocument>();
                            }
                        }
                    }
                    else
                    {
                        LoggerHelper.Logger.Warn("Failed to ApiV1MusicGetsonginfosAsync: {message}", result.ExtraMessage);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Logger.Error(ex, "Error occur in {method}", nameof(SearchMusicInfo));
                }
            }

            IsLoading = false;
        }

        IEnumerable<BatchSearchRequest> GetBatchSearchRequests()
        {
            var batch = 0;
            var shouldContinue = true;
            while (shouldContinue)
            {
                var items = MusicFiles.Skip(batch * Constants.MaxRequestForOneBatch).Take(Constants.MaxRequestForOneBatch).Select(u => new SearchRequestItem { Album = u.Album, Genres = u.TagInfo.Tag.Genres, Singers = u.TagInfo.Tag.Performers, Title = string.IsNullOrWhiteSpace(u.Title) ? u.FileInfo.Name : u.Title, Path = u.Path }).ToArray();

                if (items.Length > 0)
                {
                    batch++;
                    yield return new BatchSearchRequest
                    {
                        SearchRequestItems = items
                    };
                }
                else
                {
                    shouldContinue = false;
                }
            }
        }
    }
}
