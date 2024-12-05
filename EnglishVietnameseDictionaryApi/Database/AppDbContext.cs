using EnglishVietnameseDictionaryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishVietnameseDictionaryApi.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Word> Words { get; set; } = default!;
        public DbSet<Meaning> Meanings { get; set; } = default!;
        public DbSet<Example> Examples { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Word>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.EnglishText)
                    .IsUnique();

                entity.Property(e => e.EnglishText).IsRequired();

            });

            modelBuilder.Entity<Meaning>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VietnameseText).IsRequired();
                entity.HasOne(e => e.Word)
                    .WithMany(e => e.Meanings)
                    .HasForeignKey(e => e.WordId);

                entity.Property(e => e.PartOfSpeech)
                    .HasConversion<string>();
            });

            modelBuilder.Entity<Example>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired();

                entity.HasOne(e => e.Meaning)
                    .WithMany(e => e.Examples)
                    .HasForeignKey(e => e.MeaningId);
            });
        }

    }
}
