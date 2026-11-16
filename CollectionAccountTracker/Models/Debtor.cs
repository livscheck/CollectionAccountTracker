namespace CollectionAccountTracker.Models;

// The person or business that owes the money.
public class Debtor
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    public ICollection<Account> Accounts { get; set; } = [];
}
