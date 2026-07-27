# Master Mapping: Decode To Unity

| Decode source | Unity target module | Cách xử lý | Priority | Dependency |
|---|---|---|---|---|
| `resources/res/drawable/*.png` | `Assets/Art/` | REUSE_ASSET | High | N/A |
| `resources/res/layout/*.xml` | `Assets/Prefabs/UI` | REFERENCE_UI | High | UGUI / Canvas |
| `sources/.../Data.java` | `Scripts/Data/GameData` | CONVERT_DATA | High | SaveSystem |
| `sources/.../SaveManager.java` | `Scripts/Save/SaveSystem` | REPLACE_ANDROID | High | Newtonsoft.Json |
| `sources/.../Formulas.java` | `Scripts/Core/Formulas` | ADAPT_LOGIC | High | GameData |
| `sources/.../Entity.java` | `Scripts/Combat/Entity` | ADAPT_LOGIC | High | Formulas |
| `sources/.../Item.java` | `Scripts/Equipment/Item` | CONVERT_DATA | High | ScriptableObjects |
| `sources/.../Area.java` | `Scripts/Areas/Dungeon` | CONVERT_DATA | High | ScriptableObjects |
| `sources/.../MainActivity.java`| `Scripts/Core/GameManager`| REPLACE_ANDROID | High | Các Manager khác |
| `sources/.../TrueTimeUtils.java`| `Scripts/Core/OfflineSystem`| ADAPT_LOGIC | High | TimeSystem |
| `sources/.../Recipes.java` | `Scripts/Crafting/Recipe` | CONVERT_DATA | Medium | Item SOs |
| `sources/.../Quest.java` | `Scripts/Quests/QuestManager` | DEFER | Low | Hệ thống Combat |
| `sources/.../Pet.java` | `Scripts/Pets/PetSystem` | DEFER | Low | Hệ thống Combat |
| `sources/.../Market.java` | `Scripts/Economy/Market` | DEFER | Low | Inventory |
| Thư viện `androidx`, `firebase` | N/A | IGNORE_LIBRARY | Low | N/A |
| Billing & Ads SDK | SDK Unity tương đương | DEFER | Low | Unity Ads / IAP |
