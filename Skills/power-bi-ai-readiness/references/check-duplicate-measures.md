# Check 6: Duplicate / Overlapping Measures (Max 5 pts)

## What to Evaluate

Multiple measures calculating similar metrics (e.g., `Total Sales`, `Sales Amount`, `Revenue`) create ambiguity. The AI cannot determine which one to use.

### Scoring Rules

1. Normalize all visible measure names: lowercase, remove all non-alphanumeric characters
2. Group normalized names — pairs with identical normalized form are near-duplicates
3. `score = max(0, 5 - nearDupeCount)`

### Detection Logic

```python
# Pseudocode
for each visible measure name:
    key = lowercase(remove_non_alphanumeric(name))
    group_by_key[key].append(name)

near_dupes = [group for group in groups if len(group) > 1]
```

Examples of near-duplicates:
- `Total Sales` vs `TotalSales` vs `Total_Sales`
- `Revenue` vs `Revenue ` (trailing space)
- `Customer Count` vs `CustomerCount`

### Remediation

1. Identify which measure is the "canonical" version
2. Remove or hide the duplicates
3. Update AI Data Schema to include only the canonical measure
4. Add a description to the canonical measure explaining when to use it

```
measure_operations(operation: "Update", definitions: [
  {name: "TotalSales", tableName: "Sales", isHidden: true}
])
```

### Best Practices

- One measure per business metric
- Clear, distinct names that don't overlap
- Hide intermediate/helper measures
- Document the canonical measure: "Use this measure for all revenue queries"
