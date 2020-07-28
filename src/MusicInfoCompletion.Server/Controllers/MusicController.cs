using System;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MusicInfoCompletion.Common;
using MusicInfoCompletion.Index;

namespace MusicInfoCompletion.Server.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class MusicController : ControllerBase
    {
        readonly MusicConfiguration musicConfiguration;
        readonly ILogger<MusicController> logger;

        public MusicController(MusicConfiguration musicConfiguration, ILogger<MusicController> logger)
        {
            this.musicConfiguration = musicConfiguration;
            this.logger = logger;
        }

        [HttpPost]
        public SearchResult GetSongInfo(string searchQuery)
        {
            var result = new SearchResult
            {
                ResultCode = ResultCode.Successful
            };

            try
            {
                logger.LogDebug("GetSongInfo start, use search query: {query}", searchQuery);

                var query = QueryParser.Parse(searchQuery);
                var songs = LucenePool.SearchSongs(musicConfiguration.IndexPath, query, 10);
                result.SongInfos = songs;

                logger.LogDebug("GetSongInfo ended, use search query: {query}, find {Length} results", searchQuery, songs.Length);
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Exception;
                result.ExtraMessage = "Error occur";
                logger.LogError(ex, "GetSongInfo Error Occur");
            }

            return result;
        }

        static QueryParser QueryParser { get; } = LucenePool.GetQueryParser();
    }
}
