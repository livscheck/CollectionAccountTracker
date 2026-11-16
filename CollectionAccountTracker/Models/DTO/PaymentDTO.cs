namespace CollectionAccountTracker.Models.DTO;

public record PaymentDTO(int Id, decimal Amount, decimal FirmFee, DateTime DatePaid, PaymentMethod Method);
