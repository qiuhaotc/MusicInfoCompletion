using System.Collections.Generic;

namespace MusicInfoCompletion.Common
{
    public class SearchResult : ResultBase
    {
        public IEnumerable<SongDocument> SongInfos { get; set; }
    }
}
