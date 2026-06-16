using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nstuning_api.Models.Admin;
using nstuning_api.Models.Auth;

namespace nstuning_api.Models
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<User, IdentityRole, string>(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<DynoRun> DynoRuns { get; set; }
        public DbSet<DynoRunReport> DynoRunReports { get; set; }
        public DbSet<ContentImage> ContentImages { get; set; }
        public DbSet<ContentImageVariant> ContentImageVariants { get; set; }
        public DbSet<DailyStatSnapshot> DailyStatSnapshots { get; set; }
        public DbSet<CarBrand> CarBrands { get; set; }
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<CarVariant> CarVariants { get; set; }
        public DbSet<CarEngine> CarEngines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DailyStatSnapshot>()
                .HasIndex(s => s.Date)
                .IsUnique();

            modelBuilder.Entity<CarBrand>()
                .HasIndex(b => b.Name)
                .IsUnique();

            modelBuilder.Entity<CarModel>()
                .HasIndex(m => new { m.BrandId, m.Name })
                .IsUnique();

            modelBuilder.Entity<CarModel>()
                .HasOne(m => m.Brand)
                .WithMany()
                .HasForeignKey(m => m.BrandId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CarVariant>()
                .HasIndex(v => new { v.ModelId, v.Name })
                .IsUnique();

            modelBuilder.Entity<CarVariant>()
                .HasOne(v => v.Model)
                .WithMany()
                .HasForeignKey(v => v.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CarEngine>()
                .HasIndex(e => new { e.VariantId, e.Name })
                .IsUnique();

            modelBuilder.Entity<CarEngine>()
                .HasOne(e => e.Variant)
                .WithMany()
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynoRun>()
                .HasIndex(d => d.Slug)
                .IsUnique();

            modelBuilder.Entity<DynoRun>()
                .HasOne(d => d.Report)
                .WithOne(r => r.DynoRun)
                .HasForeignKey<DynoRunReport>(r => r.DynoRunId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynoRun>()
                .HasOne(d => d.CoverImage)
                .WithMany()
                .HasForeignKey(d => d.CoverImageId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ContentImageVariant>()
                .HasOne(v => v.ContentImage)
                .WithMany(i => i.Variants)
                .HasForeignKey(v => v.ContentImageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
