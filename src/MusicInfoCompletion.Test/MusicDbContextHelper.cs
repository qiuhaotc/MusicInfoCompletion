using System;
using Microsoft.EntityFrameworkCore;
using MusicInfoCompletion.Data;

namespace MusicInfoCompletion.Test
{
    public static class MusicDbContextHelper
    {
        public static MusicDbContext GetMusicDbContext() => new MusicDbContext(new DbContextOptionsBuilder<MusicDbContext>()
                .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
                .Options);
    }
}
