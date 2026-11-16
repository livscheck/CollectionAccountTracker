namespace CollectionAccountTracker.Models.DTO;

// One stat card on the Accounts page: how many accounts have this status and what they still owe.
public record StatusSummaryDTO(string StatusCode, string StatusName, int Count, decimal TotalBalance);


/*
    Note:
        Use a record for data that's filled in once and only read, like a receipt. 
        Use a class for data that gets filled in or changed over time, like a form.
*/

