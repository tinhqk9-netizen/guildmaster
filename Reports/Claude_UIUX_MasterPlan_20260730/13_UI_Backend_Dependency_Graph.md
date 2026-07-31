# 13 — Implementation Dependency Graph

Chain: Backend contract → presenter/view model → reusable components → screen → navigation → feedback → tests → runtime validation.

| Workstream | Depends on | Blocks | Can parallel | Risk | Exit criteria |
|---|---|---|---|---|---|
| W1 Backend contract fixes (BG-01..BG-08 blockers) | none | Offline Summary UI, Quest UI usefulness, Merchant Buy UI usefulness, Promotion/Ascension UI, Dungeon chain progression | with W2 (design system work doesn't need backend) | HIGH — touches save schema (Party) and DatabaseBuilder registration | All BLOCKER/CRITICAL rows in `11` resolved or explicitly deferred with a UI-visible empty state |
| W2 Design system + reusable components (ConfirmPopup, progress bar wiring, card selection state, rarity badges, HUD reskin) | none (pure UI, uses existing UICardFactory/UITemporaryTheme) | every screen rebuild in Phase 1-5 | with W1 | LOW | Components exist, documented, used by at least 2 screens |
| W3 Navigation/IA rework (bottom nav, back-stack de-dup, screen-state conventions) | W2 (needs new nav components) | all screen work | with W1 | MEDIUM (touches UIService, a shared class) | New nav verified against `07_Information_Architecture.md` tree |
| W4 Fix existing-screen UI-only gaps (UG-01..UG-24, no BG dependency) | W2 | none downstream, but should land before Phase 5 polish pass | with W1, W5 | LOW-MEDIUM (touches 9 existing screens) | All P0/P1 UI gaps with no BG dependency closed |
| W5 Pets screen (net-new, backend-ready) | W2, W3 | none | with W1, W4 | LOW | Pets screen ships, matches `10` spec |
| W6 Party screen (net-new) | BG-09 (PartyService), W2, W3 | Dungeon party-select UX improvements | after W1 | MEDIUM | Party persists across restart |
| W7 Doctrine overview screen | W2, W3 (no BG dependency — DoctrineService already exposes all 8) | none | with W1, W4, W5 | LOW | All 8 doctrines visible |
| W8 Offline Summary popup | BG-01 | none | after W1 | LOW once W1 done | Summary shown once per meaningful session start |
| W9 Promotion/Ascension screen | BG-04 (data registration), BG-16 (stacking decision) | none | after W1 | MEDIUM (needs a design decision, DEC in `17`) | Promotion reachable and mathematically correct |
| W10 Raid / Shelter screens | BG-11 / BG-10 (full backend builds, out of this plan's UI scope) | — | N/A — explicitly out of scope for Phase 1-5 | HIGH (requires new game-design + engineering work first) | Not an exit criterion of this plan; tracked as future work |
| W11 Testing/runtime validation | all of the above per-screen | plan sign-off | after each screen workstream | — | Manual acceptance checks in `16` pass; PlayMode smoke test confirms no null-ref on empty data |

## What's UI-only-doable-now (no backend dependency)
W2, W3, W4 (all of UG-01..UG-24 except those tagged with a BG dependency), W5 (Pets), W7 (Doctrine overview).

## What needs backend first
W6 (Party — BG-09), W8 (Offline Summary — BG-01), W9 (Promotion — BG-04), Merchant Buy real usefulness (BG-03), Quest real usefulness (BG-02), Dungeon chain progression (BG-05).

## What can parallel
W1 and W2 have zero shared files and can run fully in parallel. W4 (existing-screen fixes) can proceed alongside W1 since none of its items are gated by a BG row.

## What needs runtime-validation-first
BG-06 (StatusEffectService wiring — confirm intent before injecting), BG-18 (Pets exclusivity — confirm intended cap before adding a guard), UG-29/UG-30 (selection-state contrast, Android back button — confirm actually absent in a running build before allocating work).
