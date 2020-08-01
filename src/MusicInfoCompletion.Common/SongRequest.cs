using System;
using System.Collections.Generic;

namespace MusicInfoCompletion.Common
{
    public class SongRequest
    {
        public Guid SongPk { get; set; }
        public IEnumerable<Guid> SingerPks { get; set; }
        public IEnumerable<Guid> GenrePks { get; set; }
        public string Title { get; set; }
        public string AKATitles { get; set; }
        public Guid AlbumPk { get; set; }
        public byte[] Picture { get; set; }
        public int Seconds { get; set; }
        public IEnumerable<string> Singers { get; set; }
        public IEnumerable<string> Genres { get; set; }
        public string Album { get; set; }
    }
}
