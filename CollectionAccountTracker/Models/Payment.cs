using System.Text.Json.Serialization;

namespace CollectionAccountTracker.Models;

// Sent as "Check", "Card", ... in JSON instead of 0, 1, ...
[JsonConverter(typeof(JsonStringEnumConverter<PaymentMethod>))]
public enum PaymentMethod
{
    Cash,
    Check,
    Card,
    Transfer,
    Other
}
public class Payment
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public required decimal Amount { get; set; }
    public required decimal FirmFee { get; set; }
    public required PaymentMethod Method { get; set; }
    public required DateTime DatePaid { get; set; }
    public string? Notes { get; set; }
}