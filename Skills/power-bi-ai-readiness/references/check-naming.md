# Check 2: Business-Friendly Naming (Max 10 pts)

## What to Evaluate

Scan all visible table, column, and measure names for technical patterns that hinder AI comprehension.

### Technical Patterns to Flag

| Pattern | Type | Examples |
|---------|------|----------|
| `^(DIM\|FACT\|FCT\|STG\|SRC\|TBL\|VW\|RPT\|TMP\|TEMP\|LKP\|REF\|BRG\|BRIDGE\|MAP\|INT\|SLV\|GLD\|GOLD\|SILVER\|BRONZE\|OWN\|RAW)_` | Database prefix | `DIM_Customer`, `FACT_Sales` |
| `_(DIM\|FACT\|FCT\|TBL\|LKP\|REF\|SK\|NK\|AK\|BK)$` | Database suffix | `Customer_DIM`, `Sales_SK` |
| `_(AMT\|QTY\|CNT\|CT\|NUM\|NBR\|DT\|TS\|FLG\|FLAG\|IND\|CD\|CODE\|KEY\|ID)$` | Column abbreviation | `ORDER_AMT`, `SHIP_DT` |
| `^[A-Z][A-Z0-9_]{2,}$` | All-uppercase name | `CUSTOMER_NAME`, `ORDER_ID` |

### Scoring Rules

Start at 10 points.

1. Count flagged objects across visible tables + columns + measures
2. Calculate flagged percentage: `flaggedCount / totalVisible * 100`
3. `score = max(0, 10 - flaggedPct / 10)`
4. If zero flags: score = 10 (PASS)

### Detection via MCP

```
tables = table_operations(operation: "List")  → filter: !isHidden
columns = column_operations(operation: "List") → filter: !isHidden, not RowNumber
measures = measure_operations(operation: "List") → filter: !isHidden
```

Check each name against the regex patterns above.

### Remediation

Use MCP to rename objects:

```
table_operations(operation: "Update", definitions: [{name: "DIM_Customer", newName: "Customer"}])
column_operations(operation: "Update", definitions: [{tableName: "Sales", name: "ORDER_AMT", newName: "Order Amount"}])
```

**Important:** Renaming can break DAX expressions, relationships, and report visuals. Always review dependencies before renaming.

### Guidelines

- Use human-readable names: `Customer Name` not `CUST_NM`
- Use spaces, not underscores: `Order Date` not `Order_Date`
- Use title case for tables and columns
- Use natural language for measures: `Total Sales`, `Customer Count`
