namespace CollectionAccountTracker.Models.DTO;

// The summary at the top of the Accounts page.
public record DashboardDTO(List<StatusSummaryDTO> ByStatus, decimal CollectedThisMonth);
