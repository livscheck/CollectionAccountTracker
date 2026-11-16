using CollectionAccountTracker.Data;          // AppDbContext, MonthRange
using CollectionAccountTracker.Models.DTO;    // RemittanceLineDTO, RemittanceReportDTO
using Microsoft.AspNetCore.Mvc;               // ControllerBase, [ApiController], [HttpGet]...
using Microsoft.EntityFrameworkCore;          // ToListAsync

namespace CollectionAccountTracker.Controllers;

[ApiController]
[Route("api/remittance")]
public class RemittanceController(AppDbContext db) : ControllerBase
{

    /*----- API ENDPOINTS -----*/

    // Monthly remittance: what was collected per creditor, the firm's fee, and the net owed
    // GET /api/remittance?year=2026&month=11
    // year and month are optional so the page can open on the current month without asking
    [HttpGet]
    public async Task<ActionResult<RemittanceReportDTO>> Get(int? year, int? month)
    {
        var y = year ?? DateTime.Today.Year;
        var m = month ?? DateTime.Today.Month;

        // 400
        if (m is < 1 or > 12 || y is < 2000 or > 2100)
            return BadRequest("Use a year from 2000 to 2100 and a month from 1 to 12.");

        // Exclusive end date: start <= DatePaid < end catches every time on the last day of the month
        var (start, end) = MonthRange.For(y, m);

        // Runs in SQL so the database does the summing and only one row per creditor comes back,
        // not every payment for the month. Starts from Accounts because a payment only knows its
        // creditor through its account.
        var lines = await db.Accounts
            .SelectMany(a => a.Payments
                .Where(p => p.DatePaid >= start && p.DatePaid < end)
                .Select(p => new
                {
                    a.CreditorId,
                    CreditorName = a.Creditor.Name,
                    p.Amount,
                    p.FirmFee
                }))
            .GroupBy(x => new { x.CreditorId, x.CreditorName })
            .OrderBy(g => g.Key.CreditorName)
            .Select(g => new RemittanceLineDTO(
                g.Key.CreditorId,
                g.Key.CreditorName,
                g.Count(),
                g.Sum(x => x.Amount),
                // Sums the FirmFee stored on each payment, so a later rate change never alters past months
                g.Sum(x => x.FirmFee),
                g.Sum(x => x.Amount) - g.Sum(x => x.FirmFee)))
            .ToListAsync();

        // Runs in memory: there are only a handful of creditor lines, so totaling them here
        // saves a second database query
        // 200 + JSON (an empty Lines list if nothing was collected that month)
        return new RemittanceReportDTO(
            y,
            m,
            lines,
            lines.Sum(l => l.PaymentCount),
            lines.Sum(l => l.Collected),
            lines.Sum(l => l.FirmFee),
            lines.Sum(l => l.NetToCreditor));
    }
}
