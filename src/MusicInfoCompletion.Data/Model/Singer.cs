using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicInfoCompletion.Data
{
    public class Singer : BaseModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(300)]
        public string AKANames { get; set; }

        [MaxLength(10000)]
        public string Description { get; set; }

        public IList<SingerSong> SingerSongs { get; set; }
    }
}
