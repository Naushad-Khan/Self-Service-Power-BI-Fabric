---
name: power-bi-ai-readiness
description: 'Score and prepare Power BI semantic models for AI (Fabric Data Agent / Copilot). Runs 11 checks covering star schema, naming, descriptions, synonyms, implicit measures, duplicates, date ambiguity, hidden objects, model complexity, Prep for AI config, and best practice analysis. Produces a 0-100 scorecard with AI Ready / Mostly Ready / Needs Improvement / Not Ready rating plus prioritized fix list. Use when the user mentions AI readiness, Prep for AI, data agent preparation, Copilot optimization, semantic model scoring, or making a model AI-ready.'
---

# Semantic Model AI Readiness Analyzer

Score a Power BI semantic model on a 0–100 scale and produce a prioritized action plan to make it AI-ready for Fabric Data Agent and Power BI Copilot.

## When to Use

- User asks to score, audit, or review a model for AI readiness
- User wants to prepare a semantic model for Fabric Data Agent or Copilot
- User mentions "Prep for AI", "AI data schema", "verified answers", or "AI instructions"
- User asks about making a model AI-ready or improving Copilot accuracy

**Trigger phrases:** "AI readiness", "score my model", "prep for AI", "data agent", "Copilot optimization", "AI-ready", "semantic model scoring", "AI data schema"

## Prerequisites

### Required Tools
- **Power BI Modeling MCP Server** — connect to the model and read metadata
  - `connection_operations`, `model_operations`, `table_operations`, `column_operations`, `measure_operations`, `relationship_operations`

### Optional Tools
- **Microsoft Learn MCP Server** — look up latest Prep for AI documentation

## Scoring Framework

| # | Check | Max Pts | What it evaluates |
|---|-------|---------|-------------------|
| 1 | Star Schema & Relationships | 15 | M:M relationships, bidirectional cross-filters, isolated tables |
| 2 | Business-Friendly Naming | 10 | DIM_/FACT_ prefixes, _AMT/_QTY suffixes, all-caps names |
| 3 | Object Descriptions | 15 | % of tables, columns, measures with descriptions |
| 4 | Synonyms | 5 | Synonym coverage on tables, columns, measures |
| 5 | Implicit Measures | 10 | Numeric columns with SummarizeBy != None |
| 6 | Duplicate / Overlapping Measures | 5 | Near-identical measure names after normalization |
| 7 | Ambiguous Date Fields | 5 | Multiple visible date columns without guidance |
| 8 | Hidden Objects Risk | 5 | Hidden columns without descriptions (Verified Answers risk) |
| 9 | Model Complexity / Bloat | 5 | Visible helper measures, high column/measure counts |
| 10 | Prep for AI Configuration | 15 | AI Data Schema, AI Instructions, Verified Answers annotations |
| 11 | Best Practice Analyzer | 10 | String keys, missing format strings, redundant CALCULATE, etc. |
| **Total** | | **100** | |

### Rating Scale

| Score | Rating | Meaning |
|-------|--------|---------|
| 90-100 | AI READY | Well-optimized. Monitor and iterate. |
| 70-89 | MOSTLY READY | Good foundation — address flagged items. |
| 50-69 | NEEDS IMPROVEMENT | Address critical items before production. |
| 0-49 | NOT READY | Significant improvements needed. |

## Workflow

### Step 1: Connect to the Model

```
1. connection_operations(operation: "ListConnections")
2. If none: connection_operations(operation: "ListLocalInstances") or ConnectFabric
3. model_operations(operation: "Get") — confirm model name
```

### Step 2: Gather Model Metadata

Run these in parallel where possible:

```
- table_operations(operation: "List")
- relationship_operations(operation: "List")
- measure_operations(operation: "List")
- column_operations(operation: "List") for each table
```

Filter out system tables: names starting with `DateTableTemplate_` or `LocalDateTable_`.

### Step 3: Run All 11 Checks

For each check, calculate the score and collect issues. See reference files for detailed logic:

| Check | Reference |
|-------|-----------|
| Star Schema & Relationships | [references/check-star-schema.md](references/check-star-schema.md) |
| Business-Friendly Naming | [references/check-naming.md](references/check-naming.md) |
| Object Descriptions | [references/check-descriptions.md](references/check-descriptions.md) |
| Synonyms | [references/check-synonyms.md](references/check-synonyms.md) |
| Implicit Measures | [references/check-implicit-measures.md](references/check-implicit-measures.md) |
| Duplicate Measures | [references/check-duplicate-measures.md](references/check-duplicate-measures.md) |
| Ambiguous Date Fields | [references/check-date-fields.md](references/check-date-fields.md) |
| Hidden Objects Risk | [references/check-hidden-objects.md](references/check-hidden-objects.md) |
| Model Complexity | [references/check-model-complexity.md](references/check-model-complexity.md) |
| Prep for AI Config | [references/check-prep-for-ai.md](references/check-prep-for-ai.md) |
| Best Practice Analyzer | [references/check-bpa.md](references/check-bpa.md) |

### Step 4: Produce the Scorecard

Output format:

```
══════════════════════════════════════════════════════════
  SEMANTIC MODEL AI READINESS SCORECARD
  Model: {model_name}
══════════════════════════════════════════════════════════

  Check                                        Score    Progress
  ────────────────────────────────────────────────────────
  Star Schema Validation                        15/15   [####################] 100% PASS
  Business-Friendly Naming                       8/10   [################....] 80%  PASS
  Object Descriptions                           10/15   [#############.......] 67%  WARN
  ...
  ────────────────────────────────────────────────────────
  TOTAL SCORE                                   72/100  (72%)

  Rating : MOSTLY READY
  Summary: Good foundation — address flagged items to maximize accuracy.

  KEY ISSUES TO ADDRESS:
    - CRITICAL: 45 objects missing descriptions
    - WARNING: No synonyms configured
    - CRITICAL: AI Instructions not detected
```

### Step 5: Generate Prioritized Fix Plan

Classify issues into three tiers:

1. **Critical** (blocks AI accuracy): Missing Prep for AI config, no descriptions, implicit measures
2. **Important** (reduces quality): Technical naming, no synonyms, duplicate measures
3. **Recommended** (polish): Hidden object descriptions, date guidance, helper measures

### Step 6: Offer to Fix Issues

After presenting the scorecard, offer to remediate using MCP tools:

- **Add descriptions**: `column_operations(operation: "Update")`, `measure_operations(operation: "Update")`
- **Rename objects**: `table_operations(operation: "Update")`, `column_operations(operation: "Update")`
- **Hide columns**: `column_operations(operation: "Update", definitions: [{isHidden: true}])`
- **Disable implicit measures**: Set SummarizeBy to None via column update
- **Add synonyms**: Use `column_operations` / `measure_operations` to add synonyms

## Bonus: Measure Dependency Analysis

When configuring AI Data Schema, it is critical to include all dependent objects. For each key measure, trace:
- Direct column references: `Table[Column]`
- Measure-to-measure references: `[Other Measure]`

Output a dependency matrix so the user knows exactly which tables, columns, and measures must be included in the AI Data Schema.

## Key Resources

- [Semantic model best practices for data agent](https://learn.microsoft.com/en-us/fabric/data-science/semantic-model-best-practices)
- [Prep data for AI](https://learn.microsoft.com/en-us/power-bi/create-reports/copilot-prepare-data-ai)
- [AI data schema setup](https://learn.microsoft.com/en-us/power-bi/create-reports/copilot-prepare-data-ai-data-schema)
- [AI instructions](https://learn.microsoft.com/en-us/power-bi/create-reports/copilot-prepare-data-ai-instructions)
- [Verified answers](https://learn.microsoft.com/en-us/power-bi/create-reports/copilot-prepare-data-ai-verified-answers)
- [Tabular Editor BPA Rules](https://github.com/TabularEditor/BestPracticeRules)
- [Power BI Modeling MCP Server](https://github.com/microsoft/powerbi-modeling-mcp)
