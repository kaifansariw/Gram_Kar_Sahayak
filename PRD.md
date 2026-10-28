# PRD: Gram Panchayat Tax & Fee Collection Tracker

**Working name:** Gram Kar Sahayak (ગ્રામ કર સહાયક / ग्राम कर सहायक)
**Stack:** ASP.NET Core MVC (C#), Razor views, Entity Framework Core (Pomelo provider), MySQL
**UI style:** Neubrutalism with a village theme, based on the supplied reference landing page (full details in Design.md)
**Version:** 1.1 (college project scope)

---

## 1. Problem Statement

Gram Panchayats collect house tax, water tax, sanitation/light tax and other fees. Most record these in paper registers. This causes:

1. **Defaulters are hard to identify.** Someone has to flip through pages to see who has not paid.
2. **Pending amounts and late fees are calculated by hand**, so mistakes and disputes are common.
3. **Receipts are handwritten**, and lost or duplicated receipts lead to arguments.
4. **No quick answer to a villager's question** ("How much do I owe?") without searching the register.
5. **No summary for the Sarpanch/Gram Sevak**: total collected, total pending, who owes the most.

## 2. Goal

Give a Panchayat clerk a very simple tool to record households, raise yearly dues, record payments, print receipts, and instantly see who owes what, with late fees calculated automatically.

### Success Metrics
| Metric | Target |
|---|---|
| Time to record a payment | Under 30 seconds |
| Time to find a household's pending amount | Under 10 seconds |
| Pending/late fee calculation errors | Zero (computed by system) |
| A first-time clerk can finish core tasks | After a 10-minute walkthrough |

## 3. Users

| Role | Who | What they need |
|---|---|---|
| **Clerk / Gram Sevak (Admin)** | Panchayat office staff, possibly low computer literacy | Add households, record payments, print receipts, see defaulters |
| **Sarpanch (Viewer)** | Elected head | Read-only dashboard: collection totals, defaulter list |
| **Villager (public lookup)** *(optional, Phase 2)* | Any household | Check own pending dues by house number |

For the college project, build **Admin** and **Sarpanch (read-only)** roles. Villager lookup is optional.

## 4. Scope

### In scope (MVP)
- Login (Admin, Sarpanch)
- Landing page in the reference's neubrutalism layout: nav, hero with highlighted headline word, framed Sarpanch + village illustration, floating sticker cards, Sarpanch message card, 3-step "How it works", 3 feature cards (House Tax, Water Tax, Receipt), village-people section, FAQ accordion and footer
- Dashboard (totals and quick actions)
- Household (property) list, add and edit
- Tax types setup (house tax, water tax, etc., with amount and due date)
- Yearly dues generation for all households
- Record payment with printable receipt
- Pending dues (defaulters) report with late fee, printable and exportable
- Language toggle: Gujarati / Hindi / English

### Out of scope
- Online payment gateway
- SMS/WhatsApp notifications (future)
- Multiple villages and multi-tenant support
- Mobile app (the web app must be mobile-friendly)

## 5. Pages (only the useful ones)

| # | Page | Route | Role | Purpose |
|---|---|---|---|---|
| 1 | Landing | `/` | Public | Single-page marketing/info layout (see FR-9): hero with Sarpanch image, steps, features, FAQ, footer; **Login** button; "Check my dues" lookup (optional) |
| 2 | Login | `/Account/Login` | Public | Username and password |
| 3 | Dashboard | `/Dashboard` | Admin, Sarpanch | Today's collection, year total collected, total pending, number of defaulters, four big action tiles |
| 4 | Households | `/Households` | Admin (edit), Sarpanch (view) | Search, list, add/edit a household |
| 5 | Record Payment | `/Payments/Create` | Admin | Find the household, see dues, enter amount, save, print receipt |
| 6 | Receipt | `/Payments/Receipt/{id}` | Admin | Print-friendly receipt |
| 7 | Pending Dues Report | `/Reports/Pending` | Admin, Sarpanch | Defaulter list with filters, print and export CSV |
| 8 | Tax Settings | `/Settings/TaxTypes` | Admin | Define tax types, yearly amounts, due dates, late fee rules, and generate yearly dues |

Households have an add/edit form (a modal or simple page). Receipt and Tax Settings are small supporting pages.

## 6. Functional Requirements

### FR-1 Authentication
- Login with username and password (hashed using ASP.NET Core Identity's `PasswordHasher`).
- Role-based access: `Admin`, `Sarpanch`. The Sarpanch can't add, edit or delete anything.
- Auto logout after 20 minutes idle.

### FR-2 Households
- Fields: House No. (unique), Owner Name, Father/Husband Name, Mobile (optional), Ward/Faliya (street area), Property Type (Residential / Shop / Other), Has Water Connection (Y/N), Status (Active/Inactive).
- Search by house number, owner name or mobile; filter by ward.
- No hard delete. Deactivate instead, to protect payment history.

### FR-3 Tax Types
- Fields: Name (House Tax, Water Tax, Sanitation, Street Light, etc.), Default Annual Amount, Due Date (day and month), Late Fee Type (Flat / Percent per month), Late Fee Value, Applies To (All / Water connection only / Shops only).
- Admin can edit amounts per year.

### FR-4 Generate Yearly Dues
- Admin picks a Financial Year (e.g. 2025-26) and clicks **Generate Dues**.
- The system creates a `Demand` row for every active household and every applicable tax type.
- Re-running must not create duplicates (unique on Household + TaxType + Year).
- Admin can adjust a single household's demand amount (e.g. a concession) with a reason note.

### FR-5 Record Payment
1. Admin searches a household (house no. or name).
2. The screen shows each pending demand: tax name, year, amount, late fee, total payable.
3. Admin enters the amount received and payment mode (Cash / UPI / Cheque) and an optional reference.
4. The system allocates the payment **oldest year first, late fee first, then principal**, and shows a preview before saving.
5. On save, a unique receipt number (e.g. `GP-2025-000123`) is generated and the receipt page opens.
- Part payments are allowed. Overpayment is not (block with a clear message).
- Payments can't be edited. A mistaken entry is **cancelled** (with a reason) and recorded again, so the audit trail stays intact.

### FR-6 Pending Dues Report
- Columns: House No., Owner, Ward, Mobile, Tax-wise pending, Late Fee, Total Due, Oldest Due Date.
- Filters: Financial Year, Ward, Tax Type, Minimum amount.
- Sort by amount or oldest due.
- Summary row with total pending.
- Print-friendly layout and CSV export.

### FR-7 Dashboard
- Cards: **Collected Today**, **Collected This Year**, **Total Pending**, **Defaulter Households**.
- A simple bar or progress visual: Collected vs Pending.
- Big shortcut tiles: Record Payment, Pending List, Households, Tax Settings.

### FR-8 Language
- UI labels in Gujarati, Hindi and English (resource files `.resx`), switchable from the header. Gujarati is the default for Gujarat deployments.

### FR-9 Landing Page Content
The landing page follows the reference design's section order:
1. **Nav:** logo and Panchayat name, links, language pill (ગુ | हि | EN), **Login** button.
2. **Hero:** headline with one highlighted word ("Village **tax**, made simple"), a short paragraph, **Login** and **Check my dues** buttons, and a framed illustration of the Sarpanch with the village. Two floating sticker cards: "Instant receipt" and "Safe records".
3. **Sarpanch message card:** name, photo and a short message.
4. **How it works (3 numbered steps):** tell your house number, see your dues, pay and take your receipt.
5. **Feature cards (3):** House Tax, Water Tax, Receipt, each with a coloured icon square and a one-line explanation.
6. **Village section:** illustration of villagers plus a short statement and a Login button.
7. **FAQ accordion** with tabs General / Tax / Payment / Receipt (content editable in resource files).
8. **Footer:** office address, timings, phone, links, and a pill-shaped house-number input for **Check my dues**.
- **Check my dues** (public, read-only): enter house number plus the registered mobile to see pending amount only. No personal details beyond the owner's first name. If not built in the MVP, the button shows office timings and contact instead.
- The Sarpanch photo, name, Panchayat name, address and timings are editable by Admin in Settings.

## 7. Business Logic: Pending and Late Fee

**Definitions**
- `Demand.Amount`: the principal for one tax, one household, one year.
- `PaidPrincipal`, `PaidLateFee`: the sums allocated from payments.
- `DueDate`: from the tax type, for that year.

**Late fee (calculated on the fly, never edited manually)**
```
if today <= DueDate:           lateFee = 0
else monthsLate = ceil(days overdue / 30)

  Flat:     lateFee = FlatValue                       (one-time)
  Percent:  lateFee = Principal_remaining * (Rate/100) * monthsLate
  lateFee   = min(lateFee, MaxCap)                    (optional cap, e.g. 50% of principal)

pendingLateFee = lateFee - PaidLateFee
pendingPrincipal = Amount - PaidPrincipal
totalPayable = pendingPrincipal + pendingLateFee
```
- The late fee is computed using **today's date** when displayed. At payment time the computed value is **frozen** in the payment allocation record, so old receipts never change.

**Defaulter:** a household with `totalPayable > 0` on any demand whose `DueDate` has passed.

**Worked example**
House Tax 2025-26: ₹1,200, due 31 Mar 2026, late fee 2% per month. Today is 15 Jun 2026, so 76 days overdue means 3 months.
Late fee = 1200 x 2% x 3 = ₹72. Total payable = ₹1,272.

## 8. Data Model (MySQL)

```
Users(UserId PK, Username UNIQUE, PasswordHash, FullName, Role, IsActive)

Households(HouseId PK, HouseNo UNIQUE, OwnerName, GuardianName, Mobile,
           Ward, PropertyType, HasWater, IsActive, CreatedAt)

TaxTypes(TaxTypeId PK, Name, DefaultAmount, DueDay, DueMonth,
         LateFeeType, LateFeeValue, MaxLateFeePercent, AppliesTo, IsActive)

Demands(DemandId PK, HouseId FK, TaxTypeId FK, FinancialYear, Amount,
        DueDate, Remarks,
        UNIQUE(HouseId, TaxTypeId, FinancialYear))

Payments(PaymentId PK, ReceiptNo UNIQUE, HouseId FK, PaymentDate,
         TotalAmount, Mode, Reference, ReceivedByUserId FK,
         IsCancelled, CancelReason)

PaymentAllocations(AllocId PK, PaymentId FK, DemandId FK,
                   PrincipalPaid, LateFeePaid)
```

Relationships: Household 1-N Demands; TaxType 1-N Demands; Household 1-N Payments; Payment 1-N PaymentAllocations; Demand 1-N PaymentAllocations.

> The idea sheet listed 3 tables (Households, TaxTypes, Payments). **Demands** and **PaymentAllocations** are added because dues must exist before they can be "pending", and part payments need tracking.

## 9. Non-Functional Requirements

| Area | Requirement |
|---|---|
| Usability | Minimum 18px near-black body text, large buttons (min 48px height), thick black outlines and hard shadows on all cards/buttons/inputs (neubrutalism), max 3 clicks to any core task |
| Responsive | Works on a low-end Android phone and a desktop |
| Performance | Household search results in under 1 second for up to 5,000 households |
| Security | Hashed passwords, anti-forgery tokens, authorization attributes on all controllers, parameterized queries (EF Core) |
| Data safety | Daily database backup script (`mysqldump`); no hard deletes on money data |
| Auditability | `CreatedBy`, `CreatedAt` on payments and demands |
| Offline-friendly | Runs on a local PC at the Panchayat office (localhost or LAN); no heavy external dependencies, fonts bundled locally |
| Browser | Latest Chrome and Edge |

## 10. Technical Architecture

- **Pattern:** MVC with a service layer (`DuesCalculator`, `PaymentService`, `DemandGenerator`).
- **Project layout:**
  ```
  GramKar.Web/
    Controllers/  Account, Dashboard, Households, Payments, Reports, Settings
    Models/       Entities + ViewModels
    Services/     DuesCalculator.cs, PaymentService.cs, DemandGenerator.cs
    Data/         AppDbContext.cs, Migrations/, Seed.cs
    Views/        Shared/_Layout.cshtml + one folder per controller
    Resources/    SharedResource.gu.resx, .hi.resx, .en.resx
    wwwroot/      css/, js/, img/, fonts/
  ```
- **Packages:** `Pomelo.EntityFrameworkCore.MySql`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (or a custom simple login), `CsvHelper` (export).
- **Seed data:** 1 Admin, 1 Sarpanch, 3 tax types, about 30 sample households, so the demo works immediately.

## 11. User Flows

**Collect a payment:** Dashboard → *Record Payment* → type house no. → see dues and late fee → enter amount → *Save* → Receipt → *Print*.

**Find defaulters:** Dashboard → *Pending List* → choose year/ward → *Print* or *Export*.

**Start a new year:** Tax Settings → update amounts → *Generate Dues for 2026-27*.

## 12. Acceptance Criteria (samples)

- Given a household with an unpaid house tax past its due date, the pending page shows principal, late fee and total, matching the formula in section 7.
- Given a part payment of ₹500 against ₹1,272 payable, the allocation first clears late fee (₹72), then ₹428 of principal, and the remaining payable becomes ₹772.
- Generating dues twice for the same year creates no duplicates.
- A Sarpanch user can't see add, edit or cancel buttons and gets "Access Denied" if they open those URLs.
- A cancelled payment restores the demand's pending amount and remains visible in history marked "Cancelled".
- The receipt prints on a single A5/A4 page with the Panchayat name, receipt number, date, house, items paid and the clerk's name.

## 13. Milestones (suggested for a college timeline)

| Week | Deliverable |
|---|---|
| 1 | DB schema, EF migrations, seed data, login and roles, `theme.css` (neubrutalism tokens), shared layout |
| 2 | Households CRUD, Tax types, Demand generation |
| 3 | Payment recording with allocation, receipt |
| 4 | Pending report, dashboard, landing page (hero, steps, features, FAQ, footer), language toggle, UI polish, testing, demo |

## 14. Risks and Mitigations

| Risk | Mitigation |
|---|---|
| Users find software hard | Few pages, big buttons, Gujarati UI, clear text labels |
| Wrong late-fee rules for a real Panchayat | Make rules configurable per tax type; confirm with a real Gram Sevak |
| Data entry of existing households is tedious | Add CSV/Excel import for households (stretch goal) |
| Data loss | Backup script and no hard deletes |
| Reference style uses small, light-grey text that villagers may struggle to read | Keep the style (outlines, shadows, cards) but enforce 18px+ dark text and high-contrast buttons |
| Illustrations and Sarpanch photo not available in time | Use SVG placeholders; make the photo uploadable in Settings |

## 15. Future Enhancements
SMS/WhatsApp reminders to defaulters, villager self-lookup by house number, UPI QR on receipt, Excel bulk import, yearly report PDF for the Gram Sabha.