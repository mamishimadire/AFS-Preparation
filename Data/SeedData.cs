using AfsWebApp.Models;

namespace AfsWebApp.Data;

public static class SeedData
{
    public static AfsLineItem[] LineItems => new[]
    {
        // ── BALANCE SHEET — ASSETS ────────────────────────────────────────────
        new AfsLineItem { Id=1,  Key="PPE",             Label="Property, Plant and Equipment",          Category="Assets",      SubCategory="Non-Current Assets",    Statement="SOFP", Section="Non-Current Assets",    SortOrder=10, LinkCodePrefix="na.400" },
        new AfsLineItem { Id=2,  Key="ACC_DEPR",        Label="Accumulated Depreciation",               Category="Assets",      SubCategory="Non-Current Assets",    Statement="SOFP", Section="Non-Current Assets",    SortOrder=11, LinkCodePrefix="na.400" },
        new AfsLineItem { Id=3,  Key="IA",              Label="Intangible Assets",                      Category="Assets",      SubCategory="Non-Current Assets",    Statement="SOFP", Section="Non-Current Assets",    SortOrder=12, LinkCodePrefix="na.200" },
        new AfsLineItem { Id=4,  Key="GOODWILL",        Label="Goodwill",                               Category="Assets",      SubCategory="Non-Current Assets",    Statement="SOFP", Section="Non-Current Assets",    SortOrder=13, LinkCodePrefix="na.100" },
        new AfsLineItem { Id=5,  Key="INVEST_ASSOC",    Label="Investments in Associates",              Category="Assets",      SubCategory="Non-Current Assets",    Statement="SOFP", Section="Non-Current Assets",    SortOrder=14, LinkCodePrefix="na.500" },
        new AfsLineItem { Id=6,  Key="INVEST_SUBS",     Label="Investments in Subsidiaries",            Category="Assets",      SubCategory="Non-Current Assets",    Statement="SOFP", Section="Non-Current Assets",    SortOrder=15, LinkCodePrefix="na.600" },
        new AfsLineItem { Id=7,  Key="FIN_ASSETS",      Label="Financial Assets - Derivatives",         Category="Assets",      SubCategory="Non-Current Assets",    Statement="SOFP", Section="Non-Current Assets",    SortOrder=16, LinkCodePrefix="na.700" },
        new AfsLineItem { Id=8,  Key="DEFERRED_TAX_A",  Label="Deferred Tax Asset",                    Category="Assets",      SubCategory="Non-Current Assets",    Statement="SOFP", Section="Non-Current Assets",    SortOrder=17, LinkCodePrefix="na.910" },
        new AfsLineItem { Id=9,  Key="OTHER_NCA",       Label="Other Non-Current Assets",               Category="Assets",      SubCategory="Non-Current Assets",    Statement="SOFP", Section="Non-Current Assets",    SortOrder=18, LinkCodePrefix="na.084" },
        new AfsLineItem { Id=10, Key="INVENTORIES",     Label="Inventories / Stock",                    Category="Assets",      SubCategory="Current Assets",        Statement="SOFP", Section="Current Assets",        SortOrder=20, LinkCodePrefix="ca.100" },
        new AfsLineItem { Id=11, Key="TRADE_REC",       Label="Trade and Other Receivables",            Category="Assets",      SubCategory="Current Assets",        Statement="SOFP", Section="Current Assets",        SortOrder=21, LinkCodePrefix="ca.200" },
        new AfsLineItem { Id=12, Key="INTERCO_REC",     Label="Intercompany / Inter-Company Receivable",Category="Assets",      SubCategory="Current Assets",        Statement="SOFP", Section="Current Assets",        SortOrder=22, LinkCodePrefix="ca.590" },
        new AfsLineItem { Id=13, Key="CASH",            Label="Cash and Cash Equivalents",              Category="Assets",      SubCategory="Current Assets",        Statement="SOFP", Section="Current Assets",        SortOrder=23, LinkCodePrefix="ca.800" },
        new AfsLineItem { Id=14, Key="CURRENT_TAX_A",   Label="Current Tax Asset",                     Category="Assets",      SubCategory="Current Assets",        Statement="SOFP", Section="Current Assets",        SortOrder=24, LinkCodePrefix="ca.910" },
        new AfsLineItem { Id=15, Key="OTHER_CA",        Label="Other Current Assets",                   Category="Assets",      SubCategory="Current Assets",        Statement="SOFP", Section="Current Assets",        SortOrder=25, LinkCodePrefix="ca.900" },

        // ── BALANCE SHEET — EQUITY ────────────────────────────────────────────
        new AfsLineItem { Id=20, Key="SHARE_CAP",       Label="Share Capital / Issued Capital",         Category="Equity",      SubCategory="Equity",                Statement="SOFP", Section="Equity",                SortOrder=30, LinkCodePrefix="e.100" },
        new AfsLineItem { Id=21, Key="SHARE_PREM",      Label="Share Premium",                          Category="Equity",      SubCategory="Equity",                Statement="SOFP", Section="Equity",                SortOrder=31, LinkCodePrefix="e.110" },
        new AfsLineItem { Id=22, Key="RET_EARN",        Label="Retained Earnings / (Accumulated Loss)", Category="Equity",      SubCategory="Equity",                Statement="SOFP", Section="Equity",                SortOrder=32, LinkCodePrefix="e.200" },
        new AfsLineItem { Id=23, Key="START_LOAN",      Label="Start-Up Loan / Shareholder Loan (Equity)",Category="Equity",   SubCategory="Equity",                Statement="SOFP", Section="Equity",                SortOrder=33, LinkCodePrefix="e.300" },
        new AfsLineItem { Id=24, Key="OTHER_RES",       Label="Other Reserves",                         Category="Equity",      SubCategory="Equity",                Statement="SOFP", Section="Equity",                SortOrder=34, LinkCodePrefix="e.400" },
        new AfsLineItem { Id=25, Key="DRAWINGS",        Label="Drawings / Distributions",               Category="Equity",      SubCategory="Equity",                Statement="SOFP", Section="Equity",                SortOrder=35, LinkCodePrefix="e.500" },
        new AfsLineItem { Id=26, Key="NCI",             Label="Non-Controlling Interest",               Category="Equity",      SubCategory="Equity",                Statement="SOFP", Section="Equity",                SortOrder=36, LinkCodePrefix="e.900" },

        // ── CUSTOM: Other Equity ──
        new AfsLineItem { Id=200, Key="OTHER_EQUITY", Label="Other Equity", Category="Equity", SubCategory="Equity", Statement="SOFP", Section="Equity", SortOrder=37, LinkCodePrefix="e.950" },

        // ── BALANCE SHEET — NON-CURRENT LIABILITIES ───────────────────────────
        new AfsLineItem { Id=30, Key="LTD",             Label="Long-Term Borrowings",                   Category="Liabilities", SubCategory="Non-Current Liabilities",Statement="SOFP", Section="Non-Current Liabilities",SortOrder=40, LinkCodePrefix="ncl.100" },
        new AfsLineItem { Id=31, Key="LOAN_SHARE",      Label="Loans from Shareholders (Non-Current)",  Category="Liabilities", SubCategory="Non-Current Liabilities",Statement="SOFP", Section="Non-Current Liabilities",SortOrder=41, LinkCodePrefix="ncl.200" },
        new AfsLineItem { Id=32, Key="LEASE_INCENT_NCL",Label="Lease Incentive (Non-Current)",          Category="Liabilities", SubCategory="Non-Current Liabilities",Statement="SOFP", Section="Non-Current Liabilities",SortOrder=42, LinkCodePrefix="ncl.400" },
        new AfsLineItem { Id=33, Key="DEFERRED_TAX_L",  Label="Deferred Tax Liability",                 Category="Liabilities", SubCategory="Non-Current Liabilities",Statement="SOFP", Section="Non-Current Liabilities",SortOrder=43, LinkCodePrefix="ncl.910" },
        new AfsLineItem { Id=34, Key="PROV_NCL",        Label="Provisions (Non-Current)",               Category="Liabilities", SubCategory="Non-Current Liabilities",Statement="SOFP", Section="Non-Current Liabilities",SortOrder=44, LinkCodePrefix="ncl.500" },
        new AfsLineItem { Id=35, Key="OTHER_NCL",       Label="Other Non-Current Liabilities",          Category="Liabilities", SubCategory="Non-Current Liabilities",Statement="SOFP", Section="Non-Current Liabilities",SortOrder=45, LinkCodePrefix="ncl.900" },

    // ── CUSTOM: Other Non-Current Assets ──
    new AfsLineItem { Id=201, Key="OTHER_NCA2", Label="Other Non-Current Assets (Custom)", Category="Assets", SubCategory="Non-Current Assets", Statement="SOFP", Section="Non-Current Assets", SortOrder=19, LinkCodePrefix="na.085" },

        // ── BALANCE SHEET — CURRENT LIABILITIES ───────────────────────────────
        new AfsLineItem { Id=40, Key="TRADE_PAY",       Label="Trade and Other Payables",               Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=50, LinkCodePrefix="cl.100" },
        new AfsLineItem { Id=41, Key="INTERCO_PAY",     Label="Intercompany / Inter-Company Payable",   Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=51, LinkCodePrefix="cl.590" },
        new AfsLineItem { Id=42, Key="BANK_OD",         Label="Bank Overdraft",                         Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=52, LinkCodePrefix="cl.200" },
        new AfsLineItem { Id=43, Key="CURRENT_TAX_L",   Label="Current Tax Payable",                    Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=53, LinkCodePrefix="cl.910" },
        new AfsLineItem { Id=44, Key="STD",             Label="Short-Term Borrowings",                  Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=54, LinkCodePrefix="cl.300" },
        new AfsLineItem { Id=45, Key="LOAN_SHARE_CL",   Label="Loans from Shareholders (Current)",      Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=55, LinkCodePrefix="cl.400" },
        new AfsLineItem { Id=46, Key="LEASE_INCENT_CL", Label="Lease Incentive (Current)",              Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=56, LinkCodePrefix="cl.500" },
        new AfsLineItem { Id=47, Key="DEFERRED_TAX_CL", Label="Deferred Tax (Current)",                 Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=57, LinkCodePrefix="cl.911" },
        new AfsLineItem { Id=48, Key="VAT_PAYABLE",     Label="VAT Payable",                            Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=58, LinkCodePrefix="cl.800" },
        new AfsLineItem { Id=49, Key="OTHER_CL",        Label="Other Current Liabilities",              Category="Liabilities", SubCategory="Current Liabilities",   Statement="SOFP", Section="Current Liabilities",    SortOrder=59, LinkCodePrefix="cl.900" },

    // ── CUSTOM: Other Current Assets ──
    new AfsLineItem { Id=202, Key="OTHER_CA2", Label="Other Current Assets (Custom)", Category="Assets", SubCategory="Current Assets", Statement="SOFP", Section="Current Assets", SortOrder=26, LinkCodePrefix="ca.901" },

    // ── CUSTOM: Other Liability ──
    new AfsLineItem { Id=203, Key="OTHER_LIAB", Label="Other Liability", Category="Liabilities", SubCategory="Liabilities", Statement="SOFP", Section="Liabilities", SortOrder=60, LinkCodePrefix="l.950" },

        // ── INCOME STATEMENT — REVENUE ────────────────────────────────────────
        new AfsLineItem { Id=50, Key="REV_GOODS",       Label="Revenue — Sale of Goods",                Category="Income",      SubCategory="Revenue",               Statement="SPL",  Section="Revenue",               SortOrder=60, LinkCodePrefix="i.000" },
        new AfsLineItem { Id=51, Key="REV_SERVICES",    Label="Revenue — Services Rendered",            Category="Income",      SubCategory="Revenue",               Statement="SPL",  Section="Revenue",               SortOrder=61, LinkCodePrefix="i.001" },
        new AfsLineItem { Id=52, Key="REV_RENTAL",      Label="Revenue — Accommodation / Rental",       Category="Income",      SubCategory="Revenue",               Statement="SPL",  Section="Revenue",               SortOrder=62, LinkCodePrefix="i.002" },
        new AfsLineItem { Id=53, Key="REV_SLP",         Label="Revenue — Short Learning Programs",      Category="Income",      SubCategory="Revenue",               Statement="SPL",  Section="Revenue",               SortOrder=63, LinkCodePrefix="i.003" },
        new AfsLineItem { Id=54, Key="REV_TRANSPORT",   Label="Revenue — Transportation",               Category="Income",      SubCategory="Revenue",               Statement="SPL",  Section="Revenue",               SortOrder=64, LinkCodePrefix="i.004" },
        new AfsLineItem { Id=55, Key="REV_MGMT_FEE",    Label="Revenue — Management Fees",              Category="Income",      SubCategory="Revenue",               Statement="SPL",  Section="Revenue",               SortOrder=65, LinkCodePrefix="i.005" },
        new AfsLineItem { Id=56, Key="OTHER_INC",       Label="Other Income — Sundry / Miscellaneous",  Category="Income",      SubCategory="Other Income",          Statement="SPL",  Section="Other Income",          SortOrder=66, LinkCodePrefix="i.950" },
        new AfsLineItem { Id=57, Key="INVEST_REV",      Label="Other Income — Interest Received",       Category="Income",      SubCategory="Other Income",          Statement="SPL",  Section="Other Income",          SortOrder=67, LinkCodePrefix="i.910" },
        new AfsLineItem { Id=58, Key="FV_GAIN",         Label="Other Income — Fair Value Gains",        Category="Income",      SubCategory="Other Income",          Statement="SPL",  Section="Other Income",          SortOrder=68, LinkCodePrefix="i.800" },

    // ── CUSTOM: Finance Income ──
    new AfsLineItem { Id=204, Key="FIN_INCOME", Label="Finance Income", Category="Income", SubCategory="Finance Income", Statement="SPL", Section="Finance Income", SortOrder=69, LinkCodePrefix="i.911" },

    // ── CUSTOM: Finance Cost ──
    new AfsLineItem { Id=205, Key="FIN_COST", Label="Finance Cost", Category="Expenses", SubCategory="Finance Cost", Statement="SPL", Section="Finance Cost", SortOrder=75, LinkCodePrefix="e.911" },

        // ── INCOME STATEMENT — COST OF SALES ─────────────────────────────────
        new AfsLineItem { Id=60, Key="COGS",            Label="Cost of Sales — Purchases / Goods",      Category="Expenses",    SubCategory="Cost of Sales",         Statement="SPL",  Section="Cost of Sales",         SortOrder=70, LinkCodePrefix="cos.000" },
        new AfsLineItem { Id=61, Key="DIRECT_SERV",     Label="Cost of Sales — Direct Service Costs",   Category="Expenses",    SubCategory="Cost of Sales",         Statement="SPL",  Section="Cost of Sales",         SortOrder=71, LinkCodePrefix="cos.100" },
        new AfsLineItem { Id=62, Key="COS_TRANSPORT",   Label="Cost of Sales — Transportation",         Category="Expenses",    SubCategory="Cost of Sales",         Statement="SPL",  Section="Cost of Sales",         SortOrder=72, LinkCodePrefix="cos.200" },
        new AfsLineItem { Id=63, Key="COS_TRES",        Label="Cost of Sales — Technology Subscription",Category="Expenses",    SubCategory="Cost of Sales",         Statement="SPL",  Section="Cost of Sales",         SortOrder=73, LinkCodePrefix="cos.300" },
        new AfsLineItem { Id=64, Key="DEPR_COS",        Label="Cost of Sales — Depreciation",           Category="Expenses",    SubCategory="Cost of Sales",         Statement="SPL",  Section="Cost of Sales",         SortOrder=74, LinkCodePrefix="cos.900" },

        // ── INCOME STATEMENT — OPERATING EXPENSES ─────────────────────────────
        new AfsLineItem { Id=70, Key="EMP_COST",        Label="OPEX — Employee Costs / Salaries & Wages",Category="Expenses",  SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=80, LinkCodePrefix="e.700" },
        new AfsLineItem { Id=71, Key="AUDIT_FEES",      Label="OPEX — Audit / Accounting Fees",         Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=81, LinkCodePrefix="e.201" },
        new AfsLineItem { Id=72, Key="DEPR_OPEX",       Label="OPEX — Depreciation and Amortisation",   Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=82, LinkCodePrefix="e.430" },
        new AfsLineItem { Id=73, Key="LEASE_EXP",       Label="OPEX — Operating Lease / Rent Expenses", Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=83, LinkCodePrefix="e.400" },
        new AfsLineItem { Id=74, Key="BANK_CHG",        Label="OPEX — Bank Charges",                    Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=84, LinkCodePrefix="e.221" },
        new AfsLineItem { Id=75, Key="IT_EXP",          Label="OPEX — IT / Telephone / Internet",       Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=85, LinkCodePrefix="e.280" },
        new AfsLineItem { Id=76, Key="LEGAL",           Label="OPEX — Legal Expenses",                  Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=86, LinkCodePrefix="e.505" },
        new AfsLineItem { Id=77, Key="CONSULT",         Label="OPEX — Consulting / Professional Fees",  Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=87, LinkCodePrefix="e.490" },
        new AfsLineItem { Id=78, Key="BOARD_FEES",      Label="OPEX — Board / Secretarial Fees",        Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=88, LinkCodePrefix="e.450" },
        new AfsLineItem { Id=79, Key="CLEANING",        Label="OPEX — Cleaning / Office Supplies",      Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=89, LinkCodePrefix="e.802" },
        new AfsLineItem { Id=80, Key="FUEL",            Label="OPEX — Fuel / Travel / Transport",       Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=90, LinkCodePrefix="e.816" },
        new AfsLineItem { Id=81, Key="FOOD_ENT",        Label="OPEX — Refreshments / Staff Welfare",    Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=91, LinkCodePrefix="e.822" },
        new AfsLineItem { Id=82, Key="PRINT_STAT",      Label="OPEX — Printing & Stationery",           Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=92, LinkCodePrefix="e.555" },
        new AfsLineItem { Id=83, Key="INSURANCE",       Label="OPEX — Short-Term Insurance",            Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=93, LinkCodePrefix="e.570" },
        new AfsLineItem { Id=84, Key="MARKETING",       Label="OPEX — Marketing and Communications",    Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=94, LinkCodePrefix="e.515" },
        new AfsLineItem { Id=85, Key="SOFTWARE_SUB",    Label="OPEX — Software Subscriptions",          Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=95, LinkCodePrefix="e.580" },
        new AfsLineItem { Id=86, Key="BAD_DEBT",        Label="OPEX — Bad Debts / Impairment",          Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=96, LinkCodePrefix="e.100" },
        new AfsLineItem { Id=87, Key="OTHER_OPEX",      Label="OPEX — Other Operating Expenses",        Category="Expenses",    SubCategory="Operating Expenses",    Statement="SPL",  Section="Operating Expenses",    SortOrder=97, LinkCodePrefix="e.900" },

        // ── INCOME STATEMENT — OTHER ─────────────────────────────────────────
        new AfsLineItem { Id=90, Key="FIN_COSTS",       Label="Finance Costs / Interest Paid",          Category="Expenses",    SubCategory="Other Expenses",        Statement="SPL",  Section="Other Income / (Expenses)", SortOrder=100, LinkCodePrefix="e.690" },
        new AfsLineItem { Id=91, Key="TAX_EXP",         Label="Income Tax Expense",                     Category="Expenses",    SubCategory="Other Expenses",        Statement="SPL",  Section="Taxation",                  SortOrder=101, LinkCodePrefix="e.910" },
        new AfsLineItem { Id=92, Key="FV_LOSS",         Label="Fair Value Losses",                      Category="Expenses",    SubCategory="Other Expenses",        Statement="SPL",  Section="Other Income / (Expenses)", SortOrder=102, LinkCodePrefix="e.800" },
    };
}
