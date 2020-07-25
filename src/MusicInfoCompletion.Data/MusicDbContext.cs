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
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Singer>().HasIndex(u => u.Name);
            modelBuilder.Entity<Song>().HasIndex(u => u.Title);
            modelBuilder.Entity<Album>().HasIndex(u => u.Title);
        }

        public DbSet<Singer> Singers { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<SingerSong> SingerSongs { get; set; }
    }
}