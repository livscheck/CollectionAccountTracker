-- Wipes all data (including payments posted while rehearsing) so the app reseeds fresh data.
-- After running this, restart the app: SeedData.Initialize refills the tables when Accounts is empty.
--
-- Only clears the data. If you changed a model and need the tables rebuilt, use
-- `docker compose down -v` then `docker compose up -d` instead.

BEGIN TRANSACTION;

-- Children first, so no foreign key is broken
DELETE FROM Notes;
DELETE FROM Payments;
DELETE FROM Accounts;
DELETE FROM Debtors;
DELETE FROM Creditors;

-- Restart IDs at 1 so account links like /accounts/1 point at the same accounts as before
DBCC CHECKIDENT ('Notes',     RESEED, 0);
DBCC CHECKIDENT ('Payments',  RESEED, 0);
DBCC CHECKIDENT ('Accounts',  RESEED, 0);
DBCC CHECKIDENT ('Debtors',   RESEED, 0);
DBCC CHECKIDENT ('Creditors', RESEED, 0);

COMMIT;
