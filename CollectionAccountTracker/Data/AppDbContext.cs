using CollectionAccountTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace CollectionAccountTracker.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Debtor> Debtors => Set<Debtor>();
    public DbSet<Creditor> Creditors => Set<Creditor>();
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(account =>
        {
            // Money: 18 digits, 2 after the decimal point
            account.Property(a => a.InitialBalance).HasPrecision(18, 2);
            account.Property(a => a.TotalPaid).HasPrecision(18, 2);

            // Computed column: SQL Server keeps it equal to InitialBalance - TotalPaid,
            // so balance is defined once and every query and report agrees
            account.Property(a => a.Balance)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("[InitialBalance] - [TotalPaid]");

            // Percentage, e.g. 33.00 = 33%
            account.Property(a => a.ContingencyFee).HasPrecision(5, 2);

            // AccountStatus is a class, not an enum, so store its Code ("PLACED", "SUIT", ...)
            // and turn it back into the matching static instance when reading.
            account.Property(a => a.Status)
                .HasConversion(status => status.Code, code => AccountStatus.FromCode(code))
                .HasMaxLength(10);

            account.HasOne(a => a.Debtor)
                .WithMany(d => d.Accounts)
                .HasForeignKey(a => a.DebtorId)
                .OnDelete(DeleteBehavior.Restrict);

            account.HasOne(a => a.Creditor)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CreditorId)
                .OnDelete(DeleteBehavior.Restrict);

            // One account → many payments; deleting an account deletes its payments
            account.HasMany(a => a.Payments)
                .WithOne()
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            account.HasMany(a => a.Notes)
                .WithOne()
                .HasForeignKey(n => n.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(payment =>
        {
            payment.Property(p => p.Amount).HasPrecision(18, 2);
            payment.Property(p => p.FirmFee).HasPrecision(18, 2);

            // Store "Check", "Card", ... instead of 0, 1, ... so the table is readable
            payment.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Debtor>().Property(d => d.Name).HasMaxLength(200);
        modelBuilder.Entity<Creditor>().Property(c => c.Name).HasMaxLength(200);
    }
}
