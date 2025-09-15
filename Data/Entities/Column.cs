namespace Donatello.Data.Entities;

public class Column
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; }

    public int BoardId { get; set; }
    public Board? Board { get; set; }  

    public List<Card> Cards { get; set; } = new List<Card>();
    public string? AttachmentPath { get; set; }
    public string? Color { get; set; }
}
