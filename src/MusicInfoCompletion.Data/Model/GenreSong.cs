using System;

namespace MusicInfoCompletion.Data
{
    public class GenreSong : BaseModel
    {
        public Guid GenrePk { get; set; }
        public Guid SongPk { get; set; }
        public Genre Genre { get; set; }
        public Song Song { get; set; }
    }
}
