# MyCustomVisual — KPI Comparison Card

## Power BI Custom Visual Documentation

**Last Updated:** March 15, 2026  
**Author:** David Kofod Hanna  
**Visual GUID:** `LightningVisualDavidHanna`

---

## Table of Contents

1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Tech Stack & Dependencies](#tech-stack--dependencies)
4. [Setup & Prerequisites](#setup--prerequisites)
5. [Building & Packaging](#building--packaging)
6. [Importing into Power BI Desktop](#importing-into-power-bi-desktop)
7. [Data Roles (Field Wells)](#data-roles-field-wells)
8. [Features](#features)
9. [Format Pane Options](#format-pane-options)
10. [Architecture & Code Map](#architecture--code-map)
11. [Key Implementation Details](#key-implementation-details)
12. [Known Issues & Troubleshooting](#known-issues--troubleshooting)
13. [Common Errors & Fixes](#common-errors--fixes)
14. [How to Add a New Format Option](#how-to-add-a-new-format-option)
15. [How to Add a New Data Role](#how-to-add-a-new-data-role)
16. [Revision History](#revision-history)
17. [Submission Testing Checklist](#submission-testing-checklist)

---

## Overview

This is a **KPI Comparison Card** custom visual for Power BI. It displays:

- A large actual value with optional target comparison
- Delta indicator (▲/▼) with numeric and percentage difference
- Trend chart (bar, line, or area) with optional highlights
- CAGR bracket annotation connecting first and last data points
- Stephen Few-style action dots for outlier detection
- Dimension split: multiple KPI tiles in a configurable grid
- Cross-filtering: click chart data points to filter other visuals
- Drill-through: right-click context menu for drill-through navigation
- Native Power BI tooltips on chart data points
- Configurable date formatting with ordinal date period display

---

## Project Structure

```
MyCustomVisual/
├── assets/
│   └── icon.png                  # Visual icon (20x20 px)
├── dist/                         # Built .pbiviz package output
├── src/
│   ├── settings.ts               # Format pane settings model (7 cards)
│   └── visual.ts                 # Main visual class (~770 lines)
├── style/
│   └── visual.less               # LESS stylesheet
├── capabilities.json             # Data roles, objects, dataViewMappings
├── eslint.config.mjs             # ESLint config
├── package.json                  # npm dependencies
├── pbiviz.json                   # Visual metadata (name, GUID, author)
└── tsconfig.json                 # TypeScript compiler config
```

---

## Tech Stack & Dependencies

| Component | Version | Purpose |
|-----------|---------|---------|
| `powerbi-visuals-api` | ~5.3.0 | Power BI Visuals API |
| `powerbi-visuals-utils-formattingmodel` | 6.0.4 | FormattingSettingsService for format pane |
| `powerbi-visuals-tools (pbiviz)` | 7.0.2 | CLI for building/packaging |
| `typescript` | 5.5.4 | TypeScript compiler |
| `eslint` | ^9.11.1 | Linting |

**Rendering approach:** Vanilla DOM + inline SVG (no d3, no React, no frameworks).

---

## Setup & Prerequisites

### 1. Install Node.js
Requires Node.js (v18+ recommended). Download from https://nodejs.org/

### 2. Install pbiviz tools globally
```powershell
npm install -g powerbi-visuals-tools
```

### 3. Install project dependencies
```powershell
cd <path-to-project>\MyCustomVisual
npm install
```

### 4. SSL Certificate (required for `pbiviz start` dev server)

Power BI dev server requires an SSL certificate. Generate one:

```powershell
$certFolder = Join-Path $env:USERPROFILE "pbiviz-certs"
if (!(Test-Path $certFolder)) { New-Item -ItemType Directory -Path $certFolder -Force }
$passphrase = (Get-Random -Maximum 999999999).ToString()
$pfxPath = Join-Path $certFolder "PowerBICustomVisualTest_public.pfx"
$passphrasePath = Join-Path $certFolder "PowerBICustomVisualTestPass.txt"

$cert = New-SelfSignedCertificate `
    -DnsName localhost `
    -Type Custom `
    -Subject 'CN=localhost' `
    -KeyAlgorithm RSA -KeyLength 2048 `
    -KeyExportPolicy Exportable `
    -CertStoreLocation Cert:\CurrentUser\My `
    -NotAfter (Get-Date).AddDays(365)

Export-PfxCertificate -Cert ("Cert:\CurrentUser\My\" + $cert.Thumbprint) `
    -FilePath $pfxPath `
    -Password (ConvertTo-SecureString -String $passphrase -Force -AsPlainText)

Set-Content -Path $passphrasePath -Value $passphrase -NoNewline
Write-Output "Certificate created at $pfxPath"
Write-Output "Passphrase saved to $passphrasePath"
```

Certificate files are saved in `%USERPROFILE%\pbiviz-certs\`.

### 5. Trust the certificate
In Windows, double-click the `.pfx` file → install to **Current User** → **Trusted Root Certification Authorities**.

---

## Building & Packaging

### Package for import (production build)
```powershell
cd <path-to-project>\MyCustomVisual
npx pbiviz package
```

Output: `dist/myCustomVisual.77221C68E9BC4BFCBCD9C9ECEAEFF777.pbiviz`

### Development server (live reload in Power BI Service)
```powershell
npx pbiviz start
```
Then enable the developer visual in Power BI Service → Settings → Developer → Enable developer visual.

**Note:** `pbiviz start` requires `pwsh` (PowerShell 7+) on some systems. If it fails with "pwsh not found", install PowerShell 7: `winget install Microsoft.PowerShell`.

---

## Importing into Power BI Desktop

1. Run `npx pbiviz package`
2. Open Power BI Desktop → create or open a report
3. In the **Visualizations** pane → click **... (three dots)** → **Import a visual from a file**
4. Browse to the `dist/` folder in your project directory
5. Select the `.pbiviz` file → click **Open** → confirm the import dialog
6. The visual icon appears in the Visualizations pane
7. Click it → drag fields into the data wells:
   - **Actual Value**: your main measure (e.g., Revenue)
   - **Target Value**: your target/budget measure
   - **Date / Category Axis**: date or category column
   - **Dimension Split**: a dimension to split into grid tiles (e.g., Region)

---

## Data Roles (Field Wells)

Defined in `capabilities.json` under `dataRoles`:

| Role | Display Name | Kind | Max | Description |
|------|-------------|------|-----|-------------|
| `actual` | Actual Value | Measure | 1 | Primary measure (required) |
| `target` | Target Value | Measure | 1 | Target/budget measure (optional) |
| `category` | Date / Category Axis | Grouping | 1 | X-axis for the trend chart (optional) |
| `dimension` | Dimension Split | Grouping | 1 | Splits into multiple KPI tiles (optional) |

### Data View Mapping
- **Categories:** Uses `select` pattern for both `category` and `dimension` groupings
- **Values:** Uses `bind` pattern for `actual` and `target` measures
- **Data Reduction:** `top: 30000` rows maximum
- **Drilldown:** Enabled on `category` role

### Capabilities Flags
- `supportsHighlight: true` — enables cross-highlighting from other visuals
- `supportsMultiVisualSelection: true` — enables slicer/filter interop with other visuals

---

## Features

### KPI Display
- **Actual value** with configurable color and smart number formatting (K/M/B suffixes)
- **Target comparison** with vs label
- **Delta indicator** (▲ positive / ▼ negative) with configurable colors (blue/orange by default)
- **Numeric difference** with +/- sign
- **Percentage difference** with configurable decimal places
- **Date period label** in ordinal format ("1st Jan 2023 to 31st Dec 2023")
- **KPI alignment** (left/center/right)

### Trend Chart
- **Chart types:** Column Bar, Area Chart, Line Chart
- **Highlights:** None, First & Last, Min & Max (with pill labels)
- **Configurable colors** for chart fill and highlights
- **CAGR bracket:** U-shaped bracket from first to last bar with filled dark pill label showing % change
- **Action dots (Stephen Few):** Red dots below chart for periods where actual is below target by configurable threshold %
- **Action dot legend:** Positioned via format option (Below Delta, Top Left/Right, Bottom Left/Right)

### Dimension Split
- When a field is placed in **Dimension Split**, the visual creates a grid of KPI tiles
- Each tile shows the KPI for one dimension value (e.g., per region)
- **Grid layout:** Configurable columns and rows (0 = auto-calculate)
- Auto-layout: columns = `min(tileCount, floor(width / 220px))`

### Interactivity
- **Cross-filtering:** Click any chart data point to filter other visuals on the page
- **Multi-select:** Ctrl+Click to select multiple points
- **Drill-through:** Right-click any data point for Power BI drill-through context menu
- **Tooltips:** Native Power BI tooltips showing Period, Actual, Target, Difference, % Difference, CAGR
- **Clear selection:** Click empty area to clear cross-filter

### Date Formatting
- 8 date format options: Auto, Year, Mon Year, Month, Day/Mon, DD/MM/YYYY, MM/DD/YYYY, YYYY-MM-DD
- Ordinal date period label (e.g., "1st Jan 2023 to 31st Dec 2023")

---

## Format Pane Options

All options are defined in `src/settings.ts` and registered in `capabilities.json`.

### Labels Card (`labels`)
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `actualLabel` | TextInput | "Actual" | Label shown above the actual value |
| `targetLabel` | TextInput | "Target" | Label used in the "vs Target" line |

### Colors Card (`colors`)
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `positiveColor` | ColorPicker | `#3498db` (blue) | Color for positive delta |
| `negativeColor` | ColorPicker | `#e67e22` (orange) | Color for negative delta |
| `actualColor` | ColorPicker | `#4a4a4a` (dark grey) | Color for the actual value number |

### Display Options Card (`display`)
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `showTarget` | Toggle | true | Show "vs Target: X" line |
| `showDifference` | Toggle | true | Show numeric difference |
| `showPercentage` | Toggle | true | Show percentage difference |
| `decimalPlaces` | NumUpDown | 1 | Decimal places for all numbers |
| `alignment` | Dropdown | Center | KPI text alignment (Left/Center/Right) |

### Trend Chart Card (`chart`)
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `showChart` | Toggle | true | Show/hide the trend chart |
| `chartType` | Dropdown | Column Bar | Bar, Area, or Line chart |
| `highlight` | Dropdown | None | None, First & Last, Min & Max |
| `chartColor` | ColorPicker | `#a0c4e8` | Chart bar/line/area color |
| `highlightColor` | ColorPicker | `#2980b9` | Highlight data point color |
| `showCAGR` | Toggle | false | Show CAGR bracket annotation |
| `showActionDots` | Toggle | false | Show Stephen Few action dots |
| `actionDotColor` | ColorPicker | `#e74c3c` (red) | Action dot color |
| `actionDotThreshold` | NumUpDown | 10 | Threshold % below target to flag |
| `legendPosition` | Dropdown | Below Delta | Action dot legend position |

### Date Format Card (`dateFormat`)
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `dateFormatType` | Dropdown | Auto | Date format for axis labels |
| `showDatePeriod` | Toggle | true | Show date range below actual value |

### Grid Layout Card (`gridLayout`)
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `gridColumns` | NumUpDown | 0 | Grid columns (0 = auto) |
| `gridRows` | NumUpDown | 0 | Grid rows (0 = auto) |

---

## Architecture & Code Map

### settings.ts (~270 lines)

Defines 6 formatting card classes using `powerbi-visuals-utils-formattingmodel`:

```
LabelsCardSettings     → "labels" object
ColorsCardSettings     → "colors" object
DisplayCardSettings    → "display" object
ChartCardSettings      → "chart" object
DateFormatCardSettings → "dateFormat" object
GridLayoutCardSettings → "gridLayout" object

VisualFormattingSettingsModel → aggregates all cards
```

**Pattern for each card:**
1. Define properties as `formattingSettings.XXX` instances (ToggleSwitch, NumUpDown, ColorPicker, TextInput, ItemDropdown)
2. Set `name` (must match the object name in capabilities.json)
3. Set `displayName` (shown in format pane)
4. Set `slices` array with all properties

### visual.ts (~770 lines)

**Class: `Visual implements IVisual`**

#### Class Fields
| Field | Type | Purpose |
|-------|------|---------|
| `host` | IVisualHost | Power BI host services |
| `container` | HTMLElement | Main div.kpi-comparison-card |
| `tooltipService` | ITooltipService | Native tooltip binding |
| `selectionManager` | ISelectionManager | Cross-filtering & drill-through |
| `visualSettings` | VisualFormattingSettingsModel | Current settings |
| `formattingSettingsService` | FormattingSettingsService | Settings parser |
| `chartDataMap` | Map<string, {...}> | Per-tile chart data for tooltips |
| `boundContainerClick` | function \| null | Stored click handler ref (cleanup) |
| `boundContainerContext` | function \| null | Stored contextmenu handler ref (cleanup) |

#### Method Map

| Method | Lines | Purpose |
|--------|-------|---------|
| `constructor()` | ~10 | Creates container, init services |
| `ordinal(n)` | ~4 | Returns "1st", "2nd", "3rd", etc. |
| `formatDateLabel(raw, fmt)` | ~15 | Formats date string per selected format |
| `formatDateOrdinal(raw)` | ~5 | Formats date as "1st Jan 2023" |
| `calcCAGR(values)` | ~5 | % change from first to last value |
| `detectActionDotIndices(actual, target, threshold)` | ~12 | Finds indices below threshold |
| `update(options)` | ~90 | **Main entry point** — reads dataView, builds HTML, binds events |
| `renderKpiTile(...)` | ~120 | Renders one KPI tile (KPI section + chart) |
| `buildTooltipItems(tileId, idx)` | ~30 | Builds tooltip data array for one data point |
| `bindTooltips()` | ~30 | Binds mouseover/move/out to tooltip service |
| `bindSelection(cat, dim)` | ~25 | Binds click for cross-filtering |
| `bindContextMenu(cat, dim)` | ~25 | Binds right-click for drill-through |
| `renderChart(...)` | ~160 | Renders SVG chart with all annotations |
| `formatCompact(val, dec)` | ~4 | Formats number with K/M/B suffix |
| `getFormattingModel()` | ~2 | Returns formatting model for format pane |
| `escapeHtml(str)` | ~3 | XSS-safe HTML escaping |

#### Data Flow

```
update(options)
  ├── populateFormattingSettingsModel() → read settings from dataView
  ├── Extract categorical data (categories, values, dimension by role)
  ├── Apply date formatting
  ├── If dimension split:
  │     ├── Group rows by dimension value
  │     ├── Calculate grid cols/rows (from settings or auto)
  │     └── For each dimension group → renderKpiTile()
  ├── Else: renderKpiTile() for single KPI
  ├── Set container.innerHTML
  ├── Apply responsive padding
  ├── bindTooltips()
  ├── bindSelection()
  └── bindContextMenu()

renderKpiTile(dimensionName, actual[], target[], labels[], ...)
  ├── Sum actual/target → compute delta, % diff
  ├── Build KPI section HTML (label, actual, date period, target, diff)
  ├── Compute action dot legend HTML
  ├── Place legend per legendPosition setting
  ├── If hasChart → store data in chartDataMap → renderChart()
  └── Return HTML string

renderChart(values, targetValues, labels, width, height, settings, ...)
  ├── Calculate margins, SVG dimensions, scales
  ├── CAGR bracket: vertical lines + horizontal connector + pill label
  ├── Chart data: bars (rect) or line/area (path) + circles
  ├── Action dots: circles below chart
  ├── Highlight labels: pill + text above highlighted points
  ├── X-axis labels: thinned tick labels
  └── Return <svg>...</svg> string
```

### visual.less (~100 lines)

Core layout styles for `.kpi-comparison-card` container and its children. Uses flexbox for KPI section layout and chart section. Chart data points have hover transitions.

### capabilities.json (~230 lines)

- 4 data roles (actual, target, category, dimension)
- 6 object groups matching settings.ts cards
- Categorical dataViewMappings with select pattern for categories
- `supportsHighlight: true` and `supportsMultiVisualSelection: true`
- `drilldown.roles: ["category"]`
- Data reduction: top 30,000 rows

---

## Key Implementation Details

### Cross-Filtering with Slicers
The visual receives filtered data automatically from Power BI when external slicers are active. The `supportsMultiVisualSelection: true` capability flag is critical for this. The visual reads whatever `dataView` Power BI provides — it does not need to manually filter.

**Important:** Event listeners for click and contextmenu are stored as references (`boundContainerClick`, `boundContainerContext`) and removed before rebinding on each `update()` call. This prevents listener accumulation that breaks filtering behavior.

### Selection IDs
Selection IDs are built using `host.createSelectionIdBuilder().withCategory(selectionCategory, origIdx)` where `origIdx` is the original row index in the dataView (stored as `data-orig-index` attribute on SVG chart elements).

### Dimension Split Grid
- Uses flex-wrap layout: `display:flex;flex-wrap:wrap`
- Grid columns: user setting or `min(tileCount, floor(width/220))`
- Grid rows: user setting or `ceil(tileCount / cols)`
- Each tile is a self-contained KPI with its own chart
- Tile borders: 1px solid `#eee` on right and bottom

### SVG Chart Rendering
- SVGs use `viewBox` attribute for crisp rendering at any size
- Dimensions: `Math.round(containerWidth)` × `Math.round(containerHeight)`
- All chart elements use `class="chart-data-point"` with `data-tile`, `data-index`, `data-orig-index` attributes for tooltip/selection binding

### Number Formatting
Compact format with suffix: K (thousands), M (millions), B (billions). Controlled by `decimalPlaces` setting (0–10, default 1).

### CAGR Calculation
Simple percentage change between first and last values:
```
CAGR = ((last - first) / |first|) × 100
```
**Note:** This is labeled "CAGR" but is actually a simple % change, not compound annual growth rate. The name was kept for UI consistency.

---

## Known Issues & Troubleshooting

### `pbiviz start` exits with code 1
- May require `pwsh` (PowerShell 7). Install with: `winget install Microsoft.PowerShell`
- May have port conflicts (default 8080). Kill any process on that port: `Stop-Process -Id (Get-NetTCPConnection -LocalPort 8080).OwningProcess -Force`
- Check SSL certificate validity. Regenerate if expired.

### Linter error: `no-inner-outer-html`
This is a known lint warning from `eslint-plugin-powerbi-visuals` about `container.innerHTML = html`. It's a security best-practice warning. The visual uses `escapeHtml()` for all user-provided strings. This warning does NOT block packaging.

### Slicer filtering not working
Fixed by:
1. Adding `supportsMultiVisualSelection: true` in capabilities.json
2. Removing event listener accumulation (storing + removing refs in bindSelection/bindContextMenu)

### Package metadata errors
If `npx pbiviz package` shows "Author name is not specified" etc., fill in these fields in `pbiviz.json`:
- `author.name`
- `author.email`
- `visual.description`
- `visual.supportUrl`

These are currently populated and should not cause issues.

### Visual not appearing after import
- Ensure the `.pbiviz` file was generated in `dist/`
- Check Power BI Desktop version supports API 5.3.0
- Re-import the visual (remove old one first if updating)

---

## Common Errors & Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| `pwsh: not found` | pbiviz start requires PowerShell 7 | `winget install Microsoft.PowerShell` |
| `EADDRINUSE 8080` | Port already in use | Kill process on port 8080 |
| `Certificate is not valid` | Expired or missing SSL cert | Regenerate using the cert script above |
| `Author name is not specified` | Empty fields in pbiviz.json | Fill in author.name, email, description, supportUrl |
| `no-inner-outer-html` lint error | eslint security rule | Benign; doesn't block build |
| Visual doesn't filter with slicers | Missing capability flag or event leaks | Ensure `supportsMultiVisualSelection: true` in capabilities.json |
| Blurry SVG in dimension split | Non-integer SVG dimensions | Use `Math.round()` for width/height + add `viewBox` attribute |
| Format option not appearing | Missing object in capabilities.json | Every property in settings.ts needs a matching object.property in capabilities.json |
| `Cannot read properties of undefined (reading 'value')` | Setting not populated | Check that object name in settings.ts matches capabilities.json exactly |

---

## How to Add a New Format Option

**Example: Adding a "Show Border" toggle to the Display card.**

### Step 1: capabilities.json
Add the property to the matching object:
```json
"display": {
    "properties": {
        // ... existing properties ...
        "showBorder": {
            "type": { "bool": true }
        }
    }
}
```

### Step 2: settings.ts
Add to the `DisplayCardSettings` class:
```typescript
showBorder = new formattingSettings.ToggleSwitch({
    name: "showBorder",       // must match capabilities.json property name
    displayName: "Show Border",
    value: false
});
```
Then add `this.showBorder` to the `slices` array.

### Step 3: visual.ts
Read the setting in your rendering code:
```typescript
const showBorder = settings.displayCard.showBorder.value;
```

### Setting Types Available
| Type | Class | capabilities.json type |
|------|-------|----------------------|
| Toggle | `formattingSettings.ToggleSwitch` | `{ "bool": true }` |
| Number | `formattingSettings.NumUpDown` | `{ "numeric": true }` |
| Text | `formattingSettings.TextInput` | `{ "text": true }` |
| Color | `formattingSettings.ColorPicker` | `{ "fill": { "solid": { "color": true } } }` |
| Dropdown | `formattingSettings.ItemDropdown` | `{ "enumeration": [...] }` |

---

## How to Add a New Data Role

**Example: Adding a "Tooltip Measure" role.**

### Step 1: capabilities.json
Add to `dataRoles`:
```json
{
    "displayName": "Tooltip Measure",
    "name": "tooltipMeasure",
    "kind": "Measure"
}
```

Add to `conditions` in `dataViewMappings`:
```json
"tooltipMeasure": { "max": 1 }
```

Add to `values.select`:
```json
{ "bind": { "to": "tooltipMeasure" } }
```

### Step 2: visual.ts
Read it in `update()`:
```typescript
if (dataView.categorical.values) {
    for (const valueColumn of dataView.categorical.values) {
        if (valueColumn.source.roles["tooltipMeasure"]) {
            // use valueColumn.values
        }
    }
}
```

---

## Revision History

| Round | Changes |
|-------|---------|
| 1 | Initial KPI card replacing default circle card template |
| 2 | Added colors (blue positive, orange negative, grey neutral) |
| 3 | Added date/category axis, chart type selector |
| 4 | Added highlight options (First & Last, Min & Max), SVG title tooltips |
| 5 | Switched to Power BI native ITooltipService, improved label visibility |
| 6 | Added CAGR calculation, Stephen Few action dots, date format options |
| 7 | Fixed CAGR to simple % diff, redesigned action dots below chart, enriched tooltips |
| 8 | Added dimension split, cross-filtering (ISelectionManager), drill-through (context menu) |
| 9 | Redesigned CAGR to U-bracket style with filled pill, added responsive padding |
| 10 | Added dark/light mode toggle (ThemeCardSettings) |
| 11 | Removed dark mode, fixed slicer filtering (event listener cleanup + supportsMultiVisualSelection), moved action dot legend to HTML with position option, fixed SVG pixel quality (viewBox + Math.round), added grid layout columns/rows settings |

| 12 | Removed unused d3 dependency, added null/NaN data guards, added rendering events (IVisualEventService), high contrast detection, SVG accessibility (role/aria-label), keyboard focus support, landing page & tooltip capability flags, removed duplicate formatNumber, fixed hardcoded paths in docs, added min-width/height to container |
| 13 | Added branded landing page with author credit when no measures are added, renamed visual GUID to `LightningVisualDavidHanna` |

---

## Submission Testing Checklist

Before publishing to AppSource, the visual must pass the general test cases documented at:
**[Test a Power BI custom visual before submitting it for publication](https://learn.microsoft.com/en-us/power-bi/developer/visuals/submission-testing#general-test-cases)**

### General Test Cases

| # | Test | Expected Result | Status |
|---|------|-----------------|--------|
| 1 | Create a Stacked column chart with Category and Value. Convert it to this visual, then back to column chart. | No errors after conversions. | |
| 2 | Create a Gauge with three measures. Convert it to this visual, then back to Gauge. | No errors after conversions. | |
| 3 | Make selections in the visual (click chart data points). | Other visuals on the page reflect the selections (cross-filtering). | |
| 4 | Select elements in other visuals. | This visual shows filtered data according to selection in other visuals. | |
| 5 | Check min/max dataViewMapping conditions. | Field buckets accept correct number of fields (1 Actual, 1 Target, 1 Category, 1 Dimension). | |
| 6 | Remove all fields in different orders. | Visual cleans up properly (shows landing page). No console errors. | |
| 7 | Open the Format pane with each possible bucket configuration. | No null reference exceptions. | |
| 8 | Filter data using the Filter pane at visual, page, and report level. | Tooltips are correct after applying filters and show filtered values. | |
| 9 | Filter data using a Slicer. | Tooltips are correct after applying filters and show filtered values. | |
| 10 | Filter data using a published visual (e.g., select a pie slice or column). | Tooltips are correct after applying filters and show filtered values. | |
| 11 | If cross-filtering is supported, verify filters work correctly. | Applied selection filters other visuals on the page. | |
| 12 | Select with Ctrl, Alt, and Shift keys. | No unexpected behaviors. | |
| 13 | Change View Mode to Actual size, Fit to page, and Fit to width. | Mouse coordinates are accurate. | |
| 14 | Resize the visual. | Visual reacts correctly to resizing. | |
| 15 | Set the report size to the minimum. | No display errors. | |
| 16 | Ensure scroll bars work correctly (dimension split with many tiles). | Scroll bars exist if necessary and are properly sized. | |
| 17 | Pin the visual to a Dashboard. | Visual displays properly. | |
| 18 | Add multiple versions of the visual to a single report page. | All versions display and operate properly. | |
| 19 | Add multiple versions of the visual to multiple report pages. | All versions display and operate properly. | |
| 20 | Switch between report pages. | Visual displays correctly. | |
| 21 | Test Reading view and Edit view. | All functions work correctly. | |
| 22 | Open the Format pane. Turn properties on and off, enter custom text, stress options, input bad data. | Visual responds correctly. | |
| 23 | Save the report and reopen it. | All property settings persist. | |
| 24 | Switch pages and switch back. | All property settings persist. | |
| 25 | Test all chart types (Bar, Area, Line) and highlight modes. | All displays and features work correctly. | |
| 26 | Test all numeric, date, and character data types. | All data is formatted properly. | |
| 27 | Review formatting of tooltip values, axis labels, data labels. | All elements are formatted correctly. | |
| 28 | Switch automatic formatting on and off for numeric values in tooltips. | Tooltips display values correctly. | |
| 29 | Test with different data volumes: thousands of rows, one row, two rows. | All displays and features work correctly. | |
| 30 | Provide bad data: null, infinity, negative values, wrong value types. | Visual handles gracefully (no crashes). | |

### Optional Browser Testing

| Browser | Expected Result |
|---------|-----------------|
| Google Chrome (latest) | All displays and features work correctly. |
| Microsoft Edge (latest) | All displays and features work correctly. |
| Mozilla Firefox (latest) | All displays and features work correctly. |
| Safari (macOS, latest) | All displays and features work correctly. |

### Desktop Testing

| Test | Expected Result |
|------|-----------------|
| Test all features in Power BI Desktop. | All displays and features work correctly. |
| Import, save, open a file, and publish to Power BI Service. | All displays and features work correctly. |
| Change numeric format string (0 decimals, 3 decimals). | Visual displays correctly. |

### Performance Testing

| Test | Expected Result |
|------|-----------------|
| Create the visual with many data points / dimension tiles. | Visual performs well, no freezing. No performance issues with animation, resizing, filtering, selecting. |

---

## Quick Reference: File-to-Feature Map

| Feature | Files Changed |
|---------|--------------|
| Add format option | `capabilities.json` + `settings.ts` + `visual.ts` |
| Add data role | `capabilities.json` + `visual.ts` |
| Change KPI layout | `visual.ts` (renderKpiTile) + `visual.less` |
| Change chart rendering | `visual.ts` (renderChart) |
| Change tooltip content | `visual.ts` (buildTooltipItems) |
| Change cross-filter behavior | `visual.ts` (bindSelection) |
| Change grid layout | `visual.ts` (update, dimension split section) |
| Change visual metadata | `pbiviz.json` |
| Change styling | `style/visual.less` + inline styles in `visual.ts` |

---

## Full File State Summary (as of March 15, 2026)

- **capabilities.json:** ~248 lines — 4 data roles, 6 objects, categorical dataViewMappings with select, supportsHighlight + supportsMultiVisualSelection + supportsKeyboardFocus + supportsLandingPage + tooltips
- **src/settings.ts:** ~270 lines — 6 card classes + VisualFormattingSettingsModel
- **src/visual.ts:** ~920 lines — Visual class with 15 methods, vanilla DOM + SVG rendering, rendering events, high contrast support
- **style/visual.less:** ~103 lines — flexbox layout, hover transitions, min-size constraintr info filled in
- **package.json:** Dependencies: powerbi-visuals-api ~5.3.0, formattingmodel 6.0.4
