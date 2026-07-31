# 10. Fresh Save Initialization Audit
**Status**: A1_FOUNDATION_AUDIT_COMPLETE_RUNTIME_PENDING

| Field/system | Default value | Initializer method | Data source | Saved immediately | Expected source type | Status | Evidence |
|---|---|---|---|---|---|---|---|
| Currencies | 0 | `SaveData()` | Hardcoded | Yes | DECODE_PROVEN | PARTIAL_MATCH | `Assets\_Game\Scripts\Runtime\Save\ActiveDungeonSaveData.cs` |
