# Check 11: Best Practice Analyzer (Max 10 pts)

## What to Evaluate

Run a lightweight set of BPA-style checks against the model using MCP. These catch common DAX and modeling issues that affect AI accuracy.

### Built-in Sub-Rules

| Rule | Severity | What to Check |
|------|----------|---------------|
| String relationship keys | Warning | Relationship columns with DataType = String (should be Int64) |
| Missing format strings | Warning | Visible measures with empty FormatString |
| Redundant CALCULATE | Info | Measures with `CALCULATE(expr)` and no filter arguments |
| IF(ISBLANK()) pattern | Info | Measures using `IF(ISBLANK(...))` instead of `//` (division) operator |
| Calculated columns with RELATED | Warning | Calculated columns referencing other tables via RELATED() |
| Floating-point columns | Info | Visible Double columns (rounding risk — prefer Decimal) |
| Inactive relationships | Info | Relationships where isActive = false without USERELATIONSHIP |
| Boolean as Int64 | Info | Int64 columns named like flags (is_, has_, _flag, _ind) |
| Wide tables | Warning | Tables with 50+ columns (noise for AI) |

### Scoring Rules

Count total BPA issues found:

| Issue Count | Score |
|-------------|-------|
| 0 | 10 |
| 1-4 | 8 |
| 5-9 | 7 |
| 10-19 | 5 |
| 20+ | 2 |

### Detection via MCP

**String relationship keys:**
```
relationships = relationship_operations(operation: "List")
For each relationship, check fromColumn and toColumn dataType = "String"
```

**Missing format strings:**
```
measures = measure_operations(operation: "List")
Flag where formatString is empty/null and !isHidden
```

**Redundant CALCULATE:**
```
For each measure expression:
  Match pattern: CALCULATE( <single-expression> ) with no comma (no filter)
```

**IF(ISBLANK()) pattern:**
```
Match: IF(ISBLANK( in measure expressions
```

**Wide tables:**
```
For each visible table, count columns. Flag if > 50.
```

### Remediation Summary

| Rule | Fix |
|------|-----|
| String keys | Add Int64 surrogate keys to replace string relationship columns |
| Missing format | Add FormatString: `"$#,##0"`, `"0.0%"`, `"#,##0"` |
| Redundant CALCULATE | Remove the CALCULATE wrapper or add proper filters |
| IF(ISBLANK()) | Replace with `DIVIDE()` or `//` operator as appropriate |
| RELATED in calc cols | Convert to a DAX measure instead |
| Double columns | Change data type to Decimal |
| Inactive rels | Ensure measures use USERELATIONSHIP() or remove if unused |
| Bool as Int64 | Change to Boolean data type if values are only 0/1 |
| Wide tables | Hide or remove unnecessary columns |

### Full BPA

For the complete 60+ rule analysis, refer users to:
- Tabular Editor Best Practice Analyzer: `Tools > Best Practice Analyzer`
- Official rules: https://github.com/TabularEditor/BestPracticeRules
- Microsoft BPA rules: https://github.com/microsoft/Analysis-Services/tree/master/BestPracticeRules
