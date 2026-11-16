namespace CollectionAccountTracker.Components.Common;

// One label/value line shown inside a StatCard, e.g. "Accounts: 3".
public record Stat
{
    public required string Label { get; init; }
    public required string Value { get; init; }
}
