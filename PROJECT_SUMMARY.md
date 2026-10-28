# Gram Kar Sahayak (ગ્રામ કર સહાયક / ग्राम कर सहायक)
### Gram Panchayat Tax & Fee Collection Tracker

> **Project Scope:** College Project & Rural Governance System (v1.1)  
> **Tech Stack:** ASP.NET Core MVC (.NET 8.0), Razor Views, Entity Framework Core (Pomelo MySQL + SQLite zero-config fallback), Vanilla CSS Neubrutalism Design System.

---

## 🏛️ Project Overview & Achievements

In accordance with [PRD.md](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/PRD.md) and [Design.md](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/Design.md), **Gram Kar Sahayak** is designed to replace manual paper registers in Gram Panchayats. It provides a simple, accessible interface tailored for Panchayat clerks (Talati-cum-Mantri) and Sarpanch with **bold Neubrutalism aesthetics**, high contrast typography (18px+), and **trilingual support (Gujarati, Hindi, English)**.

---

## 🎨 Neubrutalism x Village Theme Implementation

All UI principles and tokens from [Design.md](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/Design.md) have been implemented in [theme.css](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/wwwroot/css/theme.css):

| Token | Value | Applied To |
|---|---|---|
| `--bg` | `#FFF8EC` | Warm cream background with faint 45° diagonal line pattern |
| `--ink` | `#111111` | 3px bold black outlines, headings, text, and hard shadows |
| `--primary` | `#E8502F` | Terracotta primary action buttons |
| `--highlight` | `#2F7DE1` | Sky blue `<mark class="hero-mark">` headline badge & active links |
| `--turmeric` | `#FFC933` | Badges, icon squares, summary banners, and active nav pills |
| `--leaf` | `#2E9E4F` | Success states, paid badges, save actions |
| `--danger` | `#D62828` | Overdue amounts, defaulters, error alerts |
| `--shadow` | `6px 6px 0 #111` | Hard offset card shadows (no blur) |
| `--shadow-sm` | `4px 4px 0 #111` | Button and input shadows (hover lifts, press sinks) |

### 🖼️ Vector Illustrations (SVGs)
- **Sarpanch Avatar ([sarpanch.svg](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/wwwroot/img/sarpanch.svg)):** Friendly Sarpanch portrait with turban (પાઘડી), shawl, tilak, and garland.
- **Village Hero Scene ([village_hero.svg](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/wwwroot/img/village_hero.svg)):** Village landscape with Panchayat Ghar, huts, fields, well, banyan tree, and rising sun.
- **Village People Band ([village_people.svg](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/wwwroot/img/village_people.svg)):** Farmer with plough, woman carrying water pots, schoolchild, and elder with register.
- **Warli Repeating Strip ([warli_strip.svg](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/wwwroot/img/warli_strip.svg)):** Authentic Warli folk art strip framing receipts and footer.
- **Emblem ([emblem.svg](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/wwwroot/img/emblem.svg)):** Gram Panchayat official seal.

---

## 🚀 Key Functional Modules

### 1. Landing Page (`/`)
- **Hero Section:** Headline with highlighted mark word ("ગામનો **કર**, હવે સરળ"), descriptive copy, **[Login]** and **[Check my dues]** buttons, framed illustration, and floating sticker cards (*"✔ Instant receipt"*, *"🔒 Safe records"*).
- **Sarpanch Message Card:** Sarpanch photo, name, and village quote.
- **How It Works (3 Numbered Steps):** Tell house number → See dues clearly → Pay and take receipt.
- **3 Feature Cards:** House Tax (Turmeric), Water Tax (Sky), Receipt (Rose).
- **Village People Section:** Illustrated community band with *"We keep village accounts clear"* statement.
- **Interactive FAQ Accordion:** Filterable by category tabs (General, Tax, Payment, Receipt).
- **Public Dues Lookup:** Villagers enter house number (e.g. `PF-01`) to view pending tax items and late fees without revealing sensitive personal information.

### 2. Authentication & Roles (`/Account/Login`)
- Hashed passwords via ASP.NET Core `PasswordHasher<User>`.
- **Admin (Talati-cum-Mantri):** Full read-write permissions, collection, demands, settings.
- **Sarpanch (Viewer):** Read-only dashboards and reports, delete/edit actions restricted.
- Automatic session idle timeout of 20 minutes with sliding expiration.
- Quick 1-click demo login buttons for evaluation.

### 3. Dashboard (`/Dashboard`)
- 4 Key Stat Cards: **Collected Today**, **Collected This Year**, **Total Pending**, **Defaulter Households**.
- **Collection vs Pending Progress Bar:** Bold Neubrutalism visual indicator with percentage calculation.
- 4 Action Shortcut Tiles (Record Payment, Pending List, Households, Tax Settings).
- Recent Payments audit table.

### 4. Households Management (`/Households`)
- Search by house number, owner name, or mobile number; filter by Ward / Faliya.
- Add and Edit property forms with validation.
- Soft deletion / deactivation to preserve financial audit integrity.
- Detailed view with tax history and payment timeline.

### 5. Automated Late Fee Calculation & Dues Generation
- Configured in [DuesCalculator.cs](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/Services/DuesCalculator.cs) following PRD Section 7:
  $$\text{Months Overdue} = \left\lceil \frac{\text{Days Overdue}}{30} \right\rceil$$
  $$\text{Late Fee} = \min(\text{Pending Principal} \times \text{Rate} \times \text{Months Overdue}, \text{Max Cap})$$
- [DemandGenerator.cs](file:///c:/Users/MAX/OneDrive/Desktop/Gram%20Kosh/Services/DemandGenerator.cs): Admin picks financial year (e.g. `2026-27`) and generates demands for all active households with duplicate prevention.

### 6. 3-Step Payment Flow & Official Receipt (`/Payments/Create`)
- **Step 1:** Search household by number or mobile.
- **Step 2:** Displays pending demands and computed late fees.
- **Step 3:** Enter amount (full or partial payments supported), select payment mode (Cash / UPI / Cheque), view **live allocation preview** (allocating oldest dues first, late fees first, then principal).
- **Official Print-Friendly Receipt (`/Payments/Receipt/{id}`):** Formatted for A5/A4 printing with Warli borders, emblem, itemized breakdown, amount in words, and authority signature line.
- Audit trail cancellation with mandatory reason.

### 7. Pending Defaulters Report (`/Reports/Pending`)
- Filter by Financial Year, Ward, Tax Type, and Minimum Amount.
- Sorting by highest pending amount, lowest pending amount, or oldest due date.
- Summary Banner: *"Total Pending ₹ X from N houses"*.
- **Print Report:** Dedicated printer-friendly view.
- **Export CSV:** Downloads formatted `.csv` using `CsvHelper`.

---

## 🗄️ Database Strategy

- **Dual Provider Support:** 
  1. **MySQL Server:** Configured with `Pomelo.EntityFrameworkCore.MySql`.
  2. **SQLite Auto-Fallback:** Automatically falls back to SQLite (`gramkosh.db`) if MySQL credentials are not set on the local machine, guaranteeing 100% out-of-the-box execution for college presentations and development.
- **Pre-Seeded Data:** Admin user (`admin`), Sarpanch user (`sarpanch`), 4 Tax Types, 30 realistic households across 4 wards, demands for FY 2024-25 and FY 2025-26, and sample payments.

---

## 🛠️ How to Run Locally

```bash
# 1. Open terminal in the project directory
cd "c:\Users\MAX\OneDrive\Desktop\Gram Kosh"

# 2. Run the application
dotnet run --launch-profile http
```
The application will start on: **`http://localhost:5209`**

### Demo Credentials
- **Clerk / Admin:** Username: `admin` | Password: `Admin@123`
- **Sarpanch / Viewer:** Username: `sarpanch` | Password: `Sarpanch@123`
