using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Donatello.Data.Entities;

namespace Donatello.Data;

public class DonatelloDbContext : IdentityDbContext<ApplicationUser>
{
    public DonatelloDbContext(DbContextOptions<DonatelloDbContext> options) : base(options) { }

    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<Column> Columns { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;
    public DbSet<BoardUser> BoardUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BoardUser>()
            .HasKey(bu => new { bu.BoardId, bu.UserId });

        modelBuilder.Entity<BoardUser>()
            .HasOne(bu => bu.Board)
            .WithMany(b => b.BoardUsers)
            .HasForeignKey(bu => bu.BoardId);

        modelBuilder.Entity<BoardUser>()
            .HasOne(bu => bu.User)
            .WithMany(u => u.BoardUsers)
            .HasForeignKey(bu => bu.UserId);
    }
}
