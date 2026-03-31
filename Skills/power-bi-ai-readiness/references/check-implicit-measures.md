# Check 5: Implicit Measures (Max 10 pts)

## What to Evaluate

Implicit measures (numeric columns with `SummarizeBy != None`) cause unpredictable AI behavior. The AI may aggregate columns in unintended ways.

### Scoring Rules

1. Identify visible numeric columns (Int64, Double, Decimal) where `SummarizeBy != None`
2. Calculate percentage: `implicitCount / totalNumericVisible * 100`
3. `score = max(0, floor(10 - pct / 10))`
4. Zero implicit columns = score 10 (PASS)

### Detection via MCP

```
For each visible column:
  - Check dataType is Int64, Double, or Decimal
  - Check summarizeBy is not "None"
  - If summarizeBy is Sum, Average, Count, Min, Max, etc. → flag as implicit
```

### Why This Matters for AI

- Users ask "what are total sales?" and the AI finds a `Sales Amount` column with `SummarizeBy = Sum`
- Instead of using the explicit `Total Sales` measure, the AI may use the implicit aggregation
- This bypasses business logic, filters, and calculated logic in the measure
- Results are unpredictable and often wrong

### Remediation

Set all numeric columns to `SummarizeBy = None`:

```
column_operations(operation: "Update", definitions: [
  {tableName: "Sales", name: "Amount", summarizeBy: "None"},
  {tableName: "Sales", name: "Quantity", summarizeBy: "None"},
  {tableName: "Sales", name: "Discount", summarizeBy: "None"}
])
```

Then ensure explicit DAX measures exist for every aggregation users need:

```
measure_operations(operation: "Create", definitions: [
  {name: "Total Sales", tableName: "Sales", expression: "SUM(Sales[Amount])", formatString: "$#,##0", description: "Sum of net sales amounts"}
])
```
