using System.ComponentModel.DataAnnotations;

namespace Donatello.Models
{
    public class InviteViewModel
    {
        [Required]
        public int BoardId { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "User Email")]
        public string Email { get; set; }
    }
}
