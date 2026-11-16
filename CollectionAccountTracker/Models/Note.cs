namespace CollectionAccountTracker.Models;

public class Note
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public required string Content { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.Now;
}
