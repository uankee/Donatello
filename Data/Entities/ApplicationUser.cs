using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Donatello.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;

        public ICollection<BoardUser> BoardUsers { get; set; } = new List<BoardUser>();
    }
}
