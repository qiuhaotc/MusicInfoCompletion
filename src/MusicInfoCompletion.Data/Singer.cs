using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicInfoCompletion.Data
{
    public class Singer : BaseModel
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string AKA1 { get; set; }

        [MaxLength(200)]
        public string AKA2 { get; set; }

        [MaxLength(200)]
        public string AKA3 { get; set; }

        [MaxLength(10000)]
        public string Description { get; set; }

        public IList<SingerSong> SingerSongs { get; set; }
    }
}
