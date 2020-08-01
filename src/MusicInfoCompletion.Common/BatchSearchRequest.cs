using System.Collections.Generic;

namespace MusicInfoCompletion.Common
{
    public class BatchSearchRequest
    {
       public IList<SearchRequestItem> SearchRequestItems { get; set; }
    }
}
