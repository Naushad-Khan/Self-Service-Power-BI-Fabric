# Check 10: Prep for AI Configuration (Max 15 pts)

## What to Evaluate

This is the most AI-specific check. The three Prep for AI components directly control how Fabric Data Agent and Copilot interpret the semantic model.

### Three Components

| Component | Annotation Keys | Points |
|-----------|----------------|--------|
| AI Data Schema | `PBI_AIDataSchema`, `AI_SCHEMA`, `AISCHEMA`, `DATA_AGENT_SCHEMA`, `PREP_AI_SCHEMA`, `COPILOT_SCHEMA`, `PBI_AI_SCHEMA` | 5 |
| AI Instructions | `PBI_AIInstructions`, `AI_INSTRUCTION`, `AIINSTRUCTION`, `PREP_AI_INSTRUCTION`, `COPILOT_INSTRUCTION`, `PBI_AI_INSTRUCTION` | 5 |
| Verified Answers | `PBI_VerifiedAnswers`, `VERIFIED`, `VERIFIED_ANSWER`, `COPILOT_VERIFIED_ANSWER`, `PBI_VERIFIED` | 5 |

### Scoring Rules

Start at 15. Deduct 5 for each component not detected.

### Detection via MCP

Check model-level annotations:

```
model = model_operations(operation: "Get")
```

Scan model annotations for the known keys listed above. The annotations are stored at the model level in the TOM (Tabular Object Model).

**Note:** The MCP server may expose annotations through `model_operations(operation: "Get")` or through direct model property inspection. If annotations are not directly accessible, this check becomes a manual checklist.

### Manual Checklist (Always Output)

Even if annotations are detected, always remind the user:

```
MANUAL CHECKLIST — verify in Power BI Desktop:
[ ] Home ribbon > Prep data for AI > Simplify data schema
[ ] Add AI instructions for business terminology
[ ] Create verified answers for 5-10 most common questions
```

### Remediation

These must be configured in Power BI Desktop or the Power BI service:

1. **AI Data Schema:**
   - Home ribbon > Prep data for AI > Simplify data schema
   - Select only tables, columns, and measures relevant to AI queries
   - Include all dependencies of selected measures (use measure dependency analysis)

2. **AI Instructions:**
   - Home ribbon > Prep data for AI > Add AI instructions
   - Define business terminology, metric preferences, default date fields
   - Keep instructions specific and non-conflicting

3. **Verified Answers:**
   - Home ribbon > Prep data for AI > Verified answers
   - Create 5-10 answers for common questions
   - Use 5-7 trigger questions per answer covering natural variations
   - Configure up to 3 filters per answer

### Key Principle

> When querying semantic models, the DAX generation tool relies solely on the semantic model's metadata and Prep for AI configurations. Instructions added at the data agent level are ignored for DAX generation.

All semantic model-specific guidance must go in Prep for AI, not in data agent instructions.
