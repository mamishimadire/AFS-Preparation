# AFS Preparation System — C# ASP.NET Core MVC
## SNG Grant Thornton | v3.0

A professional web application for preparing Annual Financial Statements from a Trial Balance.
Modelled on Draftworx's 4-step workflow: Client Setup → Capture TB → Link Accounts → View Financials.

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) — download and install
- Visual Studio 2022 **or** VS Code with C# extension **or** any terminal

### Run in 3 commands
```bash
cd AfsWebApp
dotnet restore
dotnet run
```
Then open your browser at: **http://localhost:5000**

---

## 📋 Features

### Step 1 — Client Setup (Quick Settings)
- Entity name, trading as, registration number
- Legal form: Private Company, SOE, NPO, etc.
- Accounting standard: **IFRS for SMEs**, **IFRS Full**, **SA GAAP**
- Going concern / Non-going concern basis
- Currency rounding: Decimals / Thousands / Millions
- All dates: Signature, approval, AGM, commencement
- Directors (for signature blocks)
- Auditor, bankers, company secretary
- **Company logo upload** (PNG/JPG) — appears on cover page

### Step 2 — Capture Trial Balance
- **Import from Excel (.xlsx) or CSV** — drag-and-drop supported
- Auto-detects common column headers (Account, Description, Closing, Opening, Prior Year)
- Handles South African TB formats including Rand prefix and parenthesis negatives
- Live balance check: Debits = Credits
- Manual capture supported

### Step 3 — Link Accounts (Auto-Map)
- **✨ Auto-Map All** — reads account descriptions and maps to AFS line items using 120+ keyword rules
  - e.g. `SALES` → Income / Revenue — Sale of Goods
  - e.g. `Goods & Serv: Bank Charges` → Expenses / OPEX — Bank Charges
  - e.g. `Accumulated Depreciation` → Assets / Accumulated Depreciation
  - e.g. `Nedbank Current Account` → Assets / Cash and Cash Equivalents
- **Sign validation** — warns if credit-normal accounts (Equity, Liabilities, Income) show positive balance
- **Manual override** — Category dropdown (Assets/Liabilities/Equity/Income/Expenses) → Line Item dropdown
- Confidence indicator per auto-mapped row
- Guided by accounting equation panel

### Step 4 — View Financials
Tabbed display:
- **Statement of Financial Position** — auto-totalled, prior-year comparative
- **Statement of Profit or Loss** — Revenue, COS, OPEX, other income/expenses, tax
- **Statement of Changes in Equity** — share capital, retained earnings
- **Accounting Policies** — auto-loaded per standard and mapped accounts
  - Editable inline
  - **🌐 Search Internet** — fetches live policy text from IFRS Foundation API
- **Notes to Financial Statements** — one note per mapped line item, auto-populated
- **🖨️ Print / PDF** — browser print-to-PDF for all statements

---

## 📁 Project Structure

```
AfsWebApp/
├── Controllers/
│   └── Controllers.cs          — All 5 controllers
├── Data/
│   ├── AfsDbContext.cs         — EF Core + SQLite
│   └── SeedData.cs             — 90+ AFS line items master
├── Models/
│   └── Models.cs               — All domain models + ViewModels
├── Services/
│   ├── AutoMappingService.cs   — 120+ keyword mapping rules
│   └── PolicyService.cs        — Built-in policies + internet search
├── Views/
│   ├── Shared/_Layout.cshtml   — Main layout with sidebar
│   ├── Home/Index.cshtml       — Client dashboard
│   ├── Client/
│   │   ├── Create.cshtml
│   │   └── Setup.cshtml        — Draftworx Quick Settings
│   ├── TrialBalance/
│   │   └── WorkingTB.cshtml    — Working TB grid
│   ├── Mapping/
│   │   └── Index.cshtml        — Detailed account linking
│   └── Financials/
│       └── Index.cshtml        — All statements + policies + notes
└── wwwroot/
    ├── css/app.css             — Full custom CSS (no Bootstrap)
    └── js/app.js               — Auto-map, mapping UI, policy editor
```

---

## 🗃️ Database

SQLite database (`afs.db`) is created automatically on first run.
No SQL Server or setup required.

To reset: delete `afs.db` and restart.

---

## 📊 Trial Balance Import Format

Your TB file should have these columns (exact names or common variants):

| Required | Accepted column names |
|----------|----------------------|
| ✅ Account number | `Account`, `Account_number`, `AccountNo`, `Acc_No` |
| ✅ Account name | `Description`, `Name`, `Account_Name`, `Discription` |
| ✅ Closing balance | `Closing`, `Closing_Balance`, `CB`, `Balance` |
| Optional | `Opening`, `Opening_Balance`, `OB` |
| Optional | `Prior`, `Prior_Year`, `2025 Final` |
| Optional | `Transactions`, `Adjustments` |

**Sign convention**: Credit-normal accounts (Income, Equity, Liabilities) should be **negative** in the TB.

---

## 🗺️ Auto-Mapping Rules

The system maps 120+ account description patterns. Key examples:

| Description keyword | Maps to |
|--------------------|---------| 
| `SALES`, `Revenue` | Income → Revenue |
| `Rev : Student Accommodation` | Income → Rental/Accommodation |
| `Goods & Serv: Bank Charges` | Expenses → Bank Charges |
| `Goods & Serv: Salaries` | Expenses → Employee Costs |
| `Depreciation :` | Expenses → Depreciation |
| `Accumulated Depreciation` | Assets → Accumulated Depreciation |
| `Nedbank Current Account` | Assets → Cash and Cash Equivalents |
| `Customer Control Account` | Assets → Trade Receivables |
| `Current Tax Liability` | Liabilities → Current Tax Payable |
| `Loan from Shareholders` | Liabilities → Loan from Shareholders |
| `Retained Income` | Equity → Retained Earnings |
| `TUT Intercompany` | Liabilities → Intercompany Payable |
| `TUTEH Prop Intercompany` | Assets → Intercompany Receivable |

---

## 📜 Accounting Policies

Built-in policies for:
- **IFRS for SMEs**: PPE, Inventories, Trade Receivables, Cash, Revenue, Employee Benefits, Depreciation, Tax, Leases, Cost of Sales, Financial Instruments, Related Parties
- **IFRS Full**: IAS 16 (PPE), IFRS 9 (Financial Instruments), IFRS 15 (Revenue), IFRS 16 (Leases), IAS 19 (Employee Benefits), IAS 12 (Tax)
- **SA GAAP**: AC 123 (PPE), AC 111 (Revenue)

All policies are editable. Click **🌐 Search Internet** to fetch the latest standard text from the IFRS Foundation.

---

## 🔧 Configuration

Edit `appsettings.json` to change the database path:
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=afs.db"
  }
}
```

---

## 📄 License

SNG Grant Thornton — Internal Use. AFS Preparation System v3.0.
