namespace CollectionAccountTracker.Models.DTO;

// One creditor's row on the Remittance page: what was collected for them in the month,
// the firm's fee, and the net owed to them.
public record RemittanceLineDTO(
    int CreditorId,
    string CreditorName,
    int PaymentCount,
    decimal Collected,
    decimal FirmFee,
    decimal NetToCreditor);


/*
Note:
    Use a record for data that's filled in once and only read, like a receipt. 
    Use a class for data that gets filled in or changed over time, like a form.
*/
