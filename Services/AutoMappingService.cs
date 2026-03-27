using AfsWebApp.Models;

namespace AfsWebApp.Services;

/// <summary>
/// Maps TB account descriptions to AFS line items using keyword matching.
/// Mirrors the keyword logic used in TUTEH/CAATS but extended to handle
/// the full TUTEH chart of accounts.
/// </summary>
public class AutoMappingService
{
    // ── Keyword rules: ordered from most-specific to least-specific ──────────
    // (keyword_substring, AfsLineItemKey, Category, StatementType, confidence)
    private static readonly (string Kw, string Key, string Cat, string Stmt, decimal Conf)[] _rules =
    [
        // ── Revenue ──────────────────────────────────────────────────────────
        ("rev : student accommodation",   "REV_RENTAL",   "Income",      "Income Statement", 0.98m),
        ("rev : leased accommodation",    "REV_RENTAL",   "Income",      "Income Statement", 0.98m),
        ("rev : accommodation",           "REV_RENTAL",   "Income",      "Income Statement", 0.97m),
        ("rev : transportation",          "REV_TRANSPORT","Income",      "Income Statement", 0.97m),
        ("rev : management fee",          "REV_MGMT_FEE", "Income",      "Income Statement", 0.97m),
        ("rev :",                         "REV_SERVICES", "Income",      "Income Statement", 0.90m),
        ("oth inc : interest",            "INVEST_REV",   "Income",      "Income Statement", 0.98m),
        ("interest received",             "INVEST_REV",   "Income",      "Income Statement", 0.97m),
        ("interest income",               "INVEST_REV",   "Income",      "Income Statement", 0.97m),
        ("oth inc:",                      "OTHER_INC",    "Income",      "Income Statement", 0.90m),
        ("sundry income",                 "OTHER_INC",    "Income",      "Income Statement", 0.95m),
        ("other income",                  "OTHER_INC",    "Income",      "Income Statement", 0.92m),
        ("revenue",                       "REV_SERVICES", "Income",      "Income Statement", 0.85m),
        ("sales",                         "REV_GOODS",    "Income",      "Income Statement", 0.90m),
        ("turnover",                      "REV_GOODS",    "Income",      "Income Statement", 0.88m),

        // ── Cost of Sales ─────────────────────────────────────────────────────
        ("cos : tres",                    "COS_TRES",     "Expenses",    "Income Statement", 0.98m),
        ("cos : leased accommodation transport","COS_TRANSPORT","Expenses","Income Statement", 0.98m),
        ("cos : transport",               "COS_TRANSPORT","Expenses",    "Income Statement", 0.96m),
        ("cos : service fee",             "DIRECT_SERV",  "Expenses",    "Income Statement", 0.97m),
        ("cos :",                         "COGS",         "Expenses",    "Income Statement", 0.88m),
        ("cost of sales",                 "COGS",         "Expenses",    "Income Statement", 0.90m),
        ("cost of goods",                 "COGS",         "Expenses",    "Income Statement", 0.90m),
        ("purchases",                     "COGS",         "Expenses",    "Income Statement", 0.87m),

        // ── Depreciation (COS vs OPEX) ────────────────────────────────────────
        ("depreciation : computer software",  "DEPR_OPEX", "Expenses",  "Income Statement", 0.98m),
        ("depreciation : furniture",           "DEPR_OPEX", "Expenses",  "Income Statement", 0.98m),
        ("depreciation : computer equipment",  "DEPR_OPEX", "Expenses",  "Income Statement", 0.98m),
        ("depreciation :",                     "DEPR_OPEX", "Expenses",  "Income Statement", 0.95m),
        ("accumulated depreciation",           "ACC_DEPR",  "Assets",    "Balance Sheet",    0.98m),
        ("accum depr",                         "ACC_DEPR",  "Assets",    "Balance Sheet",    0.97m),
        ("accum deprec",                       "ACC_DEPR",  "Assets",    "Balance Sheet",    0.97m),

        // ── OPEX — Salaries ───────────────────────────────────────────────────
        ("salaries : company uif",          "EMP_COST",   "Expenses",    "Income Statement", 0.98m),
        ("salaries : company sdl",          "EMP_COST",   "Expenses",    "Income Statement", 0.98m),
        ("salaries : company contribution", "EMP_COST",   "Expenses",    "Income Statement", 0.98m),
        ("salaries : travel allowance",     "EMP_COST",   "Expenses",    "Income Statement", 0.97m),
        ("salaries : buss travel",          "EMP_COST",   "Expenses",    "Income Statement", 0.97m),
        ("salaries : cell phone",           "EMP_COST",   "Expenses",    "Income Statement", 0.97m),
        ("salaries-leave provision",        "EMP_COST",   "Expenses",    "Income Statement", 0.97m),
        ("salaries and related",            "EMP_COST",   "Expenses",    "Income Statement", 0.97m),
        ("salaries and wages",              "EMP_COST",   "Expenses",    "Income Statement", 0.97m),
        ("salary & wages",                  "EMP_COST",   "Expenses",    "Income Statement", 0.97m),
        ("salary and wages",                "EMP_COST",   "Expenses",    "Income Statement", 0.97m),
        ("wages control",                   "EMP_COST",   "Expenses",    "Income Statement", 0.95m),
        ("salary",                          "EMP_COST",   "Expenses",    "Income Statement", 0.90m),
        ("salaries",                        "EMP_COST",   "Expenses",    "Income Statement", 0.90m),
        ("wages",                           "EMP_COST",   "Expenses",    "Income Statement", 0.88m),
        ("payroll",                         "EMP_COST",   "Expenses",    "Income Statement", 0.88m),
        ("uif",                             "EMP_COST",   "Expenses",    "Income Statement", 0.92m),
        ("sdl contribution",                "EMP_COST",   "Expenses",    "Income Statement", 0.92m),
        ("leave provision",                 "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.92m),
        ("provision for accrued leave",     "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.97m),

        // ── OPEX — Goods & Services ───────────────────────────────────────────
        ("goods & serv : bank charges",     "BANK_CHG",   "Expenses",    "Income Statement", 0.99m),
        ("goods & serv : company secretarial","BOARD_FEES","Expenses",   "Income Statement", 0.98m),
        ("goods & serv : computer consumables","IT_EXP",  "Expenses",    "Income Statement", 0.97m),
        ("goods & serv : general consulting","CONSULT",   "Expenses",    "Income Statement", 0.98m),
        ("goods & serv : it support",       "IT_EXP",     "Expenses",    "Income Statement", 0.98m),
        ("goods & serv : legal",            "LEGAL",      "Expenses",    "Income Statement", 0.99m),
        ("goods & serv : marketing",        "MARKETING",  "Expenses",    "Income Statement", 0.98m),
        ("goods & serv : office rent",      "LEASE_EXP",  "Expenses",    "Income Statement", 0.99m),
        ("goods & serv : operating lease",  "LEASE_EXP",  "Expenses",    "Income Statement", 0.99m),
        ("goods & serv : printing",         "PRINT_STAT", "Expenses",    "Income Statement", 0.98m),
        ("goods & serv : short term insurance","INSURANCE","Expenses",   "Income Statement", 0.98m),
        ("goods & serv : software",         "SOFTWARE_SUB","Expenses",   "Income Statement", 0.98m),
        ("goods & serv : staff welfare",    "FOOD_ENT",   "Expenses",    "Income Statement", 0.97m),
        ("goods & serv : telephone",        "IT_EXP",     "Expenses",    "Income Statement", 0.98m),
        ("goods & serv : travel",           "FUEL",       "Expenses",    "Income Statement", 0.97m),
        ("goods & serv : interest paid",    "FIN_COSTS",  "Expenses",    "Income Statement", 0.99m),
        ("goods & serv : tice",             "MARKETING",  "Expenses",    "Income Statement", 0.95m),
        ("goods & serv:",                   "OTHER_OPEX", "Expenses",    "Income Statement", 0.80m),

        // ── OPEX — Generic ────────────────────────────────────────────────────
        ("bank charges",                    "BANK_CHG",   "Expenses",    "Income Statement", 0.96m),
        ("bank fee",                        "BANK_CHG",   "Expenses",    "Income Statement", 0.95m),
        ("accounting fee",                  "AUDIT_FEES", "Expenses",    "Income Statement", 0.96m),
        ("audit fee",                       "AUDIT_FEES", "Expenses",    "Income Statement", 0.97m),
        ("audit fee accrual",               "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.97m),
        ("telephone",                       "IT_EXP",     "Expenses",    "Income Statement", 0.92m),
        ("wifi",                            "IT_EXP",     "Expenses",    "Income Statement", 0.90m),
        ("internet",                        "IT_EXP",     "Expenses",    "Income Statement", 0.90m),
        ("office rent",                     "LEASE_EXP",  "Expenses",    "Income Statement", 0.95m),
        ("operating lease",                 "LEASE_EXP",  "Expenses",    "Income Statement", 0.95m),
        ("rental",                          "LEASE_EXP",  "Expenses",    "Income Statement", 0.86m),
        ("interest paid",                   "FIN_COSTS",  "Expenses",    "Income Statement", 0.97m),
        ("finance cost",                    "FIN_COSTS",  "Expenses",    "Income Statement", 0.95m),
        ("printing",                        "PRINT_STAT", "Expenses",    "Income Statement", 0.93m),
        ("stationery",                      "PRINT_STAT", "Expenses",    "Income Statement", 0.93m),
        ("refreshment",                     "FOOD_ENT",   "Expenses",    "Income Statement", 0.94m),
        ("staff welfare",                   "FOOD_ENT",   "Expenses",    "Income Statement", 0.94m),
        ("electricity",                     "OTHER_OPEX", "Expenses",    "Income Statement", 0.90m),
        ("fuel cost",                       "FUEL",       "Expenses",    "Income Statement", 0.95m),
        ("fuel",                            "FUEL",       "Expenses",    "Income Statement", 0.88m),
        ("travel",                          "FUEL",       "Expenses",    "Income Statement", 0.86m),
        ("cleaning",                        "CLEANING",   "Expenses",    "Income Statement", 0.90m),
        ("legal cost",                      "LEGAL",      "Expenses",    "Income Statement", 0.95m),
        ("consulting",                      "CONSULT",    "Expenses",    "Income Statement", 0.90m),
        ("insurance",                       "INSURANCE",  "Expenses",    "Income Statement", 0.88m),
        ("marketing",                       "MARKETING",  "Expenses",    "Income Statement", 0.88m),
        ("software",                        "SOFTWARE_SUB","Expenses",   "Income Statement", 0.84m),
        ("depreciation",                    "DEPR_OPEX",  "Expenses",    "Income Statement", 0.90m),
        ("tax expense",                     "TAX_EXP",    "Expenses",    "Income Statement", 0.97m),
        ("income tax",                      "TAX_EXP",    "Expenses",    "Income Statement", 0.95m),
        ("normal income tax",               "TAX_EXP",    "Expenses",    "Income Statement", 0.97m),

        // ── Assets — PPE ──────────────────────────────────────────────────────
        ("computer equipment - @ cost",     "PPE",        "Assets",      "Balance Sheet",    0.99m),
        ("computer equipment - accum",      "ACC_DEPR",   "Assets",      "Balance Sheet",    0.99m),
        ("office equipment - @ cost",       "PPE",        "Assets",      "Balance Sheet",    0.99m),
        ("office equipment - accum",        "ACC_DEPR",   "Assets",      "Balance Sheet",    0.99m),
        ("furniture & fittings - @ cost",   "PPE",        "Assets",      "Balance Sheet",    0.99m),
        ("furniture & fittings - accum",    "ACC_DEPR",   "Assets",      "Balance Sheet",    0.99m),
        ("kitchen equipment - @ cost",      "PPE",        "Assets",      "Balance Sheet",    0.99m),
        ("kitchen equipment - accum",       "ACC_DEPR",   "Assets",      "Balance Sheet",    0.99m),
        ("computer software - @ cost",      "PPE",        "Assets",      "Balance Sheet",    0.99m),
        ("computer software - accum",       "ACC_DEPR",   "Assets",      "Balance Sheet",    0.99m),
        ("sundry assets less than",         "PPE",        "Assets",      "Balance Sheet",    0.97m),
        ("sundry assets < r2k accum",       "ACC_DEPR",   "Assets",      "Balance Sheet",    0.97m),
        ("property plant",                  "PPE",        "Assets",      "Balance Sheet",    0.95m),
        ("equipment - @ cost",              "PPE",        "Assets",      "Balance Sheet",    0.95m),
        ("equipment - accum",               "ACC_DEPR",   "Assets",      "Balance Sheet",    0.95m),
        ("@ cost",                          "PPE",        "Assets",      "Balance Sheet",    0.85m),

        // ── Assets — Current ──────────────────────────────────────────────────
        ("customer control account",        "TRADE_REC",  "Assets",      "Balance Sheet",    0.97m),
        ("due from entity",                 "TRADE_REC",  "Assets",      "Balance Sheet",    0.95m),
        ("rental deposits paid",            "TRADE_REC",  "Assets",      "Balance Sheet",    0.95m),
        ("e-waste",                         "TRADE_REC",  "Assets",      "Balance Sheet",    0.88m),
        ("project cost",                    "TRADE_REC",  "Assets",      "Balance Sheet",    0.82m),
        ("provision for doubtful debts",    "TRADE_REC",  "Assets",      "Balance Sheet",    0.95m),
        ("vat input",                       "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.95m),
        ("nedbank current",                 "CASH",       "Assets",      "Balance Sheet",    0.99m),
        ("nedbank call",                    "CASH",       "Assets",      "Balance Sheet",    0.99m),
        ("nedbank petty cash",              "CASH",       "Assets",      "Balance Sheet",    0.99m),
        ("nedbank garage",                  "CASH",       "Assets",      "Balance Sheet",    0.98m),
        ("nedbank procurement",             "CASH",       "Assets",      "Balance Sheet",    0.98m),
        ("petty cash",                      "CASH",       "Assets",      "Balance Sheet",    0.97m),
        ("cash and cash equivalents",       "CASH",       "Assets",      "Balance Sheet",    0.99m),
        ("bank account",                    "CASH",       "Assets",      "Balance Sheet",    0.94m),
        ("call account",                    "CASH",       "Assets",      "Balance Sheet",    0.93m),
        ("receivable",                      "TRADE_REC",  "Assets",      "Balance Sheet",    0.88m),
        ("debtor",                          "TRADE_REC",  "Assets",      "Balance Sheet",    0.88m),
        ("trade and other receivables",     "TRADE_REC",  "Assets",      "Balance Sheet",    0.98m),
        ("sundry debtor",                   "TRADE_REC",  "Assets",      "Balance Sheet",    0.90m),
        ("inventory",                       "INVENTORIES","Assets",      "Balance Sheet",    0.95m),
        ("stock",                           "INVENTORIES","Assets",      "Balance Sheet",    0.92m),

        // ── Intercompany ──────────────────────────────────────────────────────
        ("tuteh prop intercompany",         "INTERCO_REC","Assets",      "Balance Sheet",    0.98m),
        ("intercompany account",            "INTERCO_PAY","Liabilities", "Balance Sheet",    0.95m),
        ("tut intercompany",                "INTERCO_PAY","Liabilities", "Balance Sheet",    0.97m),
        ("tuteh intercompany",              "INTERCO_PAY","Liabilities", "Balance Sheet",    0.97m),
        ("due to entity",                   "INTERCO_PAY","Liabilities", "Balance Sheet",    0.96m),
        ("inter-company",                   "INTERCO_PAY","Liabilities", "Balance Sheet",    0.90m),

        // ── Liabilities ───────────────────────────────────────────────────────
        ("provision for normal income tax", "CURRENT_TAX_L","Liabilities","Balance Sheet",  0.99m),
        ("provision for workmens",          "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.96m),
        ("vat control",                     "VAT_PAYABLE","Liabilities", "Balance Sheet",    0.97m),
        ("vat output",                      "VAT_PAYABLE","Liabilities", "Balance Sheet",    0.97m),
        ("supplier control",                "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.97m),
        ("salary & wages control",          "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.97m),
        ("audit fee accrual",               "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.97m),
        ("sundry creditor",                 "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.92m),
        ("creditor",                        "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.86m),
        ("payable",                         "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.85m),
        ("trade and other payables",        "TRADE_PAY",  "Liabilities", "Balance Sheet",    0.98m),
        ("loan from shareholder",           "LOAN_SHARE", "Liabilities", "Balance Sheet",    0.96m),
        ("bank overdraft",                  "BANK_OD",    "Liabilities", "Balance Sheet",    0.97m),
        ("overdraft",                       "BANK_OD",    "Liabilities", "Balance Sheet",    0.92m),
        ("deferred tax - balance",          "DEFERRED_TAX_L","Liabilities","Balance Sheet", 0.98m),
        ("deferred tax",                    "DEFERRED_TAX_L","Liabilities","Balance Sheet", 0.90m),

        // ── Equity ────────────────────────────────────────────────────────────
        ("tshwane university capital contribution","SHARE_CAP","Equity", "Balance Sheet",    0.98m),
        ("capital contribution",            "SHARE_CAP",  "Equity",      "Balance Sheet",    0.94m),
        ("start up loan",                   "START_LOAN", "Equity",      "Balance Sheet",    0.97m),
        ("retained income",                 "RET_EARN",   "Equity",      "Balance Sheet",    0.97m),
        ("accumulated loss",                "RET_EARN",   "Equity",      "Balance Sheet",    0.97m),
        ("retained earnings",               "RET_EARN",   "Equity",      "Balance Sheet",    0.97m),
        ("issued capital",                  "SHARE_CAP",  "Equity",      "Balance Sheet",    0.97m),
        ("share capital",                   "SHARE_CAP",  "Equity",      "Balance Sheet",    0.97m),
        ("share premium",                   "SHARE_PREM", "Equity",      "Balance Sheet",    0.97m),
        ("drawings",                        "DRAWINGS",   "Equity",      "Balance Sheet",    0.93m),
        ("capital",                         "SHARE_CAP",  "Equity",      "Balance Sheet",    0.80m),
    ];

    public AutoMapResult MapAccount(TbAccount account, List<AfsLineItem> lineItems)
    {
        var desc = (account.Description ?? "").ToLower().Trim();
        var result = new AutoMapResult
        {
            AccountNumber = account.AccountNumber,
            Description   = account.Description ?? ""
        };

        // Try each rule in order
        foreach (var (kw, key, cat, stmt, conf) in _rules)
        {
            if (desc.Contains(kw))
            {
                var li = lineItems.FirstOrDefault(x => x.Key == key);
                if (li != null)
                {
                    result.AfsLineItemKey   = key;
                    result.AfsLineItemLabel = li.Label;
                    result.Category         = cat;
                    result.StatementType    = stmt;
                    result.Confidence       = conf;

                    // Sign validation
                    var signWarn = ValidateSign(cat, account.ClosingBalance);
                    if (signWarn != null) result.Warning = signWarn;

                    return result;
                }
            }
        }

        // No match found
        result.AfsLineItemKey   = "OTHER_OPEX";
        result.AfsLineItemLabel = "Other — Review Required";
        result.Category         = "Expenses";
        result.StatementType    = "Income Statement";
        result.Confidence       = 0.20m;
        result.Warning          = "No keyword match found — please review and map manually.";
        return result;
    }

    private static string? ValidateSign(string category, decimal closingBalance)
    {
        var creditCats  = new[] { "Liabilities", "Equity", "Income" };
        var debitCats   = new[] { "Assets", "Expenses" };
        if (creditCats.Contains(category) && closingBalance > 0.01m)
            return $"⚠️ {category} is credit-normal — expected negative balance in TB. Got {closingBalance:N2}.";
        if (debitCats.Contains(category) && closingBalance < -0.01m)
            return $"⚠️ {category} is debit-normal — expected positive balance in TB. Got {closingBalance:N2}.";
        return null;
    }

    public List<AutoMapResult> MapAll(List<TbAccount> accounts, List<AfsLineItem> lineItems)
        => accounts.Select(a => MapAccount(a, lineItems)).ToList();
}
