using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Mvc;
using MusicInfoCompletion.Common;
using MusicInfoCompletion.Data;
using MusicInfoCompletion.Index;

namespace MusicInfoCompletion.Server.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class MusicController : ControllerBase
    {
        readonly MusicDbContext musicDbContext;
        readonly MusicConfiguration musicConfiguration;

        public MusicController(MusicDbContext musicDbContext, MusicConfiguration musicConfiguration)
        {
            this.musicDbContext = musicDbContext;
            this.musicConfiguration = musicConfiguration;
        }

        [HttpPost]
        public SearchResult GetSongInfo(string searchQuery)
        {
            var query = QueryParser.Parse(searchQuery);
            var songs = LucenePool.SearchSongs(musicConfiguration.IndexPath, query, 100);
            var result = new SearchResult
            {
                SongInfos = songs
            };

            return result;
        }

        static QueryParser QueryParser { get; } = LucenePool.GetQueryParser();
    }
}
