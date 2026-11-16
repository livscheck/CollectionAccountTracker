namespace CollectionAccountTracker.Models.DTO;

// The whole Remittance page for one month: a line per creditor plus the totals row.
public record RemittanceReportDTO(
    int Year,
    int Month,
    List<RemittanceLineDTO> Lines,
    int TotalPaymentCount,
    decimal TotalCollected,
    decimal TotalFirmFee,
    decimal TotalNetToCreditor);
