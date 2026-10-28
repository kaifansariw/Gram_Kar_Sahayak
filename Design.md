# Design.md: Gram Kar Sahayak UI Design System (v2, Neubrutalism x Village)

> **Reference:** the uploaded "Deco." landing page (neubrutalism trend). We keep its *structure and style*: thick black outlines, hard offset shadows, rounded cards, a highlighted headline word, floating sticker cards, numbered steps, feature cards, an FAQ accordion and a pill footer. We change its *content and colours* to a village theme, and make text larger and darker because villagers and older clerks will use it.

---

## 1. Design Principles

1. **Neubrutalism, friendly version.** Bold black outlines, flat colours and hard shadows, with no blur, glass or gradients (except the sky in illustrations).
2. **Village first.** The illustrations show fields, huts, a banyan tree, farmers, women with water pots, children and the Sarpanch. They replace the "Deco" hooded-dancer art.
3. **Simple enough for anyone.** One main action per screen, big buttons, big text, icon plus word.
4. **Local language first.** Gujarati by default, with Hindi and English one tap away.
5. **Trustworthy for money.** Clear totals, clear status, a confirmation step before saving.
6. **Same look on every page.** Every page uses the same outline, shadow, radius, colours and header.

## 2. What We Take From the Reference

| Reference element | Our version |
|---|---|
| Top nav with logo, links, pill buttons (Get started / Download) | Logo + Panchayat name, links, language pill, **Login** (filled) and **Check my dues** (outlined) |
| Hero: headline with one word highlighted in a blue box, paragraph, 2 buttons | Headline with "**tax**" highlighted; paragraph; **Login** + **Check my dues** buttons |
| Hero image in a thick-framed rounded box with offset shadow | **Sarpanch + village scene** illustration in the same framed box |
| Floating "sticker" cards over the hero (Quickly design, Safe cloud) | Stickers: "✔ Instant receipt", "🔒 Safe records" |
| "Weekly challenges" card with image | **Sarpanch's message** card with a small village picture |
| 3 numbered steps joined by a dotted line | **How it works:** 1 Tell house number, 2 See your dues, 3 Pay and get receipt |
| 3 feature cards with coloured icon squares | **House Tax**, **Water Tax**, **Receipt**, each with a coloured icon square |
| Image left, text right section with button | Village people illustration + "We keep the village accounts clear" + button |
| FAQ card with pill tabs and accordion | FAQ with tabs: General, Tax, Payment, Receipt |
| Footer with links and pill email input | Footer with office details and a **House no. pill input** ("Check my dues") |

## 3. Colour Tokens

| Token | Hex | Use |
|---|---|---|
| `--bg` | `#FFF8EC` | Page background (warm cream) with a faint diagonal-line pattern like the reference |
| `--paper` | `#FFFFFF` | Cards |
| `--ink` | `#111111` | **All outlines**, shadows, headings, body text |
| `--primary` | `#E8502F` | Primary buttons (terracotta) |
| `--highlight` | `#2F7DE1` | Highlighted headline word, links, active tab text (sky blue, matching the reference) |
| `--leaf` | `#2E9E4F` | Success, "Paid", Save Payment |
| `--turmeric` | `#FFC933` | Icon squares, stickers, active nav pill |
| `--rose` | `#F0508C` | Icon square (Receipt), accent |
| `--danger` | `#D62828` | Pending, defaulter, errors |
| `--muted` | `#4A4A4A` | Helper text (dark enough to read; the reference's light grey is too faint) |
| `--shadow-color` | `#111111` | Hard offset shadow |

Status always has an icon and a word, never colour alone.

## 4. Neubrutalism Core Rules (apply to every component)

```css
:root{
  --border: 3px solid #111;          /* reference uses ~2px; we go thicker for clarity */
  --radius-card: 20px;
  --radius-btn: 14px;
  --radius-pill: 999px;
  --shadow: 6px 6px 0 #111;          /* hard offset, no blur */
  --shadow-sm: 4px 4px 0 #111;
}
.card{ background:#fff; border:var(--border); border-radius:var(--radius-card); box-shadow:var(--shadow); padding:24px; }
.btn { border:var(--border); border-radius:var(--radius-btn); box-shadow:var(--shadow-sm);
       min-height:52px; padding:0 24px; font-size:20px; font-weight:700;
       transition:transform .08s, box-shadow .08s; }
.btn:hover  { transform:translate(-2px,-2px); box-shadow:6px 6px 0 #111; }
.btn:active { transform:translate(4px,4px);  box-shadow:0 0 0 #111; }   /* "pressed" feel */
```
- No blurred shadows and no transparency on cards.
- Every interactive thing has a black outline and a hard shadow.
- Hover lifts, press sinks. Disable movement under `prefers-reduced-motion`.
- Focus ring: extra 3px turmeric outline plus the black border.

## 5. Typography

| Role | Font | Size | Weight |
|---|---|---|---|
| English / numbers | **DM Sans** (reference look) or Poppins | n/a | n/a |
| Gujarati | **Noto Sans Gujarati** | n/a | n/a |
| Hindi | **Noto Sans Devanagari** | n/a | n/a |
| Hero headline | n/a | 44-56px desktop / 34px mobile | 800 |
| H1 | n/a | 32px | 800 |
| H2 | n/a | 26px | 700 |
| Body | n/a | **18px** minimum | 400-500 |
| Buttons | n/a | 20px | 700 |
| Money | DM Sans, tabular figures | 24-36px | 800 |

Bundle fonts locally in `wwwroot/fonts` (works offline). Line height 1.6. Indian number grouping (₹1,25,000).

**Highlighted headline word:** wrap one word in `<mark>` with background `--highlight`, white text, 6px horizontal padding, 8px radius and a 3px black outline, as in the reference ("designers").

## 6. Illustration Style

- Flat vector, thick black outlines (matching the UI) and a limited palette: terracotta, turmeric, leaf green, sky blue, cream, black.
- Replace the reference's hooded figure with:
  1. **Hero:** the Sarpanch (turban, garland or shawl, smiling) in front of a village scene: huts, banyan tree, well, fields, hills, rising sun.
  2. **Village people band:** farmer with plough and bullocks, woman with a water pot, children, villagers sitting under the tree.
  3. **Panchayat Ghar** building.
  4. Warli-style repeating border strip for the receipt and the footer top.
- SVG only (small and sharp). Use the Sarpanch's real photo, with permission, cropped inside the same framed box (Settings lets the admin upload one).
- Suggested sources: Canva or Figma by hand; free sets like unDraw, Storyset, Open Peeps (check licences).
- Use meaningful `alt` text.

## 7. Components

### 7.1 Header / Nav
- White bar, 3px bottom border. Left: emblem plus "ગ્રામ પંચાયત <Village>" (logo style like "Deco."). Centre: nav links. Right: **language pill** (ગુ | हि | EN) and the Login or Logout pill button.
- The active link has a turmeric pill with a black outline.
- Mobile: logo and menu button on top; a **bottom tab bar** inside the app with 4-5 big icons, each with a label and the same outline style.

### 7.2 Buttons
| Type | Look | Used for |
|---|---|---|
| Primary | Terracotta fill, white text, black border and shadow | Login, Search, Next |
| Secondary | White fill, black text, black border and shadow | Check my dues, Print, Back |
| Success | Leaf green fill | Save Payment, Confirm |
| Danger | White fill, red text, black border | Cancel Payment, Deactivate |
| Pill small | Rounded pill (nav, language, tabs) | Language, FAQ tabs |
Icon on the left plus a text label. Full width on mobile.

### 7.3 Cards
White, 3px black border, 20px radius, 6px hard shadow. Variants: stat card, action card, form card, list card.

### 7.4 Sticker Cards (floating on the hero)
Small white cards with a black border and hard shadow, with a small coloured icon and a title (blue) plus a one-line description. Slightly rotated (-3deg and +2deg), overlapping the hero image edge. Hidden on screens under 600px (or stacked below the image).

### 7.5 Icon Square
48px rounded square, 3px black border, flat colour fill (turmeric, rose, sky), white or black icon. Used in feature cards and stat cards.

### 7.6 Numbered Steps
Circle (48px, white, 3px black border) with a number, joined by a dotted vertical line. Title in bold, a one-line description beside it.

### 7.7 Forms
- Label above (18px, bold). Inputs 54px high, 3px black border, 12px radius, white fill; focus adds a turmeric shadow `4px 4px 0 #FFC933`.
- Pill-shaped input with an attached round button for house-number lookup (as in the reference footer newsletter).
- Friendly inline errors in the user's language.
- Mobile number uses the numeric keypad.

### 7.8 Tables and Lists
- Table inside a card; header row in turmeric with a black bottom border; rows separated by 2px black lines (zebra: white and cream).
- Mobile: rows become stacked cards with a hard shadow.
- Amounts: pending in bold red, paid in bold green.

### 7.9 Status Chips
Pill, black border: `✔ Paid` (green fill), `◐ Partial` (turmeric), `⚠ Pending` (red fill, white text), `✖ Cancelled` (grey).

### 7.10 Tabs and Accordion (FAQ)
- Tabs are small pills; the active one has a black border plus a hard shadow.
- Accordion rows show the number (01, 02, 03), a bold question and a chevron; the open one shows an answer in 18px text and the number turns blue, as in the reference.

### 7.11 Toast and Modal
Toast: card style with a coloured left block (green or red) at the top of the screen. Modal: card with a dark overlay (flat `rgba(0,0,0,.5)`), two large buttons.

## 8. Page Designs

### 8.1 Landing Page `/` (follows the reference top to bottom)

```
┌───────────────────────────────────────────────────────────┐
│ (logo) ગ્રામ પંચાયત <Village>   Home Services Help  ગુ|हि|EN  [Login] │
├───────────────────────────────────────────────────────────┤
│ HERO                                                      │
│  Pay your village [tax] the                ┌────────────┐ │
│  easy way                                  │ SARPANCH + │ │
│  Short line: see dues, pay at the          │ VILLAGE    │ │
│  Panchayat, get a receipt.                 │ ILLUSTRATION│ │
│  [Login →] [Check my dues ▾]               │ (framed,   │ │
│  ● Any question? Visit office              │  shadow)   │ │
│        ┌ sticker: ✔ Instant receipt ┐      └────────────┘ │
│        └ sticker: 🔒 Safe records ──┘                      │
├───────────────────────────────────────────────────────────┤
│ ┌ Sarpanch's message ┐        ① Tell your house number     │
│ │ small village pic  │        ② See your dues clearly      │
│ └────────────────────┘        ③ Pay and take your receipt  │
├───────────────────────────────────────────────────────────┤
│ [🏠 House Tax] [💧 Water Tax] [🧾 Receipt]  3 feature cards │
├───────────────────────────────────────────────────────────┤
│ [Village people illustration]   We keep the village        │
│  (farmers, women, children)     accounts clear  [Login →]  │
├───────────────────────────────────────────────────────────┤
│ FAQ card: tabs General | Tax | Payment | Receipt           │
│ 01 How do I know my dues?  02 What is a late fee? 03 ...   │
├───────────────────────────────────────────────────────────┤
│ Footer: logo, office address, timing, phone, links,        │
│ "Check my dues" [ Enter house no. ( → ) ]                  │
│ Warli border strip · © Gram Panchayat <Village>            │
└───────────────────────────────────────────────────────────┘
```
Content (Gujarati first, with English alongside):
- **Headline:** "ગામનો કર, હવે સરળ" / "Village **tax**, made simple"
- **Stickers:** "ફટાફટ પહોંચ / Instant receipt", "સુરક્ષિત રેકોર્ડ / Safe records"
- **Sarpanch card:** name, photo, a 2-line message ("ગામનો વિકાસ, સૌનો સાથ").
- **Feature cards:** House Tax (turmeric icon), Water Tax (sky icon), Receipt (rose icon), each with a one-line explanation.
- **FAQ examples:** How do I know what I owe? What is the late fee? Can I pay in parts? How do I get a lost receipt?
- Check my dues is a small public lookup by house number and mobile (read-only). If it is not built, the button links to "Visit the Panchayat office" info.
- Subtle motion only (stickers float 4px, sun rises); respects reduced motion.

### 8.2 Login `/Account/Login`
Centred card with the hard shadow on the cream pattern background; a small framed Sarpanch avatar on top, title, username and password (show/hide), one big **Login** button, friendly error message.

### 8.3 Dashboard `/Dashboard`
- Greeting: "નમસ્તે, <Name> 🙏" (the name inside a highlighted `<mark>`).
- 4 stat cards with icon squares: Collected Today, Collected This Year, Total Pending, Defaulter Households.
- Collected vs Pending as a thick **progress bar** (black border, green and red fills).
- 4 big action cards: Record Payment, Pending List, Households, Tax Settings (Sarpanch sees 2).
- "Latest payments" list card.

### 8.4 Households `/Households`
Search pill, Ward filter, **+ Add House** button. List cards or a table with pending chip and actions. Add/Edit form in a form card. Detail view: payment history using the numbered-step timeline style.

### 8.5 Record Payment `/Payments/Create`
Three numbered steps on one page (the reference's step style): **1 Find house** (big search), **2 See dues** (table card plus a large total "Total Payable ₹ 1,272"), **3 Receive** (amount, mode as 3 big outlined radio cards: Cash / UPI / Cheque, then the green **Save Payment** button). Confirmation modal before saving, then the Receipt.

### 8.6 Receipt `/Payments/Receipt/{id}`
Printable A5: emblem, Panchayat name, receipt number, date, house and owner, items (tax, principal, late fee), total in numbers and words, mode, clerk's name, signature line, Warli border. Print CSS: black and white friendly, shadows removed, 2px borders. Buttons: Print, Back.

### 8.7 Pending Dues Report `/Reports/Pending`
Filter card (Year, Ward, Tax type, Minimum amount, **Show**). A turmeric summary banner: "Total Pending ₹ X from N houses". Table or cards with tap-to-call mobile numbers. **Print** and **Export CSV**.

### 8.8 Tax Settings `/Settings/TaxTypes`
Tax type cards with an icon square, amount, due date, late fee rule and an Edit button. A highlighted **Generate Dues for <Year>** card with a confirmation showing the house count. Section for the Sarpanch photo and Panchayat details.

## 9. Responsive Behaviour

| Width | Layout |
|---|---|
| Under 600px | One column, bottom tab bar, tables become cards, full-width buttons, hero image below the text, stickers stacked or hidden |
| 600-1024px | Two columns for stats and feature cards |
| Over 1024px | Hero in two columns (text left, framed illustration right), 3 feature cards in a row, 4 stat cards in a row, content max width 1100px |

## 10. Accessibility and Simplicity (adjustments to the reference)

- Reference text is small and light grey. We use **18px minimum and near-black text (contrast 7:1)**.
- Touch targets at least 48x48px; every icon has a text label.
- Keyboard focus rings visible; `lang` attribute set to `gu`, `hi` or `en`.
- Hard shadows don't carry meaning; status uses icon plus text.
- Indian number grouping; dates as DD/MM/YYYY.
- Plain words only ("Pending amount", not "outstanding receivable").
- One primary button per screen; confirmation before saving or cancelling money.
- Print styles remove shadows and keep a black outline.

## 11. Implementation Notes (ASP.NET Core MVC)

- `_Layout.cshtml` (app pages) and `_LandingLayout.cshtml` (landing, login) share `wwwroot/css/theme.css`, which holds the tokens in section 4.
- Use plain CSS, or Bootstrap 5 with these variables overriding its defaults; no heavy front-end framework.
- Partials: `_StatCard`, `_ActionCard`, `_StickerCard`, `_StatusChip`, `_Steps`, `_Faq`, `_Toast`, `_PageHeader`.
- Faint diagonal-line background: a small repeating SVG or CSS `repeating-linear-gradient` at 4% opacity.
- Localization with `IStringLocalizer` and `.resx` (gu, hi, en); the choice is stored in a cookie.
- FAQ accordion with a few lines of vanilla JS (or the HTML `<details>` element for no-JS simplicity).

## 12. Design Checklist (before demo)

- [ ] Landing page has: nav, hero with highlighted word, framed Sarpanch + village illustration, sticker cards, message card, 3 steps, 3 feature cards, village-people section, FAQ, footer
- [ ] Every card, button and input has a thick black outline and a hard shadow
- [ ] All pages share the same header, colours, radius and shadow
- [ ] Text is 18px or larger and high contrast
- [ ] Gujarati, Hindi and English toggle works everywhere
- [ ] Receipt and Pending report print cleanly
- [ ] Core tasks take 3 clicks or fewer