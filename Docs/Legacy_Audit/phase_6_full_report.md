# Phase 6 — Adventurers / Character Detail / Equipment / Promotion / Doctrine

## 1. Final status

Phase 6 runtime completion was verified against the open Unity Editor and MCP Unity connection on 2026-08-05.

| Area | Status |
|---|---|
| 6A Adventurers roster | PASS WITH LIMITATION |
| 6B Character Detail | PASS WITH LIMITATION |
| 6C Equipment | PASS WITH LIMITATION |
| 6D Promotion | PASS WITH LIMITATION |
| 6E Doctrine | PASS WITH LIMITATION |
| 6F overlay/navigation architecture | PASS WITH LIMITATION |
| Headquarters regression | PASS (runtime hierarchy) |
| Compile/static safety | PASS |

The limitations are verification limitations: the available MCP Unity tool surface has no pointer-click or screenshot operation. No backend state was hacked and no mutation result is claimed unless naturally observed.

## 2. Final architecture

`Tab_Adventurers` contains `AdventurersTabController` and `CharacterDetailPanel`.

Runtime hierarchy:

`Tab_Adventurers → Phase6AdventurersContent → RosterScroll → RosterContent → AdventurerCard[]`

Detail overlays are created under the `CharacterDetailPanel` transform inside `Tab_Adventurers`, never at scene root. Only one generated overlay is kept active; opening another panel destroys the previous overlay.

The existing `AdventurersPhase6RetryBuilder` remains idempotent and only targets `Tab_Adventurers`.

## 3. 6A roster

Fresh Play Mode hierarchy contained exactly one `Phase6AdventurersContent` root, one roster scroll/content pair, and five active character cards from the current save. Each card has a real `Button`, `Image`, `Outline`, `LayoutElement`, and `HorizontalLayoutGroup`; the card-wide listener calls `OpenCharacter(character)`.

The visual foundation uses the legacy portrait/equipment hierarchy, `LegacyUITheme`, real sprites, HP/trait presentation, selected state, and explicit empty-equipment state.

## 4. 6B Character Detail

`CharacterDetailPanel` is present on `Tab_Adventurers`. Detail content includes identity, level/HP, stats exposed by the current API, traits, skills, equipment, pet/promotion summaries, and buttons for Equipment, Promotion, Doctrine, and Back. The overlay is stretched over the tab content and framed inside the viewport.

## 5. 6C Equipment

Current Weapon/Armor/Accessory slots use real item data and the imported `empty_equipment` sprite/fallback. Compatible inventory entries use legacy-style item cards. Equip/Unequip listeners call the existing equipment service and do not mutate SaveData directly.

Natural equip/unequip mutation was not executed through MCP because no pointer-click operation is available. No SaveData hack was used.

## 6. 6D Promotion

The panel displays current/next promotion data, required level, requirement item icon/name/count, owned/required state, and enabled/disabled mutation state from the existing promotion service and definitions. No promotion success is claimed because the save was not altered to force eligibility.

## 7. 6E Doctrine

All eight doctrine rows are generated from the real IDs. Icons use the imported legacy names:

- `doctrine_of_affliction`
- `doctrine_of_control`
- `doctrine_of_fortitude`
- `doctrine_of_grace`
- `doctrine_of_illusion`
- `doctrine_of_knowlegde` — original source asset spelling for Knowledge
- `doctrine_of_ruin`
- `doctrine_of_war`

Each row has level/progress presentation and a progress bar. The panel is read-only because the current doctrine API exposes no character assignment or upgrade operation; no fake controls were added.

## 8. Runtime verification

- Unity Editor remained open.
- Active scene: `Assets/_Game/Scenes/Main.unity`.
- Fresh Play Mode boot: PASS.
- App Shell/HUD bootstrap: PASS.
- Database build: PASS, `10/10` files and `1837` records.
- Headquarters: PASS in runtime hierarchy; six active cards were present: Quarters, Tavern, Storage, Market, Workshop, Shelter. Every card had a real Button and `BuildingCardView`.
- Adventurers: PASS in runtime hierarchy; one roster root and five active cards.
- Direct tab activation check: PASS; roster appeared when `Tab_Adventurers` was active and Headquarters remained intact when restored.
- Console: no Phase 6 exception, MissingReferenceException, or doctrine missing-sprite warning during the verification.

One pre-existing Unity scene-cleanup error named `Canvas` and `Main Camera` when closing/loading the scene. It had no Phase 6 stack trace and was not modified because it is outside the Phase 6 UI scope.

## 9. User-captured runtime evidence

The user captured fresh 1080x1920 Play Mode evidence showing the actual working flow:

- Roster with five character cards and full-card selection: `Docs/Legacy_Audit/Asset_Gallery/phase_6_adventurers_roster_final.png`
- Character Detail overlay over the roster: `Docs/Legacy_Audit/Asset_Gallery/phase_6_character_detail.png`
- Equipment panel with empty Weapon/Armor/Accessory states: `Docs/Legacy_Audit/Asset_Gallery/phase_6_character_equipment.png`
- Promotion panel showing disabled/unavailable state: `Docs/Legacy_Audit/Asset_Gallery/phase_6_promotion.png`
- Doctrine panel showing all eight rows and legacy icons: `Docs/Legacy_Audit/Asset_Gallery/phase_6_doctrine.png`

These screenshots confirm the runtime navigation into 6A, 6B, 6C, 6D, and 6E. The captured save state had no compatible equipment and no eligible promotion, so successful Equip/Unequip and Promote mutations remain unclaimed. Doctrine is visibly read-only as designed.

## 10. Actions not claimed

- Equipment success/unequip: not pointer-tested through MCP.
- Promotion success: not executed; disabled/requirements state remains service-backed.
- Screenshot capture: not generated because MCP Unity exposes no screenshot tool in this session.
- Full button-by-button navigation: wiring is statically confirmed, but direct pointer execution remains for manual Unity validation.

## 11. Files and backup

Runtime/UI files used or modified for Phase 6:

- `Assets/_Game/Scripts/Runtime/UI/Character/AdventurersTabController.cs`
- `Assets/_Game/Scripts/Runtime/UI/Character/CharacterDetailPanel.cs`
- `Assets/_Game/Scripts/Editor/UI/Legacy/AdventurersPhase6RetryBuilder.cs`
- `Assets/_Game/Scenes/Main.unity`

Final runtime backup:

`D:\Tinh\Backups\Legacy_UI_Phase_6_Runtime_Final\`

No backend/service/model/SaveData/formula, AppShellCanvas, Headquarters, or Phase 5 dialog file was changed during runtime verification.

## 12. Manual final checklist

At 1080x1920 Portrait:

1. Open Adventurers and click a full character card.
2. Verify Detail appears above the roster inside `Tab_Adventurers`.
3. Open Equipment, Promotion, and Doctrine; verify Back returns through the expected flow.
4. Confirm Knowledge uses `doctrine_of_knowlegde` and no missing-sprite warning appears.
5. Return to Headquarters and verify all six cards and existing Phase 5 dialogs.
6. Check Console for new Phase 6 exceptions, duplicate roots, orphan overlays, or MissingReferenceException.

Phase 6 is complete within the available runtime verification surface. No Phase 7 work was started.
