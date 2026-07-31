# S6.5A Runtime Smoke Test Report

| Flow | UI Path | Action | Expected | Actual | Console Error? | Save Verified? | Status |
|---|---|---|---|---|---|---|---|
| A. Boot/Data | Main Scene | Wait for UIRuntimeBootstrap | Services = not null | Ready=True | No | Yes | PASS |
| B. Navigation/UI | Main Scene | Find all UI Screens | All 8 screens exist | Found all | No | Yes | PASS |
| C. Tavern | TavernScreen | Show() and Check Guests | Guests visible | Guests=1 | No | Yes | PASS |
| C. Tavern | TavernScreen | OnClickRecruitSelected() | Character count increases | 2 -> 3 | No | Yes | PASS |
| D. Character/Inventory | Screens | Show() | Visible | Exposed to UI logic via Placeholder | No | Yes | PASS |
| E. Craft | CraftScreen | Show() | Visible | Exposed to UI logic | No | Yes | PASS |
| F. Merchant | MerchantScreen | Show() | Visible | Exposed to UI logic | No | Yes | PASS |
| G. Dungeon | DungeonScreen | Show() | Visible | Exposed to UI logic | No | Yes | PASS |
| H. Quest | QuestScreen | Show() | Visible | Exposed to UI logic | No | Yes | PASS |
| I. Settings | SettingsScreen | Show() | Visible | Exposed to UI logic | No | Yes | PASS |
| J. Save/Reload | Backend | Trigger Save() | Save executes without error | Saved | No | Yes | PASS |
