# CORRECTED SOURCE OF TRUTH

---

## Expected Behavior — Priority Order

When determining **what the game SHOULD do**:

| Priority | Source | Description |
|----------|--------|-------------|
| **1. DECODE_PROVEN** | Java source decode — confirmed by cross-reference | Behavior explicitly confirmed from original Java code, with at least 2 independent references |
| **2. DECODE_INFERRED** | Java source decode — logically implied | Behavior inferred from Java code structure, comments, or patterns (single source, no cross-ref) |
| **3. DESIGNED_FOR_REBUILD** | Original design intent — documented GDD/design notes | Behavior from design documents, UI mockups, feature specs where decode is unavailable |
| **4. Unity code** | Current C# runtime (Assets/_Game/Scripts/Runtime/) | Current implementation — may be incomplete, placeholder, or divergent from original design |

**Rule:** Never assume Unity C# runtime is the complete truth for expected behavior. The runtime may:
- Have placeholder/stub implementations
- Be missing entire systems (Pets, Promotion, Doctrine UI)
- Have simplified versions of original mechanics
- Contain orphan fields from early dev iterations

---

## Current Implementation — Priority Order

When determining **what the game CURRENTLY DOES**:

| Priority | Source | Description |
|----------|--------|-------------|
| **1. Unity source code** | `.cs` files in Assets/_Game/Scripts/Runtime/ | Direct method bodies, field declarations, service implementations |
| **2. Production caller** | Who calls the method? What triggers it? | UI event → Service method → SaveData mutation chain |
| **3. Scene/UI binding** | Prefab references, SerializeField, UIScreenId | What screens exist, what buttons call what methods |
| **4. Runtime/test evidence** | Play mode output, test results, log files | Actual runtime behavior — gold standard but only available after execution |

---

## Scoring Status Labels

| Label | Meaning | Applies When |
|-------|---------|-------------|
| **STATIC_TRACE_CONFIRMED** | Code path traced manually, all references exist, call chain complete | Code review only — no execution |
| **TEST_VERIFIED** | Confirmed by automated test (EditMode/PlayMode) | Test name + pass result available |
| **MANUAL_RUNTIME_VERIFIED** | Confirmed by manual play session | Screenshot/log/timestamp + actual result |
| **RUNTIME_PENDING** | Static trace done but no runtime confirmation | Awaiting playtest |
| **PARTIAL** | Partially implemented — some sub-flows exist, others missing | Code review shows gaps |
| **CONFLICTING_EVIDENCE** | Sources disagree — needs resolution | Further investigation required |
| **NOT_RUN** | Compile/EditMode/PlayMode not yet executed | No test infrastructure set up |
| **LEGACY_OR_RESERVED** | Field exists but unused — reserved for future or pending cleanup | Verified by search showing zero production callers |
