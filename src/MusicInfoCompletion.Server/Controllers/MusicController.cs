using System;
using System.Collections.Generic;
using System.Linq;
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
        public SingleSearchResult GetSongInfo(string searchQuery)
        {
            var result = new SingleSearchResult
            {
                ResultCode = ResultCode.Successful
            };

            try
            {
                logger.LogDebug("GetSongInfo start, use search query: {query}", searchQuery);

                var query = Parser.Parse(searchQuery);
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

        [HttpPost]
        public BatchSearchResult GetSongInfos(BatchSearchRequest searchRequest)
        {
            var result = new BatchSearchResult
            {
                ResultCode = ResultCode.Successful
            };

            if (searchRequest.SearchRequestItems.Count > Common.Constants.MaxRequestForOneBatch)
            {
                result.ResultCode = ResultCode.Failed;
                result.ExtraMessage = $"Too many request items, maxinum {Common.Constants.MaxRequestForOneBatch}.";
            }
            else if (searchRequest.SearchRequestItems?.Any() ?? false)
            {
                foreach (var item in searchRequest.SearchRequestItems)
                {
                    var batchItem = new BatchSearchResultItem
                    {
                        ResultCode = ResultCode.Successful,
                        SearchRequestItem = item
                    };

                    try
                    {
                        logger.LogDebug("{Method} start for {item}", nameof(GetSongInfos), item);

                        var searchStr = GetSearchQueryStr(item);

                        if (!string.IsNullOrWhiteSpace(searchStr))
                        {
                            var query = Parser.Parse(searchStr);
                            var songs = LucenePool.SearchSongs(musicConfiguration.IndexPath, query, 10);
                            batchItem.SongInfos = songs;
                            logger.LogDebug("{Method} ended for {item}, find {count} results.", nameof(GetSongInfos), item, songs.Length);
                        }
                        else
                        {
                            batchItem.ResultCode = ResultCode.Failed;
                            batchItem.ExtraMessage = "No valid info to generate search query";
                            logger.LogWarning("{Method} ended for no valid search information", nameof(GetSongInfos));
                        }
                    }
                    catch (Exception ex)
                    {
                        batchItem.ResultCode = ResultCode.Exception;
                        batchItem.ExtraMessage = "Error occur";
                        logger.LogError(ex, "GetSongInfo Error Occur");
                    }

                    result.SearchResults.Add(batchItem);
                }
            }

            return result;
        }

        string GetSearchQueryStr(SearchRequestItem item)
        {
            var result = new (string Field, string Value)[] {
                (nameof(SongDocument.SongTitle), EscapeStr(item.Title)),
                (nameof(SongDocument.SongAKATitles), EscapeStr(item.Title)),
                (nameof(SongDocument.SingerNames), EscapeStr(string.Join(Common.Constants.Separater, item.Singers))),
                (nameof(SongDocument.SingerAKANames), EscapeStr(string.Join(Common.Constants.Separater, item.Singers))),
                (nameof(SongDocument.Album), EscapeStr(item.Album)),
                (nameof(SongDocument.Genres), EscapeStr(string.Join(Common.Constants.Separater, item.Genres)))
            }.Where(u => !string.IsNullOrEmpty(u.Value)).Select(u => $"{u.Field}:{u.Value}");

            return string.Join(" OR ", result);
        }

        string EscapeStr(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : QueryParser.Escape(value);
        }

        static QueryParser Parser { get; } = LucenePool.GetQueryParser();
    }
}
