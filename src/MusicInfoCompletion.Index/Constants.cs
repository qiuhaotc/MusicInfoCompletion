using Lucene.Net.Util;

namespace MusicInfoCompletion.Index
{
    public static class Constants
    {
        public const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        public const int ReadWriteLockTimeOutMilliseconds = 60000; // 60 seconds
    }
}
