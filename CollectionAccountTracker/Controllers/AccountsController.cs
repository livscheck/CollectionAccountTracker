using CollectionAccountTracker.Data;          // AppDbContext, FeeCalculator, MonthRange
using CollectionAccountTracker.Models;        // Account, Payment, AccountStatus
using CollectionAccountTracker.Models.DTO;    // your DTOs
using Microsoft.AspNetCore.Mvc;               // ControllerBase, [ApiController], [HttpGet]...
using Microsoft.EntityFrameworkCore;          // ToListAsync, FirstOrDefaultAsync...

namespace CollectionAccountTracker.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(AppDbContext db) : ControllerBase
{

    /*----- HELPER FUNCTIONS -----*/
    // Shared helpers live outside the controller so the API, pages and reports all use the same ones:
    //   a.Balance                   computed column on Account (InitialBalance - TotalPaid); works inside queries
    //   AccountStatus.TryFromCode   status lookup for a code from the URL; null if unknown
    //   MonthRange.For              first of the month → first of next month, for date filters
    //   FeeCalculator.FirmFee       the fee math


    /*----- API ENDPOINTS -----*/
    // Before ToListAsync/FirstOrDefaultAsync: IQueryable → one SQL statement; only columns and arithmetic.
    // After: IEnumerable → in-memory list; any C# (DTOs, Status.Code). Marked "Runs in SQL/memory" below.

    // Get account by ID
    // GET /api/accounts/5
    // id comes from the URL; added to the class route → api/accounts/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountDetailDTO>> GetById(int id)
    {
        // Runs in SQL. Status is selected whole, because EF can't read .Code/.Name inside SQL.
        var account = await db.Accounts
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                DebtorName = a.Debtor.Name,
                CreditorName = a.Creditor.Name,
                a.Balance,
                a.TotalPaid,
                a.ContingencyFee,
                a.Status,
                a.DatePlaced,
                Payments = a.Payments
                    .OrderByDescending(p => p.DatePaid)
                    .Select(p => new PaymentDTO(
                        p.Id,
                        p.Amount,
                        p.FirmFee,
                        p.DatePaid,
                        p.Method))
                    .ToList()
            })
            .FirstOrDefaultAsync();

        // 404
        if (account == null)
            return NotFound();

        // Runs in memory: now Status.Code and Status.Name can be read
        // 200 + JSON
        return new AccountDetailDTO(
            account.Id,
            account.DebtorName,
            account.CreditorName,
            account.Balance,
            account.TotalPaid,
            account.ContingencyFee,
            account.Status.Code,
            account.Status.Name,
            account.DatePlaced,
            account.Payments);
    }

    // Search and filter accounts
    // GET /api/accounts?search=smith&status=SUIT
    // search and status come from the query string; no route template → just api/accounts
    [HttpGet]
    public async Task<ActionResult<List<AccountDTO>>> GetAll(string? search, string? status)
    {
        // IQueryable: each .Where adds to the SQL. IEnumerable would load every account
        // and run the filters in C# (and a.Debtor/a.Payments wouldn't be loaded).
        IQueryable<Account> query = db.Accounts;

        // Only filter when the parameter was given
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a =>
                a.Debtor.Name.Contains(search) ||
                a.Creditor.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var accountStatus = AccountStatus.TryFromCode(status);

            // 400
            if (accountStatus == null)
            {
                var validCodes = string.Join(", ", AccountStatus.All.Select(s => s.Code));
                return BadRequest($"Unknown status '{status}'. Use one of: {validCodes}.");
            }

            query = query.Where(a => a.Status == accountStatus);
        }

        // Nothing has hit the database yet; ToListAsync sends one SQL query with all the filters
        var accounts = await query
            .OrderByDescending(a => a.Balance)
            .Select(a => new
            {
                a.Id,
                DebtorName = a.Debtor.Name,
                CreditorName = a.Creditor.Name,
                a.Balance,
                a.Status,
                a.DatePlaced,
                PaymentCount = a.Payments.Count,             // subquery, same statement (no N+1)
                // Nullable cast: no payments → null instead of an error
                LastPaymentDate = a.Payments.Max(p => (DateTime?)p.DatePaid)
            })
            .ToListAsync();

        // 200 + JSON
        return accounts
            .Select(a => new AccountDTO(
                a.Id,
                a.DebtorName,
                a.CreditorName,
                a.Balance,
                a.Status.Code,
                a.Status.Name,
                a.DatePlaced,
                a.PaymentCount,
                a.LastPaymentDate))
            .ToList();
    }

    // Totals for the stat cards
    // GET /api/accounts/dashboard
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDTO>> GetDashboard()
    {
        // Runs in SQL: one row per status with its count and remaining balance
        var groups = await db.Accounts
            .GroupBy(a => a.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                TotalBalance = g.Sum(a => a.Balance)
            })
            .ToListAsync();

        // Runs in memory: put the statuses in workflow order (Placed, Demand Sent, ...)
        var byStatus = groups
            .OrderBy(g => Array.IndexOf(AccountStatus.All, g.Status))
            .Select(g => new StatusSummaryDTO(
                g.Status.Code,
                g.Status.Name,
                g.Count,
                g.TotalBalance))
            .ToList();

        // Exclusive end date: start <= DatePaid < end catches every time on the last day of the month
        var (monthStart, monthEnd) = MonthRange.For(DateTime.Today);

        var collectedThisMonth = await db.Payments
            .Where(p => p.DatePaid >= monthStart && p.DatePaid < monthEnd)
            .SumAsync(p => p.Amount);

        // 200 + JSON
        return new DashboardDTO(byStatus, collectedThisMonth);
    }

    // Post a payment
    // POST /api/accounts/5/payments   body: { "amount": 100.00, "method": "Check" }
    // request comes from the JSON body.
    // [ApiController] returns 400 automatically if the body fails CreatePaymentDTO's validation.
    [HttpPost("{id:int}/payments")]
    public async Task<ActionResult<PaymentDTO>> AddPayment(int id, CreatePaymentDTO request)
    {
        var account = await db.Accounts.FindAsync(id);

        // 404
        if (account == null)
            return NotFound();

        // Validation already guaranteed these aren't null
        var amount = request.Amount!.Value;

        // Fresh: the account was just loaded from the database
        var balance = account.Balance;

        // 400
        if (amount > balance)
            return BadRequest($"Payment can't be more than the remaining balance of {balance:C}.");

        var payment = new Payment
        {
            AccountId = id,
            Amount = amount,
            // Calculated on the server and stored, so a later rate change never alters past remittances
            FirmFee = FeeCalculator.FirmFee(amount, account.ContingencyFee),
            Method = request.Method!.Value,
            DatePaid = DateTime.Now
        };

        db.Payments.Add(payment);
        account.TotalPaid += amount;

        // Paid off exactly → close the account
        if (account.TotalPaid == account.InitialBalance)
        {
            account.Status = AccountStatus.Paid;
            account.DateClosed = DateTime.Now;
        }

        // One SaveChangesAsync: the payment and the account update succeed or fail together
        await db.SaveChangesAsync();

        // 201 + a Location header pointing at GET /api/accounts/5
        return CreatedAtAction(
            nameof(GetById),
            new { id },
            new PaymentDTO(
                payment.Id,
                payment.Amount,
                payment.FirmFee,
                payment.DatePaid,
                payment.Method));
    }
}
