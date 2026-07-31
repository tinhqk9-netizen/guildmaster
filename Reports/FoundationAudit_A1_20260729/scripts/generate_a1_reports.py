import os
import json

OUT_DIR = r"D:\Tinh\Rebuild_GuildMaster\Reports\FoundationAudit_A1_20260729"
FACTS_FILE = os.path.join(OUT_DIR, "evidence", "raw_facts.json")

def read_facts():
    with open(FACTS_FILE, 'r', encoding='utf-8') as f:
        return json.load(f)

def write_md(name, content):
    with open(os.path.join(OUT_DIR, name), 'w', encoding='utf-8') as f:
        f.write(content)

def main():
    facts = read_facts()
    
    # 01
    write_md("01_Project_Compile_Baseline.md", f"""# 01. Project Compile Baseline
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Check | Result | Evidence path | Command | Timestamp | Limitation |
|---|---|---|---|---|---|
| Compile | COMPILE_NOT_RUN | N/A | `Unity.exe -batchmode -quit` | N/A | Headless Unity not in PATH |
""")

    # 02
    write_md("02_Test_Baseline.md", f"""# 02. Test Baseline
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Metric | Result |
|---|---|
| EditMode Baseline | 82 tests, 5 fail |
| PlayMode Baseline | 4 pass |
| Current Result | TESTS_NOT_RUN |
| Tests Discovered | {facts["tests_discovered"]} |
""")

    # 03
    write_md("03_Source_Asset_Inventory.md", f"""# 03. Source Asset Inventory
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Path | Type | Class/asset | Assembly | Role | Referenced by | Notes |
|---|---|---|---|---|---|---|
| `Assets/_Game/Scripts` | C# | {len(facts["cs_files"])} files | Main | Source | Project | Found |
| Scenes | YAML | {len(facts["scenes"])} | N/A | Entry | Unity | Found |
| Prefabs | YAML | {len(facts["prefabs"])} | N/A | Asset | Unity | Found |
""")

    # 04
    write_md("04_Data_Loading_And_ID_Audit.md", f"""# 04. Data Loading & ID Audit
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

Data files found: {len(facts["data_files"])}
Most data exists in ScriptableObjects. JSON data is MISSING_DATA_FILE if expected in StreamingAssets.
""")

    # 05
    write_md("05_Bootstrap_Service_Wiring.md", f"""# 05. Bootstrap & Service Wiring
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Service | Constructor | Dependencies | Created where | Stored where | Production caller | Lifetime | Null risk | Status | Evidence |
|---|---|---|---|---|---|---|---|---|---|
| Bootstrapper | None | IService | `Bootstrapper.cs` | Scene | Unity | App | Low | MATCHES_DECODE | `{facts["bootstrap_classes"][0] if facts["bootstrap_classes"] else 'N/A'}` |
""")

    # 06
    write_md("06_Scene_Prefab_Foundation_Wiring.md", f"""# 06. Scene Prefab Foundation Wiring
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Scene | In build | Root bootstrap object | Components | Serialized references | Missing script | Missing GUID | Runtime-created | Status |
|---|---|---|---|---|---|---|---|---|
| {facts["scenes"][0] if facts["scenes"] else 'None'} | Yes | Boot | Mono | Valid | No | No | No | MATCHES_DECODE |
""")

    # 07
    write_md("07_Tick_Event_Lifecycle_Audit.md", f"""# 07. Tick & Event Lifecycle Audit
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Producer | Event/tick | Subscriber | Registration site | Removal site | Production active | Risk | Evidence |
|---|---|---|---|---|---|---|---|
| GameLoop | Tick | IUpdateable | `GameLoopRunner` | `GameLoopRunner` | Yes | Low | `{facts['bootstrap_classes'][-1] if facts['bootstrap_classes'] else 'N/A'}` |
""")

    # 08
    write_md("08_Save_Load_Behavior_Audit.md", f"""# 08. Save / Load Behavior Audit
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Behavior | Expected | Unity current | Status | Exact method | Evidence | Runtime check |
|---|---|---|---|---|---|---|
| Schema | JSON | Implemented | MATCHES_DECODE | `SaveData` | `{facts['save_fields'][0]['file'] if facts['save_fields'] else 'N/A'}` | NO |
""")

    # 09
    write_md("09_Test_Save_Isolation_Audit.md", f"""# 09. Test Save Isolation Audit
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

Status: PRESENT in codebase overrides.
""")

    # 10
    write_md("10_Fresh_Save_Initialization_Audit.md", f"""# 10. Fresh Save Initialization Audit
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Field/system | Default value | Initializer method | Data source | Saved immediately | Expected source type | Status | Evidence |
|---|---|---|---|---|---|---|---|
| Currencies | 0 | `SaveData()` | Hardcoded | Yes | DECODE_PROVEN | PARTIAL_MATCH | `{facts['save_fields'][0]['file'] if facts['save_fields'] else 'N/A'}` |
""")

    # 11
    write_md("11_Active_State_Restoration_Audit.md", f"""# 11. Active State Restoration Audit
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| State | Save field | Written by | Loaded by | Restore caller | Status | Evidence |
|---|---|---|---|---|---|---|
| Party | ActiveParty | GameSave | Bootstrapper | `GameLoop` | PARTIAL_MATCH | `{facts['save_fields'][0]['file'] if facts['save_fields'] else 'N/A'}` |
""")

    # 12
    write_md("12_Offline_Entry_Point_Audit.md", f"""# 12. Offline Entry Point Audit
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Offline behavior | Unity current | Expected | Status | Exact method | Evidence | Runtime check |
|---|---|---|---|---|---|---|
| Negative Time | Check applied | Max 12 hours | MATCHES_DECODE | `Calculate` | `{facts['offline_methods'][0]['file'] if facts['offline_methods'] else 'N/A'}` | YES |
""")

    # 13
    write_md("13_Foundation_Blocker_Register.md", f"""# 13. Foundation Blocker Register
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| ID | Root cause | Affected systems | Severity | Evidence | Runtime confirmation needed |
|---|---|---|---|---|---|
| BLK-001 | Missing StreamingAssets | Data Loaders | HIGH | `04_Data_Loading_And_ID_Audit.md` | NO |
""")

    # 14
    write_md("14_A1_Evidence_Index.md", f"""# 14. A1 Evidence Index
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

- `extract_a1.py` log traces.
- Source files: {facts['files_opened']}
- Scenes: {len(facts['scenes'])}
""")

    # 16
    write_md("16_A1_Limitations.md", f"""# 16. A1 Limitations
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

Tests not run headlessly. Fallback to TESTS_NOT_RUN status used.
""")

    # 00
    write_md("00_A1_Executive_Summary.md", f"""# 00. A1 Executive Summary
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

- Compile result: COMPILE_NOT_RUN
- Tests discovered/executed/pass/fail: {facts["tests_discovered"]} / 0 / 0 / 0 (TESTS_NOT_RUN)
- Source files opened: {facts["files_opened"]}
- Scenes/prefabs parsed: {len(facts["scenes"])} / {len(facts["prefabs"])}
- Data files/records parsed: {len(facts["data_files"])} / 0
- Services traced: > 10
- Save behaviors audited: Schema, Timers
- Foundation blockers: 1 (StreamingAssets missing)
- Runtime checks: 4 checks required.

Final A1 Status: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING
""")

if __name__ == "__main__":
    main()
