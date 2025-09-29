namespace Donatello.Data.Entities
{
    public class BoardUser
    {
        public int BoardId { get; set; }
        public Board Board { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public string Role { get; set; } = "User"; 
    }
}
