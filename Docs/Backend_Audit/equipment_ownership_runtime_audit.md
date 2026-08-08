# Equipment Ownership Runtime Audit

Date: 2026-08-07

## Scope

Read-only audit. No gameplay, SaveData, service, or UI code was changed.

Unity was in Edit Mode during the audit, so the snapshot below is from the current persisted save file:

`C:\Users\gifft\AppData\LocalLow\DefaultCompany\Rebuild_GuildMaster\save.json`

## Persisted ownership snapshot

### Characters

| Character | WeaponInstanceId |
|---|---|
| footman `6e87696c-fca8-4ea6-b1bf-38c63970472e` | `40f1a377-234a-41cb-8c79-b7ef3a05429d` |
| footman `4b62412e-b604-429d-bc56-46ba2404ca7a` | `a295aeda-dd6a-4c39-9900-b1a7835af3d1` |
| footman `7d26daf2-21cf-4551-ad5c-0b2cf618fc8e` | `6980dd41-2d1d-4e75-83ad-3631649306b7` |

### Items

There are six `copper_sword` item records and no duplicate `InstanceId` values.

- Equipped and referenced by a character: 3
- Not referenced by any character: 3
- Visible under `InventoryService.GetAllItems()` semantics: 3

The three unreferenced records are:

- `0b6f128b-8e9b-4fd7-b941-552b0eea8094`
- `66fd9c13-324c-4b38-89ca-0f7132f8e287`
- `2876a7f0-8094-4dbf-be89-39a9a4e2bcec`

## Code-path evidence

- `StorageDialog.Refresh()` calls `Inventory.GetAllItems()`.
- `InventoryService.GetAllItems()` excludes items whose `InstanceId` is referenced by any character equipment slot.
- `StorageDialog.CreateSlot()` names slots from `Definition.id` and renders the item sprite from `Definition.id`; it does not show `InstanceId`.
- `StorageItemDetailPanel` can show `Not equipped` for the three unreferenced swords when one is selected.

## Classification

- A — `GetAllItems()` returns an equipped weapon: **not reproduced** in persisted state; current save semantics return only the three unreferenced swords.
- B — `EquipmentService` fails to mark ownership: **not supported**; equipment references and `IsLocked=true` are present for all three equipped swords.
- C — duplicate weapon instances: **partial**. There are multiple instances with the same `DefinitionId`, but no duplicate `InstanceId`, and the extra instances are not referenced by characters. Their acquisition origin is not proven by this save alone.
- D — save/load creates duplicate instances: **not reproduced**. No duplicate `InstanceId` exists in the current save.

## Root cause of the observed visual symptom

The current Storage visual is ambiguous: equipped starter weapons and unowned inventory weapons share the same `DefinitionId` (`copper_sword`) and the same sprite, while the grid does not display `InstanceId` or ownership state. The three visible copper sword slots therefore look like the equipped weapons even though they are three different, unreferenced item instances.

## Limitations

This is a persisted-save snapshot, not a live in-memory log from a Play Mode session. A live A/B confirmation would require opening Storage in Play Mode and logging the actual `GetAllItems()` result, which requires a runtime diagnostic hook or permission to add a temporary debug tool. No such code was added in this audit.
