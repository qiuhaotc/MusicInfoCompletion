using System.Collections.Generic;

namespace MusicInfoCompletion.Common
{
    public class SingleSearchResult : ResultBase
    {
        public IEnumerable<SongDocument> SongInfos { get; set; }
    }
}
