using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicInfoCompletion.Data
{
    public class Album : BaseModel
    {
        [MaxLength(200)]
        public string Title { get; set; }

        public Guid SingerPk { get; set; }

        public DateTime ReleaseDate { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public IList<Song> Songs { get; set; }
    }
}
