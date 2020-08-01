using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicInfoCompletion.Common;
using MusicInfoCompletion.Data;
using MusicInfoCompletion.Index;
using NUnit.Framework;

namespace MusicInfoCompletion.Test
{
    public class LucenePoolTest
    {
        [Test]
        public async Task TestGetSongDocument()
        {
            using var context = MusicDbContextHelper.GetMusicDbContext();

            var song = GetSong();

            await context.Songs.AddAsync(song);
            await context.SaveChangesAsync();

            var document = song.GetSongDocument();
            Assert.Multiple(() =>
            {
                Assert.AreEqual("4", document.Album);
                Assert.AreEqual("2", document.AlbumDescription);
                Assert.AreEqual("1 | 2", document.Genres);
                Assert.AreEqual(song.Picture, document.Picture);
                Assert.AreEqual(song.Album.ReleaseDate.Value.ToString(Common.Constants.DateTimeFormat), document.ReleaseDate);
                Assert.AreEqual(0f, document.Score);
                Assert.AreEqual("1 | 4 | 5", document.SingerAKANames);
                Assert.AreEqual("2 | 6", document.SingerDescription);
                Assert.AreEqual("3 | 7", document.SingerNames);
                Assert.AreEqual("2", document.SongAKATitles);
                Assert.AreEqual(3, document.SongSeconds);
                Assert.AreEqual("3", document.SongTitle);
            });
        }

        [Test]
        public void TestGetDocument()
        {
            var song = GetSong();
            song.Pk = Guid.NewGuid();
            var document = song.GetSongDocument().GetDocument();
            Assert.AreEqual(12, document.Fields.Count);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(song.Pk.ToString(), document.Get(nameof(SongDocument.SongPk)));
                Assert.AreEqual("4", document.Get(nameof(SongDocument.Album)));
                Assert.AreEqual("2", document.Get(nameof(SongDocument.AlbumDescription)));
                Assert.AreEqual("1 | 2", document.Get(nameof(SongDocument.Genres)));
                Assert.AreEqual(song.Picture, document.GetBinaryValue(nameof(SongDocument.Picture)).Bytes);
                Assert.AreEqual(song.Album.ReleaseDate.Value.ToString(Common.Constants.DateTimeFormat), document.Get(nameof(SongDocument.ReleaseDate)));
                Assert.AreEqual("1 | 4 | 5", document.Get(nameof(SongDocument.SingerAKANames)));
                Assert.AreEqual("2 | 6", document.Get(nameof(SongDocument.SingerDescription)));
                Assert.AreEqual("3 | 7", document.Get(nameof(SongDocument.SingerNames)));
                Assert.AreEqual("2", document.Get(nameof(SongDocument.SongAKATitles)));
                Assert.AreEqual(3, document.GetField(nameof(SongDocument.SongSeconds)).GetInt32Value().Value);
                Assert.AreEqual("3", document.Get(nameof(SongDocument.SongTitle)));
            });
        }

        Song GetSong()
        {
            return new Song
            {
                AddedBy = "1",
                AKATitles = "2",
                Album = new Album
                {
                    AddedBy = "1",
                    Description = "2",
                    LastModifyBy = "3",
                    ReleaseDate = DateTime.Now,
                    Title = "4"
                },
                GenreSongs = new List<GenreSong>
                {
                    new GenreSong
                    {
                        Genre = new Genre
                        {
                            Title = "1"
                        }
                    },
                    new GenreSong
                    {
                        Genre = new Genre
                        {
                            Title = "2"
                        }
                    }
                },
                Picture = new byte[] { 1, 2, 3 },
                Seconds = 3,
                SingerSongs = new List<SingerSong>()
                {
                    new SingerSong
                    {
                        Singer = new Singer
                        {
                            AKANames = "1",
                            Description = "2",
                            Name = "3"
                        }
                    },
                    new SingerSong
                    {
                        Singer = new Singer
                        {
                            AKANames = "4 | 5",
                            Description = "6",
                            Name = "7"
                        }
                    }
                },
                Title = "3"
            };
        }
    }
}