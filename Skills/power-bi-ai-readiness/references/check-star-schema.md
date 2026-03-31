# Check 1: Star Schema & Relationship Structure (Max 15 pts)

## What to Evaluate

Using `relationship_operations(operation: "List")`, inspect every relationship in the model.

### Scoring Rules

Start at 15 points, deduct for issues:

| Issue | Deduction | Cap |
|-------|-----------|-----|
| No relationships at all (flat model) | Set to 0 | — |
| Each many-to-many (M:M) relationship | -2 per | Max -5 |
| Each bidirectional cross-filter | -1 per | Max -3 |
| Isolated tables (no relationships, not a single-table model) | -2 per | Max -4 |

### Detection Logic

**Many-to-Many:**
- Check `fromCardinality` and `toCardinality` — both are "Many"
- DAX accuracy and performance suffer with M:M relationships

**Bidirectional Cross-Filtering:**
- Check `crossFilteringBehavior` = "BothDirections"
- Bidirectional filters introduce ambiguity in AI-generated DAX

**Isolated Tables:**
- Tables with no relationship (from or to) that are not the only table in the model
- Isolated tables cannot be joined, causing query failures

### Remediation

| Issue | Fix |
|-------|-----|
| M:M relationships | Introduce a bridge table or refactor into proper star schema |
| Bidirectional filters | Change to single-direction unless specifically needed for a measure using CROSSFILTER |
| Isolated tables | Add relationships or hide the table if not needed for AI |

### Output Example

```
CHECK 1 — STAR SCHEMA / RELATIONSHIP STRUCTURE (max 15 pts)
  Relationships found: 8
  PASSED: Relationship structure is consistent with star schema design.
  Score: [####################] 15/15 (100%) PASS
```
