namespace CollectionAccountTracker.Models;

public class AccountStatus
{
    public string Code { get; private init; } = string.Empty;
    public string Name { get; private init; } = string.Empty;
    public string Description { get; private init; } = string.Empty;

    // Private: no code outside this class can create a new status.
    private AccountStatus() { }

    public static readonly AccountStatus Placed = new()
    {
        Code = "PLACED",
        Name = "Placed",
        Description = "Account has been placed for collection"
    };

    public static readonly AccountStatus DemandSent = new()
    {
        Code = "DEMAND",
        Name = "Demand Sent",
        Description = "Demand letter has been sent to the debtor"
    };

    public static readonly AccountStatus PaymentPlan = new()
    {
        Code = "PLAN",
        Name = "Payment Plan",
        Description = "Debtor is making payments under an agreed plan"
    };

    public static readonly AccountStatus SuitFiled = new()
    {
        Code = "SUIT",
        Name = "Suit Filed",
        Description = "Lawsuit has been filed against the debtor"
    };

    public static readonly AccountStatus Judgment = new()
    {
        Code = "JUDGMT",
        Name = "Judgment",
        Description = "Court has entered a judgment on the account"
    };

    public static readonly AccountStatus Paid = new()
    {
        Code = "PAID",
        Name = "Paid",
        Description = "Balance has been paid in full"
    };

    public static readonly AccountStatus Closed = new()
    {
        Code = "CLOSED",
        Name = "Closed",
        Description = "Account is closed with no further action"
    };

    public static readonly AccountStatus[] All = [Placed, DemandSent, PaymentPlan, SuitFiled, Judgment, Paid, Closed];

    // For codes that came from the database, which are always valid. Throws on an unknown code.
    public static AccountStatus FromCode(string code) => All.First(s => s.Code == code);

    // For codes that came from a user or a URL. Ignores case; returns null on an unknown code.
    public static AccountStatus? TryFromCode(string? code) =>
        All.FirstOrDefault(s => s.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
}
