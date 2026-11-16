using CollectionAccountTracker.Models;

namespace CollectionAccountTracker.Data;

// Fills an empty database with fake accounts. All names are made up; never use real debtor data.
public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Accounts.Any()) return;

        // Fixed seed so every run (and every machine) gets the same data
        var rng = new Random(42);

        string[] firstNames = ["Jordan", "Casey", "Morgan", "Riley", "Taylor", "Avery", "Quinn", "Drew", "Jamie", "Parker"];
        string[] lastNames = ["Smith", "Nguyen", "Garcia", "Johnson", "Kowalski", "Brown", "Patel", "Miller", "Lee", "Schmidt"];
        PaymentMethod[] methods = [PaymentMethod.Check, PaymentMethod.Card, PaymentMethod.Transfer];

        // Fake creditors and their contingency fee percentages
        (Creditor Creditor, decimal ContingencyFee)[] creditors =
        [
            (new Creditor { Name = "Lakeshore Medical", ContactName = "Dana Price" }, 30.00m),
            (new Creditor { Name = "Badger Credit Union", ContactName = "Sam Becker" }, 25.00m),
            (new Creditor { Name = "Northwoods Auto Finance", ContactName = "Lee Wells" }, 33.00m),
            (new Creditor { Name = "Fox Valley Utilities", ContactName = "Pat Moreno" }, 35.00m)
        ];

        // 25 accounts pick from 18 debtors, so some debtors have more than one account.
        // Only debtors that end up with an account get saved.
        var debtors = Enumerable.Range(1, 18)
            .Select(i => new Debtor
            {
                Name = $"{firstNames[rng.Next(firstNames.Length)]} {lastNames[rng.Next(lastNames.Length)]}",
                Phone = $"555-01{i:D2}"
            })
            .ToList();

        AccountStatus[] openStatuses =
        [
            AccountStatus.Placed, AccountStatus.DemandSent, AccountStatus.PaymentPlan,
            AccountStatus.SuitFiled, AccountStatus.Judgment
        ];

        var accounts = Enumerable.Range(1, 25).Select(_ =>
        {
            var placed = DateTime.Today.AddDays(-rng.Next(60, 365));
            var (creditor, contingencyFee) = creditors[rng.Next(creditors.Length)];

            // 0–4 payments in the last 90 days, never before the account was placed,
            // so this month and last month both have remittance data
            var maxDaysAgo = Math.Min(90, (DateTime.Today - placed).Days);
            var payments = Enumerable.Range(0, rng.Next(0, 5)).Select(_ =>
            {
                var amount = Math.Round((decimal)(rng.NextDouble() * 300 + 25), 2);
                return new Payment
                {
                    Amount = amount,
                    FirmFee = FeeCalculator.FirmFee(amount, contingencyFee),
                    Method = methods[rng.Next(methods.Length)],
                    DatePaid = DateTime.Today.AddDays(-rng.Next(0, maxDaysAgo))
                };
            }).ToList();

            var totalPaid = payments.Sum(p => p.Amount);

            // Always leave a balance, so no seeded account is accidentally paid off
            var initialBalance = Math.Round((decimal)(rng.NextDouble() * 9500 + 150), 2) + totalPaid;

            return new Account
            {
                Debtor = debtors[rng.Next(debtors.Count)],
                Creditor = creditor,
                DatePlaced = placed,
                Status = openStatuses[rng.Next(openStatuses.Length)],
                InitialBalance = initialBalance,
                TotalPaid = totalPaid,
                ContingencyFee = contingencyFee,
                Payments = payments,
                Notes = [new Note { Content = $"Placed for collection by {creditor.Name}.", DateCreated = placed }]
            };
        }).ToList();

        db.Accounts.AddRange(accounts);
        db.SaveChanges();
    }
}
