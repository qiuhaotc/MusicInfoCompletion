using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using MusicInfoCompletion.Common;
using MusicInfoCompletion.Data;
using CommonConstants = MusicInfoCompletion.Common.Constants;
using IndexConstants = MusicInfoCompletion.Index.Constants;

namespace MusicInfoCompletion.Index
{
    public static class LucenePool
    {
        public static SongDocument GetSongDocument(this Song song)
        {
            var docuemnt = new SongDocument
            {
                Album = song.Album?.Title,
                Genres = string.Join(CommonConstants.Separater, song.GenreSongs?.Select(u => u.Genre?.Title) ?? Enumerable.Empty<string>()),
                SingerAKANames = string.Join(CommonConstants.Separater, song.SingerSongs?.Select(u => u.Singer?.AKANames) ?? Enumerable.Empty<string>()),
                SingerNames = string.Join(CommonConstants.Separater, song.SingerSongs?.Select(u => u.Singer?.Name) ?? Enumerable.Empty<string>()),
                SongAKATitles = song.AKATitles.ToStringSafe(),
                SongTitle = song.Title.ToStringSafe(),
                SongPk = song.Pk.ToString(),
                SongSeconds = song.Seconds,
                AlbumDescription = song.Album?.Description,
                Picture = song.Picture,
                ReleaseDate = song.Album.ReleaseDate?.ToString(CommonConstants.DateTimeFormat),
                SingerDescription = string.Join(CommonConstants.Separater, song.SingerSongs?.Select(u => u.Singer?.Description) ?? Enumerable.Empty<string>()),
            };

            return docuemnt;
        }

        public static Document GetDocument(this SongDocument songDocument)
        {
            return new Document
            {
                new TextField(nameof(songDocument.Album), songDocument.Album.ToStringSafe(), Field.Store.YES),
                // StringField indexes but doesn't tokenize
                new StringField(nameof(songDocument.SongPk), songDocument.SongPk.ToStringSafe(), Field.Store.YES),
                new TextField(nameof(songDocument.Genres) , songDocument.Genres.ToStringSafe(), Field.Store.YES),
                new TextField(nameof(songDocument.SingerAKANames), songDocument.SingerAKANames.ToStringSafe(), Field.Store.YES),
                new TextField(nameof(songDocument.SingerNames), songDocument.SingerNames.ToStringSafe(), Field.Store.YES),
                new TextField(nameof(songDocument.SongAKATitles), songDocument.SongAKATitles.ToStringSafe(), Field.Store.YES),
                new TextField(nameof(songDocument.SongTitle), songDocument.SongTitle.ToStringSafe(), Field.Store.YES),
                new Int32Field(nameof(songDocument.SongSeconds), songDocument.SongSeconds, Field.Store.YES),
                new StoredField(nameof(songDocument.AlbumDescription), songDocument.AlbumDescription.ToStringSafe()),
                new StoredField(nameof(songDocument.SingerDescription), songDocument.SingerDescription.ToStringSafe()),
                new StoredField(nameof(songDocument.ReleaseDate), songDocument.ReleaseDate.ToStringSafe()),
                new StoredField(nameof(songDocument.Picture), songDocument.Picture ?? Array.Empty<byte>()),
            };
        }

        internal static void SaveResults(string luceneIndex, bool forceCommitChanges)
        {
            ReadWriteLock.TryEnterWriteLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                if ((forceCommitChanges || shouldCommitChanges) && IndexWritesPool.TryGetValue(luceneIndex, out var indexWriter))
                {
                    indexWriter.Commit();
                    shouldCommitChanges = false;
                }
            }
            finally
            {
                ReadWriteLock.ExitWriteLock();
            }
        }

        internal static void SaveResultsAndClearLucenePool(string luceneIndex)
        {
            ReadWriteLock.TryEnterWriteLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                if (IndexReaderPool.TryRemove(luceneIndex, out var indexReader))
                {
                    indexReader.Dispose();
                }

                if (IndexWritesPool.TryRemove(luceneIndex, out var indexWriter))
                {
                    indexWriter.Dispose();
                }

                IndexSearcherPool.TryRemove(luceneIndex, out _);

                IndexGotChanged.AddOrUpdate(luceneIndex, u => 0, (u, v) => 0);
            }
            finally
            {
                ReadWriteLock.ExitWriteLock();
            }
        }

        public static (Document Document, float Score)[] Search(string luceneIndex, Query query, int maxResults)
        {
            ReadWriteLock.TryEnterReadLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                return SearchDocuments(luceneIndex, query, maxResults);
            }
            finally
            {
                ReadWriteLock.ExitReadLock();
            }
        }

        public static SongDocument[] SearchSongs(string luceneIndex, Query query, int maxResults)
        {
            ReadWriteLock.TryEnterReadLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                return SearchDocuments(luceneIndex, query, maxResults).Select(doc => GetSongDocument(doc)).ToArray();
            }
            finally
            {
                ReadWriteLock.ExitReadLock();
            }
        }

        public static QueryParser GetQueryParser()
        {
            return new QueryParser(Constants.AppLuceneVersion, nameof(SongDocument.SongTitle), GetAnalyzer());
        }

        public static Analyzer GetAnalyzer()
        {
            return new StandardAnalyzer(Constants.AppLuceneVersion, CharArraySet.EMPTY_SET); // TODO: Implement SmartChineseAnalyzer, app exit with no error, don't know why
        }

        internal static void BuildIndex(string luceneIndex, bool triggerMerge, bool applyAllDeletes, IEnumerable<Document> documents, bool needFlush)
        {
            ReadWriteLock.TryEnterReadLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                var writer = CreateOrGetIndexWriter(luceneIndex);
                writer.AddDocuments(documents);

                if (needFlush)
                {
                    writer.Flush(triggerMerge, applyAllDeletes);
                }

                IndexGotChanged.AddOrUpdate(luceneIndex, u => 0, (u, v) => v + 1);
                shouldCommitChanges = true;
            }
            finally
            {
                ReadWriteLock.ExitReadLock();
            }
        }

        internal static List<SongDocument> GetAllIndexes(string luceneIndex)
        {
            ReadWriteLock.TryEnterReadLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                var query = new MatchAllDocsQuery();
                var filter = new FieldValueFilter(nameof(SongDocument.SongPk));

                return SearchDocuments(luceneIndex, query, int.MaxValue, filter).Select(doc => GetSongDocument(doc)).ToList();
            }
            finally
            {
                ReadWriteLock.ExitReadLock();
            }
        }

        internal static void DeleteIndex(string luceneIndex, params Query[] searchQueries)
        {
            ReadWriteLock.TryEnterReadLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                var indexWriter = CreateOrGetIndexWriter(luceneIndex);
                indexWriter.DeleteDocuments(searchQueries);

                IndexGotChanged.AddOrUpdate(luceneIndex, u => 0, (u, v) => v + 1);
                shouldCommitChanges = true;
            }
            finally
            {
                ReadWriteLock.ExitReadLock();
            }
        }

        internal static void DeleteIndex(string luceneIndex, params Term[] terms)
        {
            ReadWriteLock.TryEnterReadLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                var indexWriter = CreateOrGetIndexWriter(luceneIndex);
                indexWriter.DeleteDocuments(terms);

                IndexGotChanged.AddOrUpdate(luceneIndex, u => 0, (u, v) => v + 1);
                shouldCommitChanges = true;
            }
            finally
            {
                ReadWriteLock.ExitReadLock();
            }
        }

        internal static void UpdateIndex(string luceneIndex, Term term, Document document)
        {
            ReadWriteLock.TryEnterReadLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                var indexWriter = CreateOrGetIndexWriter(luceneIndex);
                indexWriter.UpdateDocument(term, document);

                IndexGotChanged.AddOrUpdate(luceneIndex, u => 0, (u, v) => v + 1);
                shouldCommitChanges = true;
            }
            finally
            {
                ReadWriteLock.ExitReadLock();
            }
        }

        internal static void DeleteAllIndex(string luceneIndex)
        {
            ReadWriteLock.TryEnterReadLock(IndexConstants.ReadWriteLockTimeOutMilliseconds);

            try
            {
                var indexWriter = CreateOrGetIndexWriter(luceneIndex);
                indexWriter.DeleteAll();
                indexWriter.Commit();

                IndexGotChanged.AddOrUpdate(luceneIndex, u => 0, (u, v) => v + 1);
            }
            finally
            {
                ReadWriteLock.ExitReadLock();
            }
        }

        static bool shouldCommitChanges = false;

        static readonly object syncLockForWriter = new object();

        static IndexWriter CreateOrGetIndexWriter(string luceneIndex)
        {
            IndexWriter indexWriter;

            if (!IndexWritesPool.TryGetValue(luceneIndex, out indexWriter))
            {
                lock (syncLockForWriter)
                {
                    if (!IndexWritesPool.TryGetValue(luceneIndex, out indexWriter))
                    {
                        var dir = FSDirectory.Open(luceneIndex);
                        //create an analyzer to process the text
                        //create an index writer
                        var indexConfig = new IndexWriterConfig(Constants.AppLuceneVersion, GetAnalyzer());

                        indexWriter = new IndexWriter(dir, indexConfig);
                        IndexWritesPool.TryAdd(luceneIndex, indexWriter);
                    }
                }
            }

            return indexWriter;
        }

        static readonly object syncLockForSearcher = new object();

        static IndexSearcher CreateOrGetIndexSearcher(string luceneIndex)
        {
            IndexSearcher indexSearcher;

            if (!IndexSearcherPool.TryGetValue(luceneIndex, out indexSearcher) || IndexGotChanged.TryGetValue(luceneIndex, out var indexChangedTimes) && indexChangedTimes > 0)
            {
                lock (syncLockForSearcher)
                {
                    if (!IndexSearcherPool.TryGetValue(luceneIndex, out indexSearcher))
                    {
                        indexSearcher = new IndexSearcher(CreateOrGetIndexReader(luceneIndex, false));
                        IndexSearcherPool.TryAdd(luceneIndex, indexSearcher);
                    }
                    else if (IndexGotChanged.TryGetValue(luceneIndex, out indexChangedTimes) && indexChangedTimes > 0)
                    {
                        indexSearcher = new IndexSearcher(CreateOrGetIndexReader(luceneIndex, true));
                        IndexSearcherPool.AddOrUpdate(luceneIndex, indexSearcher, (u, v) => indexSearcher);
                        IndexGotChanged.AddOrUpdate(luceneIndex, 0, (u, v) => 0);
                    }
                }
            }

            if (!indexSearcher.IndexReader.TryIncRef())
            {
                return CreateOrGetIndexSearcher(luceneIndex);
            }

            return indexSearcher;
        }

        static (Document Document, float Score)[] SearchDocuments(string luceneIndex, Query query, int maxResult, FieldValueFilter fieldValueFilter = null)
        {
            (Document Document, float Score)[] documents = null;
            IndexSearcher indexSearcher = null;

            try
            {
                indexSearcher = CreateOrGetIndexSearcher(luceneIndex);

                if (fieldValueFilter != null)
                {
                    documents = indexSearcher.Search(query, fieldValueFilter, maxResult).ScoreDocs.Select(hit => (indexSearcher.Doc(hit.Doc), hit.Score)).ToArray();
                }
                else
                {
                    documents = indexSearcher.Search(query, maxResult).ScoreDocs.Select(hit => (indexSearcher.Doc(hit.Doc), hit.Score)).ToArray();
                }
            }
            finally
            {
                indexSearcher.IndexReader.DecRef();
            }

            return documents ?? Array.Empty<(Document Document, float Score)>();
        }

        static readonly object syncLockForReader = new object();

        static IndexReader CreateOrGetIndexReader(string luceneIndex, bool forceRefresh)
        {
            IndexReader indexReader;

            if (!IndexReaderPool.TryGetValue(luceneIndex, out indexReader) || forceRefresh)
            {
                lock (syncLockForReader)
                {
                    if (!IndexReaderPool.TryGetValue(luceneIndex, out indexReader))
                    {
                        indexReader = CreateOrGetIndexWriter(luceneIndex).GetReader(true);
                        IndexReaderPool.TryAdd(luceneIndex, indexReader);
                    }
                    else if (forceRefresh)
                    {
                        indexReader.DecRef(); // Dispose safely
                        indexReader = CreateOrGetIndexWriter(luceneIndex).GetReader(true);
                        IndexReaderPool.AddOrUpdate(luceneIndex, indexReader, (u, v) => indexReader);
                    }
                }
            }

            return indexReader;
        }

        static string ToStringSafe(this string value)
        {
            return value ?? string.Empty;
        }

        static SongDocument GetSongDocument((Document Document, float Score) doc)
        {
            return new SongDocument
            {
                SongPk = doc.Document.Get(nameof(SongDocument.SongPk)),
                SongAKATitles = doc.Document.Get(nameof(SongDocument.SongAKATitles)),
                SingerNames = doc.Document.Get(nameof(SongDocument.SingerNames)),
                Album = doc.Document.Get(nameof(SongDocument.Album)),
                Genres = doc.Document.Get(nameof(SongDocument.Genres)),
                SongSeconds = doc.Document.GetField(nameof(SongDocument.SongSeconds)).GetInt32Value() ?? 0,
                SingerAKANames = doc.Document.Get(nameof(SongDocument.SingerAKANames)),
                Score = doc.Score,
                AlbumDescription = doc.Document.Get(nameof(SongDocument.AlbumDescription)),
                Picture = doc.Document.GetBinaryValue(nameof(SongDocument.Picture)).Bytes,
                ReleaseDate = doc.Document.Get(nameof(SongDocument.ReleaseDate)),
                SingerDescription = doc.Document.Get(nameof(SongDocument.SingerDescription)),
                SongTitle = doc.Document.Get(nameof(SongDocument.SongTitle)),
            };
        }

        public static ConcurrentDictionary<string, IndexWriter> IndexWritesPool { get; } = new ConcurrentDictionary<string, IndexWriter>();
        static ConcurrentDictionary<string, IndexSearcher> IndexSearcherPool { get; } = new ConcurrentDictionary<string, IndexSearcher>();
        static ConcurrentDictionary<string, int> IndexGotChanged { get; } = new ConcurrentDictionary<string, int>();
        static ConcurrentDictionary<string, IndexReader> IndexReaderPool { get; } = new ConcurrentDictionary<string, IndexReader>();
        static ReaderWriterLockSlim ReadWriteLock { get; } = new ReaderWriterLockSlim();
    }
}
