namespace MusicInfoCompletion.Common
{
    public class SongDocument
    {
        public string SingerNames { get; set; }
        public string SingerAKANames { get; set; }
        public string SongTitle { get; set; }
        public string SongAKATitles { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public int SongSeconds { get; set; }
        public string SongPk { get; set; }
        public float Score { get; set; }
        public string ReleaseDate { get; set; }
        public byte[] Picture { get; set; }
        public string SingerDescription { get; set; }
        public string AlbumDescription { get; set; }
    }
}
