using Microsoft.EntityFrameworkCore;
using Donatello.Data.Entities;

namespace Donatello.Data;

public class DonatelloDbContext : DbContext
{
    public DonatelloDbContext(DbContextOptions<DonatelloDbContext> options) : base(options) { }

    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<Column> Columns { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Board>().HasData(new Board { Id = 1, Title = "Personal Board", Description = "Example board (Donatello)" });

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
