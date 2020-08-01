using System;
using System.Collections.Generic;

namespace MusicInfoCompletion.Common
{
    public class BatchSearchResultItem : ResultBase
    {
        public SearchRequestItem SearchRequestItem { get; set; }
        public IEnumerable<SongDocument> SongInfos { get; set; } = Array.Empty<SongDocument>();
    }
}
