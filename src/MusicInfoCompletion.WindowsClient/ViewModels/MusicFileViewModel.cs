using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using MusicInfoCompletion.Common;

namespace MusicInfoCompletion.WindowsClient
{
    public class MusicFileViewModel : BaseViewModel
    {
        IEnumerable<SongDocument> songInfos;

        public MusicFileViewModel(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            SaveResultToFileCommand = new CommandHandler(SaveResultToFile, () => !IsSaving);
            TagInfo = TagLib.File.Create(fileInfo.FullName);
            SongInfos = Array.Empty<SongDocument>();
        }

        public FileInfo FileInfo { get; }
        public TagLib.File TagInfo { get; }
        public string Path => FileInfo.FullName;
        public string Title => TagInfo.Tag.Title;
        public string Album => TagInfo.Tag.Album;
        public string Genres => string.Join(", ", TagInfo.Tag.Genres);
        public string Singers => string.Join(", ", TagInfo.Tag.Performers);
        public IEnumerable<SongDocument> SongInfos
        {
            get => songInfos;
            set
            {
                songInfos = value; NotifyPropertyChange(nameof(SearchResults));
                SelectedSongInfo = null;
            }
        }
        public string SearchResults => string.Join(Constants.Separater, SongInfos);
        public ICommand SaveResultToFileCommand { get; }
        public bool IsSaving { get; set; }
        public SongDocument SelectedSongInfo { get; set; }

        void SaveResultToFile()
        {
            SelectedSongInfo ??= SongInfos?.FirstOrDefault();

            if (SelectedSongInfo != null)
            {
                try
                {
                    IsSaving = true;
                    var genres = SelectedSongInfo.Genres?.Split(Constants.Separater, StringSplitOptions.RemoveEmptyEntries);
                    if (genres?.Any() ?? false)
                    {
                        TagInfo.Tag.Genres = genres;
                    }

                    TagInfo.Tag.Title = string.IsNullOrEmpty(SelectedSongInfo.SongTitle) ? TagInfo.Tag.Title : SelectedSongInfo.SongTitle;

                    TagInfo.Tag.Album = string.IsNullOrEmpty(SelectedSongInfo.Album) ? TagInfo.Tag.Album : SelectedSongInfo.Album;

                    var singers = SelectedSongInfo.SingerNames?.Split(Constants.Separater, StringSplitOptions.RemoveEmptyEntries);
                    if (singers?.Any() ?? false)
                    {
                        TagInfo.Tag.Performers = singers;
                    }

                    TagInfo.Save();

                    NotifyPropertyChange(nameof(Title));
                    NotifyPropertyChange(nameof(Album));
                    NotifyPropertyChange(nameof(Genres));
                    NotifyPropertyChange(nameof(Singers));

                    LoggerHelper.Logger.Info("Save search results tag to file {path}", FileInfo.FullName);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Logger.Error(ex, "Error occur in {method}", nameof(SaveResultToFile));
                }
                finally
                {
                    IsSaving = false;
                }
            }
        }
    }
}
