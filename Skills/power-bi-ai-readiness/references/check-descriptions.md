# Check 3: Object Descriptions (Max 15 pts)

## What to Evaluate

Descriptions are the single most impactful metadata for AI. The DAX generation tool reads descriptions to understand the purpose of every object in the AI data schema.

### Scoring Rules

1. Count visible tables, columns, and measures missing descriptions (empty or whitespace-only)
2. Calculate coverage per category:
   - `tblCoverage = 1 - (tablesNoDesc / totalTables)`
   - `colCoverage = 1 - (colsNoDesc / totalColumns)`
   - `msrCoverage = 1 - (measuresNoDesc / totalMeasures)`
3. Overall coverage = `1 - (totalMissing / totalObjects)`
4. `score = max(0, floor(15 * overallCoverage))`

### Priority Order

Descriptions matter most in this order:
1. **Measures** — highest impact on AI accuracy
2. **Columns** — especially date, key, and dimension columns
3. **Tables** — helps AI understand table purpose and grain

### Detection via MCP

```
For each table in table_operations(operation: "List"):
  - Check table.description is non-empty
  
For each column in column_operations(operation: "List"):
  - Check column.description is non-empty
  
For each measure in measure_operations(operation: "List"):
  - Check measure.description is non-empty
```

### Remediation

Batch update descriptions using MCP:

```
column_operations(operation: "Update", definitions: [
  {tableName: "Sales", name: "OrderDate", description: "Date when the order was placed. Primary date field for time intelligence."},
  {tableName: "Sales", name: "Amount", description: "Net sales amount in USD after discounts."}
])

measure_operations(operation: "Update", definitions: [
  {name: "Total Sales", tableName: "Sales", description: "Sum of all net sales amounts. Use as the default revenue metric."}
])
```

### Writing Effective Descriptions

- Explain the business meaning, not just the technical definition
- For measures: state what it calculates and when to use it
- For columns: specify the grain, format, and any defaults
- For tables: describe what entity it represents and its grain
- Include disambiguation: "Use this measure (not Gross Sales) when users ask about revenue"
