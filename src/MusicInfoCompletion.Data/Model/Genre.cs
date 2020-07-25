using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicInfoCompletion.Data
{
    public class Genre : BaseModel
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public IList<GenreSong> GenreSongs { get; set; }
    }
}
