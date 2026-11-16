namespace CollectionAccountTracker.Models.DTO;

// Everything the account detail page shows. Payments are newest first.
public record AccountDetailDTO(
    int Id,
    string DebtorName,
    string CreditorName,
    decimal Balance,
    decimal TotalPaid,
    decimal ContingencyFee,
    string StatusCode,
    string StatusName,
    DateTime DatePlaced,
    List<PaymentDTO> Payments);
