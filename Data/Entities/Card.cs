using System;
using System.ComponentModel.DataAnnotations;

namespace Donatello.Data.Entities;

public class Card
{
    public int Id { get; set; }

    public int ColumnId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? AttachmentPath { get; set; }

    public int Order { get; set; } 

    public DateTime? DueDate { get; set; }
    public bool IsDone { get; set; }

    public Column? Column { get; set; }
}
