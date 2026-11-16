using System.Net;
using System.Net.Http.Json;                   // GetFromJsonAsync, PostAsJsonAsync, ReadFromJsonAsync
using CollectionAccountTracker.Models.DTO;    // the DTOs the API sends and receives
using Microsoft.AspNetCore.Mvc;               // ValidationProblemDetails

namespace CollectionAccountTracker.Services;

// What the Blazor pages use to talk to the API. Pages only see DTOs and never touch the database,
// so the UI could move to WebAssembly or another app could reuse the same API without changes.
public class AccountAPIClient(HttpClient http)
{

    /*----- ACCOUNTS -----*/

    // Accounts page list. Both filters are optional, so only the ones given go in the URL.
    public async Task<List<AccountDTO>> GetAccountsAsync(string? search = null, string? statusCode = null)
    {
        var filters = new List<string>();

        // Escaped so a search like "O'Brien & Sons" can't break the URL
        if (!string.IsNullOrWhiteSpace(search))
            filters.Add($"search={Uri.EscapeDataString(search)}");

        if (!string.IsNullOrWhiteSpace(statusCode))
            filters.Add($"status={Uri.EscapeDataString(statusCode)}");

        var url = "api/accounts" + (filters.Count > 0 ? "?" + string.Join("&", filters) : "");

        // Empty list rather than null, so the page can always loop over the result
        return await http.GetFromJsonAsync<List<AccountDTO>>(url) ?? [];
    }

    // Account detail page. Null when the account doesn't exist, so the page can show
    // "Account not found" instead of an error.
    public async Task<AccountDetailDTO?> GetAccountAsync(int id)
    {
        var response = await http.GetAsync($"api/accounts/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        // Any other failure is unexpected, so let it surface instead of hiding it as "not found"
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AccountDetailDTO>();
    }

    // Stat cards and "Collected this month" on the Accounts page
    public async Task<DashboardDTO?> GetDashboardAsync() =>
        await http.GetFromJsonAsync<DashboardDTO>("api/accounts/dashboard");


    /*----- PAYMENTS -----*/

    // Posts a payment. Returns null on success, or a message the page can show as-is.
    // Returning the message instead of throwing keeps the form's error handling to a single if.
    public async Task<string?> AddPaymentAsync(int accountId, CreatePaymentDTO payment)
    {
        var response = await http.PostAsJsonAsync($"api/accounts/{accountId}/payments", payment);

        if (response.IsSuccessStatusCode)
            return null;

        if (response.StatusCode == HttpStatusCode.NotFound)
            return "This account no longer exists.";

        return await ReadErrorAsync(response);
    }


    /*----- REMITTANCE -----*/

    // Remittance page for one month
    public async Task<RemittanceReportDTO?> GetRemittanceAsync(int year, int month) =>
        await http.GetFromJsonAsync<RemittanceReportDTO>($"api/remittance?year={year}&month={month}");


    /*----- HELPER FUNCTIONS -----*/

    // The API sends errors two ways, and the page shouldn't have to care which:
    //   - business rules (e.g. overpayment): plain text from BadRequest("...")
    //   - DTO validation from [ApiController]: JSON with an "errors" list per field
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            var messages = problem?.Errors.SelectMany(e => e.Value).ToList() ?? [];

            if (messages.Count > 0)
                return string.Join(" ", messages);

            return problem?.Title ?? "The payment couldn't be posted.";
        }

        var text = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(text) ? "The payment couldn't be posted." : text;
    }
}
