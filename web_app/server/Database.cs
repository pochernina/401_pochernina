using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace server
{
    class ImageItem
    {
        [Key]
        public int ImageId { get; set; }
        public int Hash { get; set; }
        public byte[] Image { get; set; }
        public string Emotions { get; set; }
    }

    class Database : DbContext
    {
        public DbSet<ImageItem> Items { get; set; }

        public Database()
        {
            Database.EnsureCreated();
        }

        public Database(DbContextOptions<Database> options) : base(options) {}

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite("Data Source=library.db");
        }
    }
}
