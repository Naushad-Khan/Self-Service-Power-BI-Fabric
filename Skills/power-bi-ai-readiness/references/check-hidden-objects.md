# Check 8: Hidden Objects Risk (Max 5 pts)

## What to Evaluate

Hidden columns that lack descriptions create a risk for Verified Answers. If a verified answer references a hidden column, the answer silently fails.

### Scoring Rules

Start at 5 points.

1. Count all hidden columns (excluding system tables)
2. Of those, count hidden columns without descriptions
3. Calculate `pctNoDesc = hiddenNoDesc / totalHidden * 100`
4. If `pctNoDesc > 50`: score = 3, flag warning
5. If `pctNoDesc <= 50`: score = 5 (PASS)
6. No hidden columns at all: score = 5 (PASS)

### Detection via MCP

```
For each column where isHidden = true:
  - Skip if table name starts with DateTableTemplate_ or LocalDateTable_
  - Check if description is empty/whitespace
```

### Why This Matters

- Verified Answers use visual properties (columns, measures, filters) to guide DAX generation
- If a verified answer visual uses a hidden column, the DAX generation tool cannot resolve it
- The answer silently fails — no error, just wrong or missing results
- Users lose trust in the data agent

### Remediation

For hidden columns that are referenced by verified answers or needed for relationships:

```
column_operations(operation: "Update", definitions: [
  {tableName: "Sales", name: "CustomerKey", description: "Foreign key linking to Customer dimension. Hidden from report view.", isHidden: true}
])
```

### Best Practices

- Add descriptions to ALL hidden columns, especially relationship keys
- Before creating verified answers, verify none of the visual's columns are hidden
- If a column must be hidden but is needed for verified answers, either unhide it or restructure the visual
