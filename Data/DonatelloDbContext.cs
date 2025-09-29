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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Board>()
            .HasMany(b => b.Users)
            .WithMany(u => u.Boards)
            .UsingEntity<Dictionary<string, object>>(
                "BoardUser",
                j => j.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId").HasConstraintName("FK_BoardUser_User_UserId"),
                j => j.HasOne<Board>().WithMany().HasForeignKey("BoardId").HasConstraintName("FK_BoardUser_Board_BoardId"),
                j =>
                {
                    j.HasKey("BoardId", "UserId");
                    j.ToTable("BoardUsers");
                });
        modelBuilder.Entity<Board>().HasData(new Board { Id = 1, Title = "Personal Board", Description = "Example board (Donatello)", Order = 0 });

        modelBuilder.Entity<Column>().HasData(
            new Column { Id = 1, BoardId = 1, Title = "To do", Order = 0 },
            new Column { Id = 2, BoardId = 1, Title = "In progress", Order = 1 },
            new Column { Id = 3, BoardId = 1, Title = "Done", Order = 2 }
        );

        modelBuilder.Entity<Card>().HasData(
            new Card { Id = 1, ColumnId = 1, Title = "Make project plan", Order = 0, Description = "Outline tasks and milestones" },
            new Card { Id = 2, ColumnId = 1, Title = "Buy milk", Order = 1 },
            new Card { Id = 3, ColumnId = 2, Title = "Implement BoardsController", Order = 0 }
        );
    }
}
