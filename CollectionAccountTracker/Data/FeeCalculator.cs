namespace CollectionAccountTracker.Data;

// The one place for the fee math, used by the seed data, the API and the live preview on the
// account page, so they can never disagree.
public static class FeeCalculator
{
    // contingencyFee is a percentage, e.g. 33.00 = 33%.
    // Half-cents round up ($0.125 -> $0.13).
    public static decimal FirmFee(decimal amount, decimal contingencyFee) =>
        Math.Round(amount * contingencyFee / 100m, 2, MidpointRounding.AwayFromZero);
}
