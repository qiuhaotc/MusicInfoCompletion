using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicInfoCompletion.Data
{
    public class Song : BaseModel
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public Guid AlbumPk { get; set; }

        public byte[] Picture { get; set; }

        public IList<SingerSong> SingerSongs { get; set; }

        public Album Album { get; set; }
    }
}
