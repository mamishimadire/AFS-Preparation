using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AfsWebApp.Models;

// ── Client / Entity ──────────────────────────────────────────────────────────
public class Client
{
    public int Id { get; set; }
    [Required] public string EntityName { get; set; } = "";
    public string? TradingAs { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxReferenceNumber { get; set; } // Added
    public string? LegalForm { get; set; }               // Private Company, SOE, etc.
    public string? Country { get; set; } = "South Africa";
    public string? NatureOfBusiness { get; set; }
    public string? RegisteredAddress { get; set; }
    public string? HoldingCompany { get; set; }
    public string? Bankers { get; set; }
    public string? Auditors { get; set; }
    public string? AuditorsAddress { get; set; } // Added
    public string? CompanySecretary { get; set; }
    public string? CompanySecretaryAddress { get; set; } // Added
    public string? LegalAdvisors { get; set; } // Added
    public string? LegalAdvisorsAddress { get; set; } // Added
    public string? LevelOfAssurance { get; set; } // Added
    public string? PreparedBy { get; set; }
    public string? PreparedByAddress { get; set; } // Added
    public string? Director1 { get; set; }
    public string? Director2 { get; set; }
    public DateTime? DateOfSignature { get; set; }
    public DateTime? FinancialStatementsApprovalDate { get; set; }
    public DateTime? BusinessCommencementDate { get; set; }
    public DateTime? AgmDate { get; set; }
    public string? PlaceOfSignature { get; set; }
    public string AccountingStandard { get; set; } = "IFRS for SMEs";
    public bool IsGroup { get; set; }
    public bool IsGoingConcern { get; set; } = true;
    public string CurrencyRounding { get; set; } = "Decimals";  // Decimals / Thousands / Millions
    public string? LogoBase64 { get; set; }
    public string? EngagementLabel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<FinancialYear> FinancialYears { get; set; } = new List<FinancialYear>();
}

// ── Financial Year ────────────────────────────────────────────────────────────
public class FinancialYear
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    [Required] public string YearLabel { get; set; } = "";   // e.g. "2025"
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<TbAccount> TbAccounts { get; set; } = new List<TbAccount>();
}

// ── TB Account ───────────────────────────────────────────────────────────────
public class TbAccount
{
    public int Id { get; set; }
    public int FinancialYearId { get; set; }
    public FinancialYear? FinancialYear { get; set; }
    [Required] public string AccountNumber { get; set; } = "";
    public string? Description { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Transactions { get; set; }
    public decimal Adjustments { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal PriorYearClosing { get; set; }

    // Mapping
    public string? LinkCode { get; set; }           // e.g. "i.000.001"
    public string? Category { get; set; }           // Assets / Liabilities / Equity / Income / Expenses
    public string? SubCategory { get; set; }        // Revenue / OPEX / Current Assets / etc.
    public string? AfsLineItemKey { get; set; }     // e.g. "REV_GOODS"
    public string? AfsLineItemLabel { get; set; }   // e.g. "Revenue from Sale of Goods"
    public string? StatementType { get; set; }      // Income Statement / Balance Sheet
    public string? DisclosureDescription { get; set; }
    public bool IsMapped { get; set; }
    public string? MappedBy { get; set; }           // "Auto" / "Manual"
}

// ── AFS Line Item Master ──────────────────────────────────────────────────────
public class AfsLineItem
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Category { get; set; } = "";
    public string SubCategory { get; set; } = "";
    public string Statement { get; set; } = "";      // SOFP / SPL
    public string Section { get; set; } = "";        // Non-Current Assets / Revenue / etc.
    public int SortOrder { get; set; }
    public string? LinkCodePrefix { get; set; }
}

// ── Accounting Policy ─────────────────────────────────────────────────────────
public class AccountingPolicy
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public string Standard { get; set; } = "IFRS for SMEs";
    public string AfsLineItemKey { get; set; } = "";
    public string Title { get; set; } = "";
    [Column(TypeName = "TEXT")] public string PolicyText { get; set; } = "";
    public bool IsEdited { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? SourceUrl { get; set; }
}

// ── View Models ───────────────────────────────────────────────────────────────
public class WorkingTrialBalanceVm
{
    public FinancialYear? FinancialYear { get; set; }
    public List<TbAccount> Accounts { get; set; } = new();
    public List<AfsLineItem> LineItems { get; set; } = new();
    public decimal TotalDebits => Accounts.Where(a => a.ClosingBalance > 0).Sum(a => a.ClosingBalance);
    public decimal TotalCredits => Accounts.Where(a => a.ClosingBalance < 0).Sum(a => Math.Abs(a.ClosingBalance));
    public bool IsBalanced => Math.Abs(Accounts.Sum(a => a.ClosingBalance)) < 0.01m;
    public int MappedCount => Accounts.Count(a => a.IsMapped);
    public int UnmappedCount => Accounts.Count(a => !a.IsMapped);
}

public class FinancialsVm
{
    public Client? Client { get; set; }
    public FinancialYear? FinancialYear { get; set; }
    public Dictionary<string, List<TbAccount>> GroupedByLineItem { get; set; } = new();
    public List<AccountingPolicy> Policies { get; set; } = new();

    // Computed totals
    public decimal TotalRevenue { get; set; }
    public decimal TotalCostOfSales { get; set; }
    public decimal GrossProfit => TotalRevenue - TotalCostOfSales;
    public decimal TotalOpex { get; set; }
    public decimal OperatingProfit => GrossProfit - TotalOpex;
    public decimal TotalOtherIncome { get; set; }
    public decimal TotalFinanceCosts { get; set; }
    public decimal ProfitBeforeTax => OperatingProfit + TotalOtherIncome - TotalFinanceCosts;
    public decimal TaxExpense { get; set; }
    public decimal ProfitAfterTax => ProfitBeforeTax - TaxExpense;
    public decimal TotalNonCurrentAssets { get; set; }
    public decimal TotalCurrentAssets { get; set; }
    public decimal TotalAssets => TotalNonCurrentAssets + TotalCurrentAssets;
    public decimal TotalEquity { get; set; }
    public decimal TotalNonCurrentLiabilities { get; set; }
    public decimal TotalCurrentLiabilities { get; set; }
    public decimal TotalLiabilities => TotalNonCurrentLiabilities + TotalCurrentLiabilities;
    public decimal TotalEquityAndLiabilities => TotalEquity + TotalLiabilities;
    public bool IsBalanced => Math.Abs(TotalAssets - TotalEquityAndLiabilities) < 1m;
}

public class AutoMapResult
{
    public string AccountNumber { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public string AfsLineItemKey { get; set; } = "";
    public string AfsLineItemLabel { get; set; } = "";
    public string StatementType { get; set; } = "";
    public decimal Confidence { get; set; }
    public string? Warning { get; set; }
}

public class PolicySearchResult
{
    public string AfsLineItemKey { get; set; } = "";
    public string Title { get; set; } = "";
    public string PolicyText { get; set; } = "";
    public string? SourceUrl { get; set; }
    public string Standard { get; set; } = "";
}
