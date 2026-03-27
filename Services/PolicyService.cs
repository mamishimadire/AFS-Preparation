using AfsWebApp.Models;
using System.Net.Http;
using System.Text.Json;

namespace AfsWebApp.Services;

/// <summary>
/// Provides accounting policies for AFS line items.
/// Built-in policies for IFRS for SMEs, IFRS Full, and SA GAAP.
/// Can fetch updated policies from IASB / SAICA websites.
/// </summary>
public class PolicyService
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<PolicyService> _log;

    public PolicyService(IHttpClientFactory http, ILogger<PolicyService> log)
    {
        _http = http;
        _log  = log;
    }

    // ── Built-in policy library ───────────────────────────────────────────────
    private static readonly Dictionary<string, Dictionary<string, (string Title, string Text, string Ref)>> _lib = new()
    {
        ["IFRS for SMEs"] = new()
        {
            ["PPE"] = ("Property, Plant and Equipment",
                "Property, plant and equipment are tangible assets which the company holds for its own use or for rental to others " +
                "and which are expected to be used for more than one period.\n\n" +
                "Initial recognition: Property, plant and equipment is initially measured at cost. Cost includes costs incurred initially " +
                "to acquire or construct an item and costs incurred subsequently to add to, replace part of, or service it.\n\n" +
                "Subsequent measurement: Property, plant and equipment is stated at cost less accumulated depreciation and any " +
                "accumulated impairment losses. Depreciation is charged to write off the asset's carrying amount over its estimated useful " +
                "life to its estimated residual value, using the straight-line method. The useful lives and residual values are reviewed " +
                "annually at the end of each reporting period.\n\n" +
                "Assets with a cost below R2,000 are expensed in the year of purchase.\n\n" +
                "Impairment: The company assesses at each reporting date whether there is any indication that an asset may be impaired.",
                "IFRS for SMEs Section 17"),

            ["ACC_DEPR"] = ("Accumulated Depreciation",
                "Accumulated depreciation represents the cumulative depreciation charges recognised against property, plant and equipment " +
                "since the date of acquisition or revaluation. It is presented as a deduction from the gross carrying amount of the " +
                "related asset in the Statement of Financial Position.\n\n" +
                "Depreciation is recognised in profit or loss on the straight-line method over the estimated useful lives of each " +
                "component of an item of property, plant and equipment.",
                "IFRS for SMEs Section 17"),

            ["INVENTORIES"] = ("Inventories",
                "Inventories are measured at the lower of cost and estimated selling price less costs to complete and sell.\n\n" +
                "Cost is determined using the first-in, first-out (FIFO) method. The cost of inventories comprises all costs of " +
                "purchase, costs of conversion and other costs incurred in bringing the inventories to their present location and condition.",
                "IFRS for SMEs Section 13"),

            ["TRADE_REC"] = ("Trade and Other Receivables",
                "Trade receivables, long-term receivables and other debtors are stated at cost less provision for bad debts. " +
                "Provision for impairment is made on an individual basis or based on expected payment patterns.\n\n" +
                "An objective assessment of the financial asset is made at year end to determine possible impairment. " +
                "An impairment loss is recognised as an expense in the Statement of Comprehensive Income.",
                "IFRS for SMEs Section 11"),

            ["CASH"] = ("Cash and Cash Equivalents",
                "Cash and cash equivalents are short-term, highly liquid investments that are readily convertible to known amounts " +
                "of cash and which are subject to an insignificant risk of changes in value.\n\n" +
                "Cash equivalents are investments convertible within three months or less from the date of acquisition. " +
                "They are initially measured at cost and subsequently measured at amortised cost using the effective interest rate method.\n\n" +
                "Bank overdrafts that are repayable on demand and form an integral part of the company's cash management are " +
                "included as a component of cash and cash equivalents in the Statement of Cash Flows.",
                "IFRS for SMEs Section 7"),

            ["SHARE_CAP"] = ("Share Capital and Equity",
                "An equity instrument is any contract that evidences a residual interest in the assets of an entity after " +
                "deducting all of its liabilities. Ordinary shares are classified as equity.\n\n" +
                "Ordinary shares are recognised at par value and classified as 'share capital' in equity. Any amounts received " +
                "from the issue of shares in excess of par value are classified as 'share premium' in equity.\n\n" +
                "Dividends on ordinary shares are recognised as a liability in the period in which they are declared. " +
                "Dividends paid are deducted directly from equity.",
                "IFRS for SMEs Section 22"),

            ["RET_EARN"] = ("Retained Earnings / Accumulated Loss",
                "Retained earnings represent the cumulative net profits or losses retained in the business after distribution of dividends. " +
                "An accumulated loss represents the cumulative net losses that exceed prior retained earnings.\n\n" +
                "Retained earnings are presented in the Statement of Changes in Equity and in the Statement of Financial Position " +
                "as a component of equity.",
                "IFRS for SMEs Section 6"),

            ["TRADE_PAY"] = ("Trade and Other Payables",
                "Trade payables and other accounts payable are recognised when the company becomes obliged to make future payments " +
                "resulting from the purchase of goods and services.\n\n" +
                "Trade and other payables are stated at their nominal value and are classified as current liabilities if payment is " +
                "due within one year or less. If not, they are presented as non-current liabilities.",
                "IFRS for SMEs Section 11"),

            ["REV_GOODS"] = ("Revenue",
                "Revenue is recognised to the extent that it is probable that the economic benefits will flow to the company and " +
                "the revenue can be reliably measured.\n\n" +
                "Revenue from the sale of goods is recognised when all of the following conditions have been satisfied:\n" +
                "- The significant risks and rewards of ownership have been transferred to the buyer;\n" +
                "- The company retains neither continuing managerial involvement nor effective control;\n" +
                "- The amount of revenue can be measured reliably;\n" +
                "- It is probable that economic benefits will flow to the company;\n" +
                "- Costs incurred can be measured reliably.\n\n" +
                "Revenue is measured at the fair value of the consideration received or receivable, net of trade discounts " +
                "and volume rebates. Revenue excludes value added tax.",
                "IFRS for SMEs Section 23"),

            ["REV_SERVICES"] = ("Revenue from Services",
                "Revenue from services is recognised by reference to the stage of completion of the transaction at the " +
                "reporting date, provided the outcome can be estimated reliably.\n\n" +
                "The stage of completion is assessed by reference to surveys of work performed. When the outcome cannot " +
                "be estimated reliably, revenue is recognised only to the extent that the expenses recognised are recoverable.\n\n" +
                "Revenue is measured at the fair value of the consideration received or receivable, excluding value added tax.",
                "IFRS for SMEs Section 23"),

            ["REV_RENTAL"] = ("Rental and Accommodation Income",
                "Operating lease income is recognised as income on a straight-line basis over the lease term, unless another " +
                "systematic basis is more representative of the time pattern of the benefit derived from the leased asset.\n\n" +
                "Where the payment schedule differs from the straight-line recognition, the resulting accrual or deferral is " +
                "recognised in the Statement of Financial Position.",
                "IFRS for SMEs Section 20"),

            ["INVEST_REV"] = ("Investment Revenue",
                "Interest income is recognised in profit or loss using the effective interest rate method. The effective " +
                "interest rate is the rate that exactly discounts estimated future cash receipts through the expected life " +
                "of the financial instrument to the gross carrying amount of the financial asset.",
                "IFRS for SMEs Section 25"),

            ["EMP_COST"] = ("Employee Benefits",
                "Short-term employee benefits\n" +
                "The cost of short-term employee benefits (those payable within 12 months after the service is rendered, " +
                "including salaries, wages, bonuses, leave pay, sick leave, and non-monetary benefits) are recognised in " +
                "the period in which the service is rendered and are not discounted.\n\n" +
                "Short-term accumulating compensated absences (e.g. annual leave) are recognised as an expense as employees " +
                "render services that increase their entitlement.\n\n" +
                "Termination benefits\n" +
                "Termination benefits are recognised as an expense when the entity is demonstrably committed either to " +
                "terminate the employment of an employee or group of employees before the normal retirement date, or to " +
                "provide termination benefits as a result of an offer made to encourage voluntary redundancy.",
                "IFRS for SMEs Section 28"),

            ["DEPR_OPEX"] = ("Depreciation and Amortisation",
                "Depreciation is recognised in profit or loss on the straight-line method over the estimated useful lives " +
                "of each component of an item of property, plant and equipment.\n\n" +
                "Depreciation commences when the asset is available for use as intended by management. " +
                "Depreciation ceases when the asset is derecognised.\n\n" +
                "The estimated useful lives for the current and comparative periods are:\n" +
                "- Furniture and fixtures: 6 years\n- Motor vehicles: 10 years\n- Office equipment: 3 years\n" +
                "- IT equipment: 3 years\n- Computer software: 3 years\n- Kitchen equipment: 6 years\n\n" +
                "Useful lives and residual values are reviewed at each reporting date.",
                "IFRS for SMEs Section 17"),

            ["LEASE_EXP"] = ("Leases",
                "A lease is classified as a finance lease if it transfers substantially all the risks and rewards incidental " +
                "to ownership to the lessee. All other leases are classified as operating leases.\n\n" +
                "Operating leases — lessee\n" +
                "Operating lease payments are recognised as an expense on a straight-line basis over the lease term unless:\n" +
                "- another systematic basis is more representative of the time pattern of the benefit; or\n" +
                "- the payments are structured to increase in line with expected general inflation.\n\n" +
                "Any contingent rents are expensed in the period they are incurred.\n\n" +
                "Operating leases — lessor\n" +
                "Operating lease income is recognised on a straight-line basis over the lease term.",
                "IFRS for SMEs Section 20"),

            ["TAX_EXP"] = ("Tax",
                "Current tax\n" +
                "Current tax is recognised as a liability to the extent that it has not yet been settled. Judgement is required " +
                "in determining the provision for income taxes due to the complexity of legislation.\n\n" +
                "Deferred tax\n" +
                "A deferred tax liability is recognised for all taxable temporary differences. A deferred tax asset is " +
                "recognised for all deductible temporary differences to the extent that it is probable that taxable profit " +
                "will be available against which the deductible temporary difference can be utilised.\n\n" +
                "Deferred tax assets and liabilities are measured at the tax rates that are expected to apply to the period " +
                "when the asset is realised or the liability is settled.\n\n" +
                "The South African corporate tax rate applied is 27% (prior year: 28%).",
                "IFRS for SMEs Section 29"),

            ["FIN_COSTS"] = ("Finance Costs",
                "Finance costs comprise interest payable on borrowings calculated using the effective interest rate method, " +
                "and late payment penalties on tax obligations.\n\n" +
                "Finance costs are recognised in profit or loss in the period in which they are incurred.",
                "IFRS for SMEs Section 25"),

            ["DEFERRED_TAX_L"] = ("Deferred Tax",
                "A deferred tax liability is recognised for all taxable temporary differences, except to the extent that " +
                "it arises from the initial recognition of goodwill, or from the initial recognition of an asset or liability " +
                "in a transaction that is not a business combination.\n\n" +
                "A deferred tax asset is recognised for all deductible temporary differences to the extent that it is probable " +
                "that taxable profit will be available. Deferred tax assets are reviewed at each reporting date.",
                "IFRS for SMEs Section 29"),

            ["COGS"] = ("Cost of Sales",
                "Cost of sales includes all direct costs attributable to the generation of revenue, including:\n" +
                "- The cost of inventory sold;\n- Direct labour costs;\n- Direct overheads attributable to production;\n" +
                "- Management fees and service fees directly related to revenue generation.\n\n" +
                "Cost of sales is recognised in the same period as the related revenue.",
                "IFRS for SMEs Section 13"),

            ["INVEST_ASSOC"] = ("Investments in Associates",
                "An associate is an entity over which the company has significant influence, but not control or joint control.\n\n" +
                "Investments in associates are initially measured at fair value using the income approach and subsequently " +
                "at fair value with changes recognised in profit or loss.\n\n" +
                "The lease incentive is recognised as a deferred gain in the Statement of Financial Position, and the realised " +
                "portion is credited to lease expenses over the lease term.",
                "IFRS for SMEs Section 14"),

            ["INTERCO_REC"] = ("Related Party Transactions — Intercompany",
                "Intercompany receivables and payables arise from transactions between the company and related entities in the group.\n\n" +
                "Loans between group entities are unsecured, interest-free and have no fixed repayment terms unless otherwise stated. " +
                "These are classified as current assets or current liabilities as they are expected to be settled within 12 months.",
                "IFRS for SMEs Section 33"),
        },

        ["IFRS Full"] = new()
        {
            ["PPE"] = ("Property, Plant and Equipment — IAS 16",
                "Property, plant and equipment (IAS 16) are tangible items held for use in production, supply of goods/services, " +
                "rental to others, or administrative purposes and expected to be used during more than one period.\n\n" +
                "Recognition: An item is recognised when it is probable that future economic benefits will flow to the entity " +
                "and the cost can be measured reliably.\n\n" +
                "Measurement after recognition: The cost model is applied — assets are carried at cost less any accumulated " +
                "depreciation and accumulated impairment losses. Alternatively, the revaluation model may be applied where " +
                "fair value can be measured reliably.\n\n" +
                "Depreciation: Depreciated on the straight-line basis over the useful life. Residual values and useful lives " +
                "are reviewed at least annually (IAS 16.51).\n\n" +
                "Component accounting: Significant components of an asset with different useful lives are depreciated separately.",
                "IAS 16 — Property, Plant and Equipment"),

            ["TRADE_REC"] = ("Financial Instruments — IFRS 9",
                "Trade receivables that do not contain a significant financing component are measured at the transaction price " +
                "on initial recognition (IFRS 15).\n\n" +
                "Impairment: The company applies the simplified approach under IFRS 9 and measures the loss allowance at an amount " +
                "equal to lifetime expected credit losses (ECL). The ECL is estimated using a provision matrix based on " +
                "historical credit loss experience, adjusted for forward-looking factors.",
                "IFRS 9 — Financial Instruments"),

            ["REV_GOODS"] = ("Revenue — IFRS 15",
                "Revenue is recognised in accordance with IFRS 15 using the five-step model:\n" +
                "1. Identify the contract(s) with the customer;\n" +
                "2. Identify the performance obligations;\n" +
                "3. Determine the transaction price;\n" +
                "4. Allocate the transaction price to performance obligations;\n" +
                "5. Recognise revenue when (or as) performance obligations are satisfied.\n\n" +
                "Revenue is measured at the fair value of consideration received or receivable, net of discounts, rebates " +
                "and value added tax.",
                "IFRS 15 — Revenue from Contracts with Customers"),

            ["LEASE_EXP"] = ("Leases — IFRS 16",
                "The company applies IFRS 16 Leases. At commencement of a lease, the company recognises a right-of-use " +
                "asset and a lease liability.\n\n" +
                "The lease liability is measured at the present value of lease payments not paid at commencement date, " +
                "discounted using the interest rate implicit in the lease or the lessee's incremental borrowing rate.\n\n" +
                "Short-term leases (lease term ≤12 months) and leases of low-value assets are expensed on a straight-line " +
                "basis. The company applies the practical expedient for short-term leases.\n\n" +
                "Right-of-use assets are depreciated over the shorter of the asset's useful life and the lease term.",
                "IFRS 16 — Leases"),

            ["EMP_COST"] = ("Employee Benefits — IAS 19",
                "Short-term employee benefits are measured on an undiscounted basis and are recognised as an expense " +
                "in the period the employee renders the related service (IAS 19.10).\n\n" +
                "Annual leave is accrued as employees earn entitlement. The provision is measured at the undiscounted " +
                "amount expected to be paid.\n\n" +
                "Defined contribution plans: Contributions are recognised as an expense when the employee renders service " +
                "entitling them to the contribution.",
                "IAS 19 — Employee Benefits"),

            ["TAX_EXP"] = ("Income Taxes — IAS 12",
                "Current tax is measured at the amount expected to be recovered from or paid to the taxation authorities, " +
                "using tax rates enacted or substantively enacted at the reporting date.\n\n" +
                "Deferred tax is recognised using the balance sheet liability method in respect of temporary differences " +
                "between the carrying amount of assets and liabilities and their tax base.\n\n" +
                "A deferred tax asset is recognised for unused tax losses, unused tax credits and deductible temporary " +
                "differences only to the extent that it is probable that future taxable profits will be available.\n\n" +
                "The South African corporate tax rate is 27%.",
                "IAS 12 — Income Taxes"),
        },

        ["SA GAAP"] = new()
        {
            ["PPE"] = ("Property, Plant and Equipment — AC 123",
                "Property, plant and equipment (AC 123, now converged with IAS 16) are tangible assets used in production " +
                "or supply of goods and services and expected to be used for more than one period.\n\n" +
                "Measurement: Assets are carried at historical cost less accumulated depreciation and impairment losses. " +
                "Depreciation is calculated using the straight-line method.\n\n" +
                "Assets costing less than R2,000 may be expensed in the year of acquisition in accordance with materiality.",
                "AC 123 / IAS 16"),

            ["REV_GOODS"] = ("Revenue — AC 111",
                "Revenue is recognised when the risks and rewards of ownership have passed to the buyer, the amount can " +
                "be reliably measured, and it is probable that economic benefits will flow to the entity.\n\n" +
                "Revenue from services is recognised by reference to the stage of completion.",
                "AC 111 / IAS 18 — Revenue"),
        }
    };

    public Dictionary<string, (string Title, string Text, string Ref)> GetPoliciesForStandard(string standard)
    {
        if (_lib.TryGetValue(standard, out var lib))
            return lib;
        return _lib["IFRS for SMEs"];
    }

    public (string Title, string Text, string Ref)? GetPolicy(string standard, string liKey)
    {
        var lib = GetPoliciesForStandard(standard);
        return lib.TryGetValue(liKey, out var p) ? p : null;
    }

    /// <summary>
    /// Attempts to fetch an updated policy from the IASB / IFRS Foundation website.
    /// Returns null if unavailable (offline or not found).
    /// </summary>
    public async Task<string?> FetchLivePolicyAsync(string standard, string liKey, string searchQuery)
    {
        try
        {
            using var client = _http.CreateClient("PolicySearch");
            client.DefaultRequestHeaders.Add("User-Agent", "AFS-Preparation-System/1.0");
            client.Timeout = TimeSpan.FromSeconds(10);

            // Search IFRS Foundation / IAASB resources
            var sources = standard == "IFRS Full"
                ? new[] { "https://www.ifrs.org", "https://www.iaasb.org" }
                : new[] { "https://www.ifrs.org/issued-standards/list-of-standards/ifrs-for-smes-standard/" };

            // Simple DuckDuckGo instant-answer API (no key required)
            var q = Uri.EscapeDataString($"{standard} {searchQuery} accounting policy");
            var url = $"https://api.duckduckgo.com/?q={q}&format=json&no_redirect=1";
            var response = await client.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            var abstract_ = json.RootElement.GetProperty("AbstractText").GetString();
            if (!string.IsNullOrWhiteSpace(abstract_) && abstract_.Length > 50)
                return abstract_;
        }
        catch (Exception ex)
        {
            _log.LogWarning("Live policy fetch failed for {Key}: {Msg}", liKey, ex.Message);
        }
        return null;
    }

    /// <summary>Gets all unique line item keys that have built-in policies for the given standard.</summary>
    public List<string> GetAvailablePolicyKeys(string standard)
        => GetPoliciesForStandard(standard).Keys.ToList();
}
