using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicInfoCompletion.Common;
using MusicInfoCompletion.Data;

namespace MusicInfoCompletion.Index
{
    public class IndexMaintainer : IDisposable
    {
        readonly MusicConfiguration musicConfiguration;
        readonly IServiceScopeFactory serviceScopeFactory;
        readonly ILogger<IndexMaintainer> logger;

        public IndexMaintainer(MusicConfiguration musicConfiguration, IServiceScopeFactory serviceScopeFactory, ILogger<IndexMaintainer> logger)
        {
            this.musicConfiguration = musicConfiguration;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        public async Task InitIndex(CancellationToken cancellationToken)
        {
            logger.LogInformation("Init index start");
            var songDocuments = new List<SongDocument>();

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var musicDbContext = scope.ServiceProvider.GetService<MusicDbContext>();

                var songs = musicDbContext.Songs.Include(u => u.SingerSongs).ThenInclude(u => u.Singer).Include(u => u.Album).Include(u => u.GenreSongs).ThenInclude(u => u.Genre);

                foreach (var item in songs)
                {
                    songDocuments.Add(item.GetSongDocument());
                }
            }

            await Task.Run(() =>
            {
                System.IO.Directory.CreateDirectory(musicConfiguration.IndexPath);
                LucenePool.BuildIndex(musicConfiguration.IndexPath, true, true, songDocuments.Select(u => u.GetDocument()), true);
                LucenePool.SaveResults(musicConfiguration.IndexPath, false);
            }, cancellationToken);

            logger.LogInformation("Init index finished");
        }

        public async Task AddIndex(Song song, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                LucenePool.BuildIndex(musicConfiguration.IndexPath, true, true, new[] { song.GetSongDocument().GetDocument() }, true);
            }, cancellationToken);
        }

        public async Task UpdateIndex(Song song, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                LucenePool.UpdateIndex(musicConfiguration.IndexPath, new Term(nameof(SongDocument.SongPk), song.Pk.ToString()), song.GetSongDocument().GetDocument());
            }, cancellationToken);
        }

        public async Task RemoveIndex(Guid songPk, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                LucenePool.DeleteIndex(musicConfiguration.IndexPath, new Term(nameof(SongDocument.SongPk), songPk.ToString()));
            }, cancellationToken);
        }

        public async Task DeleteAllIndex(CancellationToken cancellationToken)
        {
            logger.LogInformation("DeleteAllIndex start");

            await Task.Run(() =>
            {
                LucenePool.DeleteAllIndex(musicConfiguration.IndexPath);
            }, cancellationToken);

            logger.LogInformation("DeleteAllIndex successful");
        }

        public void Dispose()
        {
            LucenePool.SaveResultsAndClearLucenePool(musicConfiguration.IndexPath);
            logger.LogInformation("Dispose successful");
        }

        public bool IndexExist()
        {
            using var dir = FSDirectory.Open(musicConfiguration.IndexPath);
            var exist = DirectoryReader.IndexExists(dir);

            return exist;
        }

        public async Task SaveResults(CancellationToken cancellationToken)
        {
            logger.LogInformation("SaveResults start");
            await Task.Run(() =>
            {
                LucenePool.SaveResults(musicConfiguration.IndexPath, true);
            }, cancellationToken);
            logger.LogInformation("SaveResults successful");
        }
    }
}
