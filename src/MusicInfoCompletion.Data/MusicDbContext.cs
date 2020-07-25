using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace MusicInfoCompletion.Data
{
    public class MusicDbContext : DbContext
    {
        public MusicDbContext(DbContextOptions<MusicDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Debugger.Launch();

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Singer>().HasIndex(nameof(Singer.Name), nameof(Singer.AKANames));

            modelBuilder.Entity<Song>().HasIndex(nameof(Song.Title), nameof(Song.AKATitles));

            modelBuilder.Entity<Album>().HasIndex(u => u.Title);

            modelBuilder.Entity<SingerSong>().HasIndex(nameof(SingerSong.SingerPk), nameof(SingerSong.SongPk)).IsUnique(true);

            modelBuilder.Entity<Genre>().HasIndex(u => u.Title).IsUnique(true);

            modelBuilder.Entity<GenreSong>().HasIndex(nameof(GenreSong.GenrePk), nameof(GenreSong.SongPk)).IsUnique(true);

            var tables = from Property in typeof(MusicDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         select Property.PropertyType into typeInfo
                         where typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(DbSet<>) && typeof(BaseModel).IsAssignableFrom(typeInfo.GetGenericArguments()[0])
                         select typeInfo.GetGenericArguments()[0];

            foreach (var table in tables)
            {
                modelBuilder.Entity(table).SetDefaultAndComputedValues();
            }
        }

        public DbSet<Singer> Singers { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<SingerSong> SingerSongs { get; set; }
    }
}