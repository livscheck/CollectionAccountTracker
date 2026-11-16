namespace CollectionAccountTracker.Models;

// The client the money is owed to; places accounts with the firm for collection.
public class Creditor
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    public ICollection<Account> Accounts { get; set; } = [];
}
