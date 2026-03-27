/* AFS Web App — Main JavaScript */

// ── Toast notifications ───────────────────────────────────────────────────
const Toast = {
    show(msg, type = 'success') {
        const c = document.getElementById('toast-container') || (() => {
            const el = document.createElement('div');
            el.id = 'toast-container'; el.className = 'toast-container';
            document.body.appendChild(el); return el;
        })();
        const t = document.createElement('div');
        t.className = `toast toast-${type}`;
        t.textContent = msg; c.appendChild(t);
        setTimeout(() => t.remove(), 4000);
    }
};

// ── Format currency ───────────────────────────────────────────────────────
function fmtAmount(v, rounding) {
    const abs = Math.abs(v);
    const formatted = new Intl.NumberFormat('en-ZA', {
        minimumFractionDigits: rounding === 'Thousands' ? 0 : 2,
        maximumFractionDigits: rounding === 'Thousands' ? 0 : 2
    }).format(abs);
    return v < 0 ? `(R ${formatted})` : `R ${formatted}`;
}

// ── Auto-map all accounts ─────────────────────────────────────────────────
async function autoMapAll(fyId) {
    const btn = document.getElementById('btn-automap');
    if (btn) { btn.disabled = true; btn.textContent = '⏳ Mapping...'; }
    try {
        const r = await fetch(`/TrialBalance/AutoMapAll`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgery() },
            body: JSON.stringify({ fyId })
        });
        const data = await r.json();
        if (data.success) {
            Toast.show(`✅ ${data.mapped} accounts mapped | ${data.warnings} sign warnings`, 'success');
            setTimeout(() => location.reload(), 1500);
        }
    } catch (e) {
        Toast.show('Auto-map failed: ' + e.message, 'error');
    } finally {
        if (btn) { btn.disabled = false; btn.textContent = '✨ Auto-Map All'; }
    }
}

// ── Suggest mapping for a single row ─────────────────────────────────────
async function suggestMapping(accountId, rowEl) {
    try {
        const r = await fetch(`/TrialBalance/SuggestMapping?accountId=${accountId}`);
        const data = await r.json();
        if (data.afsLineItemKey) {
            const catSel = rowEl.querySelector('.sel-category');
            const liSel  = rowEl.querySelector('.sel-lineitem');
            if (catSel) { catSel.value = data.category; catSel.dispatchEvent(new Event('change')); }
            setTimeout(() => {
                if (liSel) liSel.value = data.afsLineItemKey;
            }, 50);
            if (data.warning) showSignWarning(rowEl, data.warning);
            showConfidence(rowEl, data.confidence);
        }
    } catch (e) {
        console.warn('Suggest failed', e);
    }
}

// ── Save a single row mapping ─────────────────────────────────────────────
async function saveMapping(id, category, liKey, stmt, disclosure) {
    try {
        const params = new URLSearchParams({ id, category, afsLineItemKey: liKey, statementType: stmt, disclosureDescription: disclosure || '' });
        const r = await fetch(`/TrialBalance/SaveRow`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': getAntiForgery() },
            body: params.toString()
        });
        const data = await r.json();
        if (data.success) Toast.show('Mapping saved', 'success');
    } catch (e) {
        Toast.show('Save failed: ' + e.message, 'error');
    }
}

// ── Populate sub-class options based on category ──────────────────────────
let _lineItems = [];
async function loadLineItems() {
    if (_lineItems.length) return;
    const r = await fetch('/TrialBalance/LineItemsJson');
    _lineItems = await r.json();
}

function populateSubclass(catSel, liSel) {
    const cat = catSel.value;
    const filtered = _lineItems.filter(l => l.category === cat);
    liSel.innerHTML = '<option value="">-- Select Line Item --</option>';
    const grouped = {};
    filtered.forEach(l => {
        if (!grouped[l.section]) grouped[l.section] = [];
        grouped[l.section].push(l);
    });
    Object.entries(grouped).forEach(([section, items]) => {
        const og = document.createElement('optgroup');
        og.label = section;
        items.forEach(l => {
            const o = document.createElement('option');
            o.value = l.key; o.textContent = l.label;
            og.appendChild(o);
        });
        liSel.appendChild(og);
    });
}

// ── Sign warning helper ───────────────────────────────────────────────────
function showSignWarning(rowEl, msg) {
    let warnEl = rowEl.querySelector('.sign-warn');
    if (!warnEl) {
        warnEl = document.createElement('div');
        warnEl.className = 'sign-warn mapping-sign-warn';
        rowEl.appendChild(warnEl);
    }
    warnEl.textContent = msg;
    warnEl.style.display = msg ? '' : 'none';
}

function showConfidence(rowEl, conf) {
    let confEl = rowEl.querySelector('.conf-badge');
    if (!confEl) {
        confEl = document.createElement('span');
        confEl.className = 'conf-badge mapping-confidence';
        rowEl.appendChild(confEl);
    }
    const pct = Math.round(conf * 100);
    confEl.textContent = `${pct}%`;
    confEl.className = `conf-badge mapping-confidence ${pct >= 90 ? 'conf-high' : pct >= 70 ? 'conf-med' : 'conf-low'}`;
}

// ── Policy editor ─────────────────────────────────────────────────────────
async function savePolicy(clientId, std, key, titleEl, textEl) {
    const title = titleEl.value.trim();
    const text  = textEl.value.trim();
    if (!text) { Toast.show('Policy text cannot be empty', 'error'); return; }
    try {
        const params = new URLSearchParams({ clientId, std, key, title, text });
        const r = await fetch('/Financials/SavePolicy', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': getAntiForgery() },
            body: params.toString()
        });
        const data = await r.json();
        if (data.success) Toast.show('Policy saved ✅', 'success');
    } catch (e) {
        Toast.show('Save failed', 'error');
    }
}

async function fetchLivePolicy(standard, key, query, titleEl, textEl) {
    titleEl.value = '⏳ Searching IFRS Foundation...';
    try {
        const r = await fetch(`/Financials/FetchLivePolicy?standard=${encodeURIComponent(standard)}&key=${key}&query=${encodeURIComponent(query)}`);
        const data = await r.json();
        if (data.found && data.text) {
            textEl.value = data.text;
            titleEl.value = query;
            Toast.show('Live policy fetched ✅', 'success');
        } else {
            Toast.show('No live result — using built-in policy', 'info');
            titleEl.value = query;
        }
    } catch (e) {
        Toast.show('Internet search failed — offline?', 'error');
        titleEl.value = query;
    }
}

// ── Policy accordion ──────────────────────────────────────────────────────
document.addEventListener('click', e => {
    const header = e.target.closest('.policy-card-header');
    if (!header) return;
    const body = header.nextElementSibling;
    if (body) body.classList.toggle('open');
    const chevron = header.querySelector('.chevron');
    if (chevron) chevron.textContent = body.classList.contains('open') ? '▲' : '▼';
});

// ── File import drag-and-drop ─────────────────────────────────────────────
function initDropZone(dropId, inputId) {
    const drop  = document.getElementById(dropId);
    const input = document.getElementById(inputId);
    if (!drop || !input) return;
    drop.addEventListener('dragover', e => { e.preventDefault(); drop.classList.add('drag-over'); });
    drop.addEventListener('dragleave', () => drop.classList.remove('drag-over'));
    drop.addEventListener('drop', e => {
        e.preventDefault(); drop.classList.remove('drag-over');
        if (e.dataTransfer.files[0]) {
            const dt = new DataTransfer();
            dt.items.add(e.dataTransfer.files[0]);
            input.files = dt.files;
            drop.querySelector('.drop-label').textContent = e.dataTransfer.files[0].name;
        }
    });
    drop.addEventListener('click', () => input.click());
    input.addEventListener('change', () => {
        if (input.files[0]) drop.querySelector('.drop-label').textContent = input.files[0].name;
    });
}

// ── Anti-forgery token ────────────────────────────────────────────────────
function getAntiForgery() {
    const el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
}

// ── Init ──────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', async () => {
    await loadLineItems();
    initDropZone('drop-zone', 'file-input');

    // Wire up all category → subclass dropdowns
    document.querySelectorAll('.sel-category').forEach(catSel => {
        const rowEl = catSel.closest('[data-account-id]');
        const liSel = rowEl?.querySelector('.sel-lineitem');
        if (!liSel) return;
        populateSubclass(catSel, liSel);
        // Restore existing value
        const existingVal = liSel.dataset.currentValue;
        if (existingVal) setTimeout(() => { liSel.value = existingVal; }, 10);

        catSel.addEventListener('change', () => {
            populateSubclass(catSel, liSel);
            const bal = parseFloat(rowEl.dataset.balance || '0');
            const creditCats = ['Liabilities','Equity','Income'];
            const debitCats  = ['Assets','Expenses'];
            let warn = '';
            if (creditCats.includes(catSel.value) && bal > 0.01)
                warn = `⚠️ ${catSel.value} is credit-normal — TB balance is positive`;
            else if (debitCats.includes(catSel.value) && bal < -0.01)
                warn = `⚠️ ${catSel.value} is debit-normal — TB balance is negative`;
            showSignWarning(rowEl, warn);
        });

        liSel.addEventListener('change', async () => {
            const accountId = rowEl.dataset.accountId;
            const category  = catSel.value;
            const liKey     = liSel.value;
            const stmt      = _lineItems.find(l => l.key === liKey)?.statement === 'SOFP' ? 'Balance Sheet' : 'Income Statement';
            if (accountId && category && liKey) await saveMapping(accountId, category, liKey, stmt, '');
        });
    });

    // Dismiss alerts
    document.querySelectorAll('.alert-dismiss').forEach(btn => {
        btn.addEventListener('click', () => btn.closest('.alert').remove());
    });
});
