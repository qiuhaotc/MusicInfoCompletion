using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MusicInfoCompletion.Common;

namespace MusicInfoCompletion.Data
{
    public class Song : BaseModel
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(300)]
        public string AKATitles { get; set; }

        public Guid AlbumPk { get; set; }

        public byte[] Picture { get; set; }

        [Range(0, int.MaxValue)]
        public int Seconds { get; set; }

        public IList<SingerSong> SingerSongs { get; set; }

        public IList<GenreSong> GenreSongs { get; set; }

        public Album Album { get; set; }
    }
}
