using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfsWebApp.Data;
using AfsWebApp.Models;
using AfsWebApp.Services;
using CsvHelper;
using System.Globalization;
using System.Text.Json;

namespace AfsWebApp.Controllers;

// ── Home ──────────────────────────────────────────────────────────────────────
public class HomeController : Controller
{
    private readonly AfsDbContext _db;
    public HomeController(AfsDbContext db) { _db = db; }

    public async Task<IActionResult> Index()
    {
        var clients = await _db.Clients.OrderByDescending(c => c.CreatedAt).Take(10).ToListAsync();
        return View(clients);
    }
}

// ── Client ────────────────────────────────────────────────────────────────────
public class ClientController : Controller
{
    private readonly AfsDbContext _db;
    public ClientController(AfsDbContext db) { _db = db; }

    public IActionResult Create() => View(new Client());

    [HttpPost]
    public async Task<IActionResult> Create(Client model, IFormFile? logo)
    {
        if (!ModelState.IsValid) return View(model);
        if (logo != null && logo.Length > 0)
        {
            using var ms = new MemoryStream();
            await logo.CopyToAsync(ms);
            model.LogoBase64 = Convert.ToBase64String(ms.ToArray());
        }
        _db.Clients.Add(model);
        await _db.SaveChangesAsync();
        // Auto-create first financial year
        var fy = new FinancialYear
        {
            ClientId    = model.Id,
            YearLabel   = DateTime.Now.Year.ToString(),
            PeriodStart = new DateTime(DateTime.Now.Year, 3, 1),
            PeriodEnd   = new DateTime(DateTime.Now.Year + 1, 2, 28),
        };
        _db.FinancialYears.Add(fy);
        await _db.SaveChangesAsync();
        return RedirectToAction("Setup", new { id = model.Id });
    }

    public async Task<IActionResult> Setup(int id)
    {
        var client = await _db.Clients
            .Include(c => c.FinancialYears)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (client == null) return NotFound();
        return View(client);
    }

    [HttpPost]
    public async Task<IActionResult> Setup(Client model, IFormFile? logo)
    {
        var existing = await _db.Clients.FindAsync(model.Id);
        if (existing == null) return NotFound();
        existing.EntityName    = model.EntityName;
        existing.TradingAs     = model.TradingAs;
        existing.RegistrationNumber = model.RegistrationNumber;
        existing.TaxReferenceNumber = model.TaxReferenceNumber;
        existing.LegalForm     = model.LegalForm;
        existing.Country       = model.Country;
        existing.NatureOfBusiness = model.NatureOfBusiness;
        existing.RegisteredAddress = model.RegisteredAddress;
        existing.HoldingCompany = model.HoldingCompany;
        existing.Bankers        = model.Bankers;
        existing.Auditors       = model.Auditors;
        existing.AuditorsAddress = model.AuditorsAddress;
        existing.CompanySecretary = model.CompanySecretary;
        existing.CompanySecretaryAddress = model.CompanySecretaryAddress;
        existing.LegalAdvisors = model.LegalAdvisors;
        existing.LegalAdvisorsAddress = model.LegalAdvisorsAddress;
        existing.LevelOfAssurance = model.LevelOfAssurance;
        existing.PreparedBy     = model.PreparedBy;
        existing.PreparedByAddress = model.PreparedByAddress;
        existing.Director1      = model.Director1;
        existing.Director2      = model.Director2;
        existing.DateOfSignature = model.DateOfSignature;
        existing.FinancialStatementsApprovalDate = model.FinancialStatementsApprovalDate;
        existing.BusinessCommencementDate = model.BusinessCommencementDate;
        existing.AgmDate        = model.AgmDate;
        existing.PlaceOfSignature = model.PlaceOfSignature;
        existing.AccountingStandard = model.AccountingStandard;
        existing.IsGroup        = model.IsGroup;
        existing.IsGoingConcern = model.IsGoingConcern;
        existing.CurrencyRounding = model.CurrencyRounding;
        existing.EngagementLabel = model.EngagementLabel;
        if (logo != null && logo.Length > 0)
        {
            using var ms = new MemoryStream();
            await logo.CopyToAsync(ms);
            existing.LogoBase64 = Convert.ToBase64String(ms.ToArray());
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Client settings saved.";
        return RedirectToAction("Setup", new { id = model.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Clients.FindAsync(id);
        if (c != null) { _db.Clients.Remove(c); await _db.SaveChangesAsync(); }
        return RedirectToAction("Index", "Home");
    }
}

// ── Trial Balance ─────────────────────────────────────────────────────────────
public class TrialBalanceController : Controller
{
    // Fields and constructor already defined above, removed duplicate

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOpeningRetainedEarnings(int id, decimal openingRetainedEarnings)
    {
        var acct = await _db.TbAccounts.FindAsync(id);
        if (acct == null) return NotFound();
        acct.OpeningBalance = openingRetainedEarnings;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Opening Retained Earnings updated.";
        return RedirectToAction("WorkingTB", new { fyId = acct.FinancialYearId });
    }
    private readonly AfsDbContext _db;
    private readonly AutoMappingService _mapper;

    public TrialBalanceController(AfsDbContext db, AutoMappingService mapper)
    {
        _db     = db;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index(int clientId)
    {
        var client = await _db.Clients.Include(c => c.FinancialYears).FirstOrDefaultAsync(c => c.Id == clientId);
        if (client == null) return NotFound();
        var activeFy = client.FinancialYears.FirstOrDefault(f => f.IsActive) ?? client.FinancialYears.FirstOrDefault();
        if (activeFy == null) return RedirectToAction("Setup", "Client", new { id = clientId });
        return RedirectToAction("WorkingTB", new { fyId = activeFy.Id });
    }

    public async Task<IActionResult> WorkingTB(int fyId, string? filter, string? category)
    {
        var fy = await _db.FinancialYears.Include(f => f.Client).FirstOrDefaultAsync(f => f.Id == fyId);
        if (fy == null) return NotFound();
        var query = _db.TbAccounts.Where(a => a.FinancialYearId == fyId);
        if (!string.IsNullOrEmpty(filter))
            query = query.Where(a => a.AccountNumber.Contains(filter) || (a.Description != null && a.Description.Contains(filter)));
        if (!string.IsNullOrEmpty(category))
            query = query.Where(a => a.Category == category);
        var accounts = await query.OrderBy(a => a.AccountNumber).ToListAsync();
        var lineItems = await _db.AfsLineItems.OrderBy(l => l.SortOrder).ToListAsync();
        var vm = new WorkingTrialBalanceVm { FinancialYear = fy, Accounts = accounts, LineItems = lineItems };
        ViewBag.Filter   = filter;
        ViewBag.Category = category;
        ViewBag.Categories = new[] { "Assets", "Liabilities", "Equity", "Income", "Expenses" };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Import(int fyId, IFormFile file)
    {
        var fy = await _db.FinancialYears.FindAsync(fyId);
        if (fy == null || file == null) return BadRequest();

        var existing = _db.TbAccounts.Where(a => a.FinancialYearId == fyId);
        _db.TbAccounts.RemoveRange(existing);

        using var stream = file.OpenReadStream();
        List<TbAccount> accounts;

        if (file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            accounts = ParseCsv(stream, fyId);
        else
            accounts = ParseExcel(stream, fyId);

        _db.TbAccounts.AddRange(accounts);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Imported {accounts.Count} accounts.";
        return RedirectToAction("WorkingTB", new { fyId });
    }

    [HttpPost]
    public async Task<IActionResult> SaveRow(int id, string category, string afsLineItemKey,
        string statementType, string? disclosureDescription)
    {
        var acct = await _db.TbAccounts.FindAsync(id);
        if (acct == null) return NotFound();
        var li = await _db.AfsLineItems.FirstOrDefaultAsync(x => x.Key == afsLineItemKey);
        acct.Category             = category;
        acct.AfsLineItemKey       = afsLineItemKey;
        acct.AfsLineItemLabel     = li?.Label ?? afsLineItemKey;
        acct.SubCategory          = li?.SubCategory;
        acct.StatementType        = statementType;
        acct.DisclosureDescription = disclosureDescription;
        acct.IsMapped             = true;
        acct.MappedBy             = "Manual";
        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> AutoMapAll(int fyId)
    {
        var accounts  = await _db.TbAccounts.Where(a => a.FinancialYearId == fyId).ToListAsync();
        var lineItems = await _db.AfsLineItems.ToListAsync();
        var results   = _mapper.MapAll(accounts, lineItems);

        foreach (var r in results)
        {
            var acct = accounts.First(a => a.AccountNumber == r.AccountNumber);
            acct.Category         = r.Category;
            acct.AfsLineItemKey   = r.AfsLineItemKey;
            acct.AfsLineItemLabel = r.AfsLineItemLabel;
            var li = lineItems.FirstOrDefault(x => x.Key == r.AfsLineItemKey);
            acct.SubCategory      = li?.SubCategory;
            acct.StatementType    = r.StatementType;
            acct.IsMapped         = r.Confidence > 0.5m;
            acct.MappedBy         = r.Confidence > 0.5m ? "Auto" : "Needs Review";
        }
        await _db.SaveChangesAsync();
        var mapped   = results.Count(r => r.Confidence > 0.5m);
        var warnings = results.Count(r => r.Warning != null);
        return Ok(new { success = true, mapped, warnings, total = results.Count });
    }

    [HttpGet]
    public async Task<IActionResult> SuggestMapping(int accountId)
    {
        var acct      = await _db.TbAccounts.FindAsync(accountId);
        if (acct == null) return NotFound();
        var lineItems = await _db.AfsLineItems.ToListAsync();
        var result    = _mapper.MapAccount(acct, lineItems);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> LineItemsJson()
    {
        var li = await _db.AfsLineItems.OrderBy(l => l.SortOrder).ToListAsync();
        return Ok(li.Select(x => new {
            x.Key, x.Label, x.Category, x.SubCategory, x.Statement, x.Section
        }));
    }

    // ── CSV/Excel parsers ─────────────────────────────────────────────────────
    private List<TbAccount> ParseCsv(Stream stream, int fyId)
    {
        using var reader = new StreamReader(stream);
        using var csv    = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read(); csv.ReadHeader();
        var list = new List<TbAccount>();
        while (csv.Read())
        {
            var acct = new TbAccount { FinancialYearId = fyId };
            // Try multiple column name variants
            acct.AccountNumber  = TryGet(csv, "Account","Account_number","AccountNo","Acc_No","ACCOUNT") ?? "";
            acct.Description    = TryGet(csv, "Description","Name","Account_Name","DESCRIPTION","Discription");
            acct.OpeningBalance = TryGetDecimal(csv, "Opening","Opening_Balance","Opening_Bal","Openings","OB");
            acct.ClosingBalance = TryGetDecimal(csv, "Closing","Closing_Balance","Closing_Bal","CB","Balance");
            acct.Transactions   = TryGetDecimal(csv, "Transactions","Trans");
            acct.Adjustments    = TryGetDecimal(csv, "Adjustments","Adj");
            acct.PriorYearClosing = TryGetDecimal(csv, "Prior","Prior_Year","PriorYear","2025 Final");
            if (!string.IsNullOrEmpty(acct.AccountNumber))
                list.Add(acct);
        }
        return list;
    }

    private List<TbAccount> ParseExcel(Stream stream, int fyId)
    {
        using var pkg = new OfficeOpenXml.ExcelPackage(stream);
        var ws = pkg.Workbook.Worksheets[0];
        var list = new List<TbAccount>();
        if (ws == null || ws.Dimension == null) return list;
        // Read header row
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int c = 1; c <= ws.Dimension.End.Column; c++)
        {
            var h = ws.Cells[1, c].Text.Trim();
            if (!string.IsNullOrEmpty(h) && !headers.ContainsKey(h))
                headers[h] = c;
        }
        int GetCol(params string[] names) =>
            names.Select(n => headers.TryGetValue(n, out int col) ? col : 0).FirstOrDefault(c => c > 0);

        var accCol  = GetCol("Account","Account_number","AccountNo","Acc_No","ACCOUNT");
        var descCol = GetCol("Description","Name","Account_Name","DESCRIPTION","Discription");
        var obCol   = GetCol("Opening","Opening_Balance","Opening_Bal","Openings","OB");
        var cbCol   = GetCol("Closing","Closing_Balance","Closing_Bal","CB","Balance");
        var trCol   = GetCol("Transactions","Trans");
        var adjCol  = GetCol("Adjustments","Adj");
        var prCol   = GetCol("Prior","Prior_Year","PriorYear","2025 Final");

        for (int r = 2; r <= ws.Dimension.End.Row; r++)
        {
            var accNo = accCol > 0 ? ws.Cells[r, accCol].Text.Trim() : "";
            if (string.IsNullOrEmpty(accNo)) continue;
            list.Add(new TbAccount
            {
                FinancialYearId = fyId,
                AccountNumber   = accNo,
                Description     = descCol > 0 ? ws.Cells[r, descCol].Text : null,
                OpeningBalance  = ParseDec(obCol > 0 ? ws.Cells[r, obCol].Text : ""),
                ClosingBalance  = ParseDec(cbCol > 0 ? ws.Cells[r, cbCol].Text : ""),
                Transactions    = ParseDec(trCol > 0 ? ws.Cells[r, trCol].Text : ""),
                Adjustments     = ParseDec(adjCol > 0 ? ws.Cells[r, adjCol].Text : ""),
                PriorYearClosing= ParseDec(prCol > 0 ? ws.Cells[r, prCol].Text : ""),
            });
        }
        return list;
    }

    private static string? TryGet(CsvReader csv, params string[] names)
    {
        foreach (var n in names)
            try { var v = csv.GetField(n); if (!string.IsNullOrEmpty(v)) return v; } catch { }
        return null;
    }

    private static decimal TryGetDecimal(CsvReader csv, params string[] names)
    {
        var v = TryGet(csv, names);
        return ParseDec(v ?? "");
    }

    private static decimal ParseDec(string s)
    {
        s = (s ?? "").Trim().Replace("R", "").Replace(" ", "").Replace("\u00a0", "").Replace(",", "");
        if (s.StartsWith("(") && s.EndsWith(")"))
        { s = "-" + s[1..^1]; }
        return decimal.TryParse(s, out var v) ? v : 0m;
    }
}

// ── Mapping ───────────────────────────────────────────────────────────────────
public class MappingController : Controller
{
    private readonly AfsDbContext _db;
    public MappingController(AfsDbContext db) { _db = db; }

    public async Task<IActionResult> Index(int fyId)
    {
        var fy       = await _db.FinancialYears.Include(f => f.Client).FirstOrDefaultAsync(f => f.Id == fyId);
        var accounts = await _db.TbAccounts.Where(a => a.FinancialYearId == fyId).OrderBy(a => a.AccountNumber).ToListAsync();
        var lineItems = await _db.AfsLineItems.OrderBy(l => l.SortOrder).ToListAsync();
        ViewBag.FinancialYear = fy;
        ViewBag.LineItems     = lineItems;
        ViewBag.LineItemsJson = JsonSerializer.Serialize(lineItems.Select(l => new
        {
            l.Key, l.Label, l.Category, l.SubCategory, l.Statement, l.Section, l.SortOrder
        }));
        return View(accounts);
    }
}

// ── Financials ────────────────────────────────────────────────────────────────
public class FinancialsController : Controller
{
    private readonly AfsDbContext _db;
    private readonly PolicyService _pol;

    public FinancialsController(AfsDbContext db, PolicyService pol)
    {
        _db  = db;
        _pol = pol;
    }

    public async Task<IActionResult> Index(int fyId)
    {
        var fy     = await _db.FinancialYears.Include(f => f.Client).FirstOrDefaultAsync(f => f.Id == fyId);
        if (fy == null) return NotFound();
        var client = fy.Client!;
        var accounts = await _db.TbAccounts.Where(a => a.FinancialYearId == fyId && a.IsMapped).ToListAsync();
        var std    = client.AccountingStandard;
        var polLib = _pol.GetPoliciesForStandard(std);

        // Ensure policies exist in DB for mapped line items
        var mappedKeys = accounts.Select(a => a.AfsLineItemKey).Distinct().ToList();
        foreach (var key in mappedKeys.Where(k => k != null))
        {
            var exists = await _db.AccountingPolicies.AnyAsync(p => p.ClientId == client.Id && p.AfsLineItemKey == key && p.Standard == std);
            if (!exists && polLib.TryGetValue(key!, out var pol))
            {
                _db.AccountingPolicies.Add(new AccountingPolicy
                {
                    ClientId = client.Id, AfsLineItemKey = key!, Standard = std,
                    Title = pol.Title, PolicyText = pol.Text, SourceUrl = pol.Ref
                });
            }
        }
        await _db.SaveChangesAsync();

        // Build VM
        var grouped = accounts
            .Where(a => a.AfsLineItemKey != null)
            .GroupBy(a => a.AfsLineItemKey!)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Get opening retained earnings from mapped OpeningBalance for RET_EARN
        decimal openingRetainedEarnings = 0m;
        if (grouped.TryGetValue("RET_EARN", out var reList))
            openingRetainedEarnings = reList.Sum(a => a.OpeningBalance);

        decimal GetSum(params string[] keys) =>
            keys.SelectMany(k => grouped.TryGetValue(k, out var list) ? list : [])
                .Sum(a => a.ClosingBalance);

        // Income categories flip sign (credit-normal = negative in TB)
        var incomeSigns = new Dictionary<string, decimal>();
        var incomeKeys = new[] {"REV_GOODS","REV_SERVICES","REV_RENTAL","REV_SLP","REV_TRANSPORT","REV_MGMT_FEE","OTHER_INC","INVEST_REV","FV_GAIN"};
        foreach (var k in incomeKeys)
            incomeSigns[k] = grouped.TryGetValue(k, out var list) ? -list.Sum(a => a.ClosingBalance) : 0m;

        var vm = new FinancialsVm
        {
            Client         = client,
            FinancialYear  = fy,
            GroupedByLineItem = grouped,
            Policies       = await _db.AccountingPolicies.Where(p => p.ClientId == client.Id && p.Standard == std).ToListAsync(),
            TotalRevenue   = incomeKeys.Where(k => k.StartsWith("REV")).Sum(k => incomeSigns.GetValueOrDefault(k)),
            TotalOtherIncome = new[] {"OTHER_INC","INVEST_REV","FV_GAIN"}.Sum(k => incomeSigns.GetValueOrDefault(k)),
            TotalCostOfSales = Math.Abs(GetSum("COGS","DIRECT_SERV","COS_TRANSPORT","COS_TRES","DEPR_COS")),
            TotalOpex        = Math.Abs(GetSum("EMP_COST","AUDIT_FEES","DEPR_OPEX","LEASE_EXP","BANK_CHG","IT_EXP","LEGAL","CONSULT","BOARD_FEES","CLEANING","FUEL","FOOD_ENT","PRINT_STAT","INSURANCE","MARKETING","SOFTWARE_SUB","BAD_DEBT","OTHER_OPEX")),
            TotalFinanceCosts = Math.Abs(GetSum("FIN_COSTS")),
            TaxExpense       = Math.Abs(GetSum("TAX_EXP")),
            TotalNonCurrentAssets = GetSum("PPE","IA","GOODWILL","INVEST_ASSOC","INVEST_SUBS","FIN_ASSETS","DEFERRED_TAX_A","OTHER_NCA"),
            TotalCurrentAssets    = GetSum("INVENTORIES","TRADE_REC","INTERCO_REC","CASH","CURRENT_TAX_A","OTHER_CA"),
            TotalEquity           = -GetSum("SHARE_CAP","SHARE_PREM","RET_EARN","START_LOAN","OTHER_RES","DRAWINGS","NCI"),
            TotalNonCurrentLiabilities = -GetSum("LTD","LOAN_SHARE","LEASE_INCENT_NCL","DEFERRED_TAX_L","PROV_NCL","OTHER_NCL"),
            TotalCurrentLiabilities    = -GetSum("TRADE_PAY","INTERCO_PAY","BANK_OD","CURRENT_TAX_L","STD","LOAN_SHARE_CL","LEASE_INCENT_CL","DEFERRED_TAX_CL","VAT_PAYABLE","OTHER_CL"),
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> SavePolicy(int clientId, string std, string key, string title, string text)
    {
        var pol = await _db.AccountingPolicies.FirstOrDefaultAsync(p =>
            p.ClientId == clientId && p.AfsLineItemKey == key && p.Standard == std);
        if (pol == null)
        {
            pol = new AccountingPolicy { ClientId = clientId, AfsLineItemKey = key, Standard = std };
            _db.AccountingPolicies.Add(pol);
        }
        pol.Title = title; pol.PolicyText = text; pol.IsEdited = true; pol.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> FetchLivePolicy(string standard, string key, string query)
    {
        var result = await _pol.FetchLivePolicyAsync(standard, key, query);
        return Ok(new { text = result, found = result != null });
    }
}
