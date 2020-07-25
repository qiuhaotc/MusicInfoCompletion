using System;

namespace MusicInfoCompletion.Data
{
    public class SingerSong : BaseModel
    {
        public Guid SingerPk { get; set; }
        public Guid SongPk { get; set; }
        public Singer Singer { get; set; }
        public Song Song { get; set; }
    }
}
