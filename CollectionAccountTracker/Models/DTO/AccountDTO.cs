namespace CollectionAccountTracker.Models.DTO;

// One row on the Accounts page.
// Status is sent as its Code and Name because AccountStatus can't be rebuilt from JSON
// (private constructor); use AccountStatus.FromCode(StatusCode) if you need the object.
public record AccountDTO(
    int Id,
    string DebtorName,
    string CreditorName,
    decimal Balance,
    string StatusCode,
    string StatusName,
    DateTime DatePlaced,
    int PaymentCount,
    DateTime? LastPaymentDate);
