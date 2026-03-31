# Check 4: Synonyms / Linguistic Schema (Max 5 pts)

## What to Evaluate

Synonyms let the AI match user terminology to the correct objects. A user might say "revenue", "income", or "turnover" when they mean `Total Sales`.

### Scoring Rules

| Condition | Score |
|-----------|-------|
| No synonyms on any visible object | 1 |
| Some synonyms but < 50% of measures covered | 3 |
| Good coverage (>= 50% of measures) | 5 |

### Detection via MCP

Check synonyms on tables, columns, and measures:

```
For each visible table:
  - Check if table has synonyms configured (non-empty synonyms list)

For each visible column (not hidden, not RowNumber):
  - Check if column.synonyms is non-empty

For each visible measure (not hidden):
  - Check if measure.synonyms is non-empty
```

Count: `tblWithSyn`, `colWithSyn`, `msrWithSyn`

### Remediation

Add synonyms using MCP or Power BI Desktop:

- **Measures:** `Total Sales` → synonyms: `revenue, income, turnover, sales amount`
- **Tables:** `Customer` → synonyms: `client, account, buyer`
- **Columns:** `Order Date` → synonyms: `purchase date, transaction date, sale date`

### Best Practices

- Focus on key measures first — measures drive most AI queries
- Add synonyms that match how your users naturally talk about the data
- Include both formal and conversational variations
- Don't add synonyms that could create ambiguity between different objects
