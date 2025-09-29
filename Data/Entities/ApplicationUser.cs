using Microsoft.AspNetCore.Identity;
using Donatello.Data.Entities;

namespace Donatello.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Board> Boards { get; set; } = new List<Board>();
    }
}
