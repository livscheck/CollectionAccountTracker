using System.ComponentModel.DataAnnotations;

namespace CollectionAccountTracker.Models.DTO;

// The body of POST /api/accounts/{id}/payments.
// The same attributes validate the Blazor form and the API request.
// Nullable so [Required] can tell "not filled in" apart from 0.
public class CreatePaymentDTO
{
    [Required(ErrorMessage = "Enter an amount.")]
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be between $0.01 and $1,000,000.")]
    public decimal? Amount { get; set; }

    [Required(ErrorMessage = "Choose a payment method.")]
    public PaymentMethod? Method { get; set; }
}
