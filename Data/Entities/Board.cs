using Microsoft.EntityFrameworkCore;

namespace Donatello.Data.Entities
{
    public class Board
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int Order { get; set; } = 0;

        public ICollection<Column> Columns { get; set; } = new List<Column>();

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
