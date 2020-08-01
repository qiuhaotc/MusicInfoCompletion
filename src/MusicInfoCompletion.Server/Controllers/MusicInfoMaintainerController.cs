using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MusicInfoCompletion.Common;
using MusicInfoCompletion.Data;
using MusicInfoCompletion.Index;

namespace MusicInfoCompletion.Server.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class MusicInfoMaintainerController : ControllerBase
    {
        public MusicInfoMaintainerController(ILogger<MusicInfoMaintainerController> logger, MusicDbContext musicDbContext, IndexMaintainer indexMaintainer, MusicConfiguration musicConfiguration)
        {
            Logger = logger;
            MusicDbContext = musicDbContext;
            IndexMaintainer = indexMaintainer;
            MusicConfiguration = musicConfiguration;
        }

        [HttpPost]
        public async Task<MaintainMusicInfoResult> AddSong(SongRequest songRequest)
        {
            try
            {
                var song = new Song
                {
                    Title = songRequest.Title,
                    AKATitles = songRequest.AKATitles,
                    Picture = songRequest.Picture,
                    Seconds = songRequest.Seconds,
                    SingerSongs = songRequest.Singers.Select(u => new SingerSong { Singer = new Singer { Name = u } }).ToList(),
                    GenreSongs = songRequest.Genres.Select(u => new GenreSong { Genre = new Genre { Title = u } }).ToList(),
                    Album = new Album { Title = songRequest.Album },
                };

                await MusicDbContext.Songs.AddAsync(song);
                await IndexMaintainer.AddIndex(song, CancellationToken.None);
                Logger.LogInformation("{AddSong} index successful", nameof(AddSong));

                await MusicDbContext.SaveChangesAsync();
                Logger.LogInformation("{AddSong} data successful", nameof(AddSong));

                return new MaintainMusicInfoResult
                {
                    ResultCode = ResultCode.Successful
                };
            }
            catch (Exception ex)
            {
                return new MaintainMusicInfoResult
                {
                    ResultCode = ResultCode.Exception,
                    ExtraMessage = ex.Message
                };
            }
        }

        [HttpPost]
        public async Task<MaintainMusicInfoResult> DeleteSong(Guid songPk)
        {
            var existSong = await MusicDbContext.Songs.FindAsync(songPk);
            if (existSong != null)
            {
                MusicDbContext.Songs.Remove(existSong);
                await IndexMaintainer.RemoveIndex(songPk, CancellationToken.None);
                Logger.LogInformation("{DeleteSong} index successful", nameof(DeleteSong));

                await MusicDbContext.SaveChangesAsync();
                Logger.LogInformation("{DeleteSong} data successful", nameof(DeleteSong));
            }

            return new MaintainMusicInfoResult
            {
                ResultCode = ResultCode.Successful
            };
        }

        [HttpPost]
        public async Task<MaintainMusicInfoResult> UpdateSong(SongRequest song)
        {
            // TODO: To be done
            //var existSong = MusicDbContext.Songs.Update(song);
            //if (existSong.State == Microsoft.EntityFrameworkCore.EntityState.Modified)
            //{
            //    await IndexMaintainer.UpdateIndex(song, CancellationToken.None);
            //    Logger.LogInformation("{UpdateSong} index successful", nameof(UpdateSong));

            //    await MusicDbContext.SaveChangesAsync();
            //    Logger.LogInformation("{UpdateSong} data successful", nameof(UpdateSong));
            //}
            //else
            //{
            //    Logger.LogWarning("No data exist in database with pk {Pk}, {UpdateSong} failed", nameof(UpdateSong));

            //    return new MaintainMusicInfoResult
            //    {
            //        ResultCode = ResultCode.Failed,
            //        ExtraMessage = $"No data exist in database with pk {song.Pk}, {nameof(UpdateSong)} failed"
            //    };
            //}

            return await Task.FromResult(new MaintainMusicInfoResult
            {
                ResultCode = ResultCode.Successful
            });
        }

        [HttpGet]
        public async Task<MaintainMusicInfoResult> SaveSongIndex()
        {
            await IndexMaintainer.SaveResults(CancellationToken.None);

            return new MaintainMusicInfoResult
            {
                ResultCode = ResultCode.Successful
            };
        }

        [HttpGet]
        public async Task<MaintainMusicInfoResult> RebuildAllIndex()
        {
            await IndexMaintainer.DeleteAllIndex(CancellationToken.None);
            await IndexMaintainer.InitIndex(CancellationToken.None);
            await IndexMaintainer.SaveResults(CancellationToken.None);

            return new MaintainMusicInfoResult
            {
                ResultCode = ResultCode.Successful
            };
        }

        ILogger<MusicInfoMaintainerController> Logger { get; }
        MusicDbContext MusicDbContext { get; }
        IndexMaintainer IndexMaintainer { get; }
        MusicConfiguration MusicConfiguration { get; }
    }
}
