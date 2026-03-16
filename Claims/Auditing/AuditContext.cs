using Microsoft.EntityFrameworkCore;

namespace Claims.Auditing
{
    public class AuditContext : DbContext
    {
        public AuditContext(DbContextOptions<AuditContext> options) : base(options)
        {
        }
        public DbSet<ClaimAudit> ClaimAudits { get; set; }
        public DbSet<CoverAudit> CoverAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ClaimAudit>()
                .HasIndex(e => e.ClaimId);

            modelBuilder.Entity<CoverAudit>()
                .HasIndex(e => e.CoverId);
        }
    }
}
