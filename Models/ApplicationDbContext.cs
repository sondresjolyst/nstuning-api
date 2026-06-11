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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DynoRun>()
                .HasIndex(d => d.Slug)
                .IsUnique();

            modelBuilder.Entity<DynoRun>()
                .HasOne(d => d.Report)
                .WithOne(r => r.DynoRun)
                .HasForeignKey<DynoRunReport>(r => r.DynoRunId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
