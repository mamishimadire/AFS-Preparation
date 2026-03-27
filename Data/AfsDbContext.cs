using Microsoft.EntityFrameworkCore;
using AfsWebApp.Models;

namespace AfsWebApp.Data;

public class AfsDbContext : DbContext
{
    public AfsDbContext(DbContextOptions<AfsDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();
    public DbSet<TbAccount> TbAccounts => Set<TbAccount>();
    public DbSet<AfsLineItem> AfsLineItems => Set<AfsLineItem>();
    public DbSet<AccountingPolicy> AccountingPolicies => Set<AccountingPolicy>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<TbAccount>().HasIndex(a => new { a.FinancialYearId, a.AccountNumber });
        mb.Entity<AccountingPolicy>().HasIndex(p => new { p.ClientId, p.AfsLineItemKey, p.Standard });

        // Seed AFS line items master data
        mb.Entity<AfsLineItem>().HasData(SeedData.LineItems);
    }
}
