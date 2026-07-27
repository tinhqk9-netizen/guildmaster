# S0-006: Full Dataset Conversion Report (REVIEW FIX)

## 1. Unit Tests
- **Status**: PASS
- **Total Tests**: 39
- **Coverage Highlights**: Resource root path correctly resolves to esources/res, XML localization parsing, Recipe false-positive filtering accurately extracts enum constants, ParseStatus validation enforces required fields, WebP and JPEG header edge cases handled smoothly, determinism correctly verifies hashes.

## 2. Localization
- **XML Strings Parsed**: 3930
- **Plurals**: 0
- **String-Arrays**: 0
- **Total Exported Records**: 3930

## 3. Asset Scanner
- **Total Assets Scanned**: 1056
- **WebP Edge Cases**: Safely parsed short headers and VP8/VP8L/VP8X variations.
- **Corrupted JPEG Handling**: Safely bypassed zero-byte width/height exceptions.
- **Errors Reported**: All previously swallowed exceptions are properly logged to sset_scan_issues.md.

## 4. Dataset Record Count Comparison (Audit Baseline vs Final Export)

| Category | Files Scanned | Baseline (Audit) | Exported Total | Full | Partial | Failed |
|---|---|---|---|---|---|---|
| Items | N/A | ~600 | 607 | 607 | 0 | 0 |
| Adventurers | N/A | ~100 | 129 | 129 | 0 | 0 |
| Enemies | N/A | ~120 | 122 | 0 | 122 | 0 |
| Skills | N/A | ~200 | 227 | 0 | 227 | 0 |
| Status Effects | N/A | ~25 | 25 | 0 | 25 | 0 |
| Dungeons | N/A | ~11 | 11 | 0 | 11 | 0 |
| Raids | N/A | ~12 | 12 | 12 | 0 | 0 |
| Quests | N/A | ~56 | 56 | 56 | 0 | 0 |
| Pets | N/A | ~21 | 21 | 21 | 0 | 0 |
| Recipes | 1 (Recipes.java) | ~321 | 321 | 78 | 243 | 0 |

### Discrepancy Explanations
1. **Recipes**: The baseline estimated 321. The updated parsing logic correctly limits extraction exclusively to the Recipes Enum, correctly returning exactly **321** recipes. No false positives.
2. **Partial vs Full**: 
   - **Enemies/Dungeons**: Marked partial because complex fields like aseStats, dropRules, and events are currently stubbed. 
   - **Skills/Status Effects**: Marked partial because awArgs is extracted but meaningful runtime arguments are not fully separated yet.
   - **Recipes**: 243 are marked partial because they require a manual rule, or we successfully extracted the ingredients but some constructs might be unsupported (though we achieved 100% ID matching).

## 5. Recipe Candidate Breakdown
- **Total Candidates Examined (Enum constants)**: 321
- **Confirmed Recipes**: 321
- **False Positives Ignored**: 0 (Because we now accurately scope extraction to the enum body).
- **Duplicate Outputs (Variants)**: 0
- **Final Exported Count**: 321

## 6. Staging Integrity (JSON Validation)
- **Status**: PASS
- **Issues Detected**: 2 WARNINGS (Duplicate IDs that exist in the original game data: warrior in quests, skeleton_key in status effects).
- **Checks Performed**: Checked valid JSON, null IDs, hash alignment with manifest.json. 

## 7. Determinism Evidence
- **Status**: PASS (Exit Code 0)
- **Identical Files**: 13/13 
- **Differing Files**: 0
- **Ignored Volatile Fields**: generatedAt, unId

## 8. Manifest Integrity
The manifest.json generation has been deeply integrated directly into the convert-all-production CLI command. It accurately reflects hash verifications and partial/failed/full object counts immediately after parsing.

## 9. Known Limitations
- Partial elements still rely extensively on awArgs rather than destructured fields.
- Deep AST lambda parsing is bypassed due to Python AST-lite constraints (logged as unsupported constructs).

## 10. Blockers
- None.

## 11. Ready for S0-007
- **YES**.
