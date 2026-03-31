# Check 7: Ambiguous Date Fields (Max 5 pts)

## What to Evaluate

Multiple visible date columns (Order Date, Ship Date, Due Date) without clear guidance confuse the AI. It cannot determine the default date for time intelligence.

### Scoring Rules

| Condition | Score |
|-----------|-------|
| No visible date columns | 5 |
| Single date column | 5 |
| 2-3 date columns | 4 (low risk, suggest AI Instructions) |
| 4+ date columns | `max(0, 5 - (dateCount - 3))` |

### Detection via MCP

```
For each visible column:
  - Flag if dataType is DateTime
  - Also flag if column name contains "date" (case-insensitive)
```

### Why This Matters for AI

User asks: "What were sales last quarter?"
- With 5 date columns and no guidance, the AI picks one at random
- It might use `Ship Date` (fulfillment perspective) instead of `Order Date` (booking perspective)
- Results will be technically correct but misaligned with business intent

### Remediation

1. **AI Instructions (primary fix):** Add to Prep for AI instructions:
   > "Use Order Date by default for all time-based analysis unless the user specifically asks about shipping or delivery dates."

2. **Descriptions (secondary fix):** Add descriptions to each date column:
   ```
   column_operations(operation: "Update", definitions: [
     {tableName: "Sales", name: "Order Date", description: "Date the order was placed. DEFAULT date for time intelligence."},
     {tableName: "Sales", name: "Ship Date", description: "Date the order was shipped. Use only when user asks about shipping timelines."},
     {tableName: "Sales", name: "Due Date", description: "Expected delivery date. Use only for delivery analysis."}
   ])
   ```

3. **Verified Answers:** Create verified answers for common date-related questions to anchor the correct date column.

4. **Hide if possible:** If a date column is only used for an inactive relationship, hide it.
