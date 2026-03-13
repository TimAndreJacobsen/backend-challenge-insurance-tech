using Claims.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Claims.Persistence;

/// <summary>
/// MongoDB-backed DbContext for Claims and Covers collections.
/// </summary>
public class ClaimsContext : DbContext
{
    public DbSet<Claim> Claims { get; init; }
    public DbSet<Cover> Covers { get; init; }

    public ClaimsContext(DbContextOptions<ClaimsContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Claim>().ToCollection("claims");
        modelBuilder.Entity<Cover>().ToCollection("covers");
    }
}
