# Check 9: Model Complexity / Bloat (Max 5 pts)

## What to Evaluate

Large, bloated models create noise for the AI. Too many visible objects reduce accuracy because the DAX generation tool has more candidates to choose from.

### Scoring Rules

Start at 5 points, deduct for issues:

| Issue | Deduction |
|-------|-----------|
| Visible helper/intermediate measures (name matches: helper, aux, temp, tmp, working, intermediate) | -2 |
| More than 500 visible columns | -1 |
| More than 150 visible measures | -1 |

Minimum score: 0.

### Detection via MCP

```
visibleTables = table_operations(operation: "List") → filter !isHidden, exclude system tables
visibleColumns = column_operations(operation: "List") → filter !isHidden, exclude system tables
visibleMeasures = measure_operations(operation: "List") → filter !isHidden, exclude system tables
```

Check measure names for helper patterns:
- `helper`, `aux`, `auxiliary`, `temp`, `tmp`, `working`, `intermediate` (case-insensitive, word boundary match)

### Why This Matters

- AI Data Schema selects from visible objects — more noise = lower accuracy
- Helper measures are meant for intermediate calculations, not user queries
- High column counts mean the AI must parse more metadata, increasing latency
- The DAX generation tool works best with a focused, curated object set

### Remediation

1. **Hide helper measures:**
   ```
   measure_operations(operation: "Update", definitions: [
     {name: "Helper - Running Total", tableName: "Sales", isHidden: true}
   ])
   ```

2. **Hide unnecessary columns:**
   - Technical keys (surrogate keys, GUIDs)
   - Staging/ETL columns not needed for analysis
   - Duplicate information columns

3. **Use AI Data Schema:** Even if you can't hide objects, configure Prep for AI > Simplify Data Schema to include only the subset users need.

4. **Consider splitting:** If the model serves multiple domains, consider whether separate models would serve AI better.
