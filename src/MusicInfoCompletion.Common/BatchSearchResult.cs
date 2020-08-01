using System.Collections.Generic;

namespace MusicInfoCompletion.Common
{
    public class BatchSearchResult : ResultBase
    {
        public IList<BatchSearchResultItem> SearchResults { get; set; } = new List<BatchSearchResultItem>();
    }
}
