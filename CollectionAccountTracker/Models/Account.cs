namespace CollectionAccountTracker.Models;

public class Account
{
    public int Id { get; set; }

    public int DebtorId { get; set; }
    public required Debtor Debtor { get; set; }

    public int CreditorId { get; set; }
    public required Creditor Creditor { get; set; }

    public required DateTime DatePlaced { get; set; }
    public required AccountStatus Status { get; set; }
    public required decimal InitialBalance { get; set; }
    public decimal TotalPaid { get; set; }

    // What's still owed: InitialBalance - TotalPaid, calculated by SQL Server (see AppDbContext).
    // Works inside queries (Where, OrderBy, Sum). Only refreshed when the account is loaded or saved,
    // so after changing TotalPaid in code it's stale until SaveChangesAsync.
    public decimal Balance { get; private set; }

    // Percentage the firm keeps from each payment, e.g. 33.00 = 33% (not 0.33).
    public decimal ContingencyFee { get; set; }
    public DateTime? DateClosed { get; set; }

    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Note> Notes { get; set; } = [];
}
