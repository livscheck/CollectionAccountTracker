-- Monthly remittance straight from the database, to check against the Remittance page.
-- Change @Year and @Month, then run against the CollectionAccountTracker database.

DECLARE @Year  INT = YEAR(GETDATE());
DECLARE @Month INT = MONTH(GETDATE());

-- Exclusive end date: start <= DatePaid < end catches every time on the last day of the month
DECLARE @Start DATETIME2 = DATEFROMPARTS(@Year, @Month, 1);
DECLARE @End   DATETIME2 = DATEADD(MONTH, 1, @Start);

SELECT
    COALESCE(c.Name, 'Total')        AS Creditor,
    COUNT(*)                         AS Payments,
    SUM(p.Amount)                    AS Collected,
    SUM(p.FirmFee)                   AS FirmFee,
    SUM(p.Amount) - SUM(p.FirmFee)   AS NetToCreditor
FROM Payments p
JOIN Accounts  a ON a.Id = p.AccountId
JOIN Creditors c ON c.Id = a.CreditorId
WHERE p.DatePaid >= @Start
  AND p.DatePaid <  @End
GROUP BY ROLLUP (c.Name)                -- ROLLUP adds the Total row at the bottom
ORDER BY GROUPING(c.Name), c.Name;
