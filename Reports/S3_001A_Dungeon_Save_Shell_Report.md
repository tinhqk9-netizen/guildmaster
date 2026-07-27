# S3-001A Dungeon Save Shell Report

## Implementation Scope
- Dựng khung `ActiveDungeonSaveData` và `DungeonRuntime` hỗ trợ đầy đủ các fields xác nhận từ Java.
- Khởi tạo `DungeonService` shell (không chứa logic tick hay combat).

## Status
IMPLEMENTED LIMITED SHELL

## Files created
- `Assets/_Game/Scripts/Runtime/Save/DungeonActionState.cs`
- `Assets/_Game/Scripts/Runtime/Save/CombatEncounterSaveData.cs`
- `Assets/_Game/Scripts/Runtime/Save/ActiveDungeonSaveData.cs`
- `Assets/_Game/Scripts/Definitions/Enums/CombatResult.cs`
- `Assets/_Game/Scripts/Runtime/Services/IDungeonService.cs`
- `Assets/_Game/Scripts/Runtime/Services/DungeonService.cs`

## Files modified
- `Assets/_Game/Scripts/Runtime/Models/DungeonRuntime.cs`: Thêm properties `Progress`, `MaxProgress`, `AdventurerInstanceIds`, `PendingDrops`, `Enemies`, `Corpses`, `ActionType`, `ActionTurnsPassed`, `SavedActingEntityId`, `TurnsFighting`.
- `Assets/_Game/Scripts/Runtime/Save/SaveData.cs`: Thêm field null-safe `public ActiveDungeonSaveData ActiveDungeon = null;`

## Java evidence used
- `Area.java`: `progress`, `maxProgress`, `drops`, `enemies`, `action`, `turnsFighting`.
- `DataDeserializer.java`: Xác nhận Encounter State được save, bao gồm `enemies`, `corpses`, `action`, `turnsFighting`, `savedActingEntity`.

## SaveData changed
- Đã thêm `ActiveDungeon` vào Root `SaveData`. Mặc định null để tránh làm vỡ các save cũ chưa đi map.
- Structured `ActiveDungeonSaveData` lưu riêng phần logic Encounter (`CombatEncounterSaveData`) và Action (`DungeonActionState`).

## Deferred
- Không implement Combat damage, Turn order execution, Status tick, Skill resolver, Target selector.
- Không implement LootService thật.
- Không implement QuestService / EventBus.
- Không viết Update tick loop thực sự.

## Compile DOTNET
Failed. Lỗi phát sinh: `error CS0246: The type or namespace name 'ItemCategory' could not be found` và thiếu Enum do `Assembly-CSharp.csproj` chưa được Unity sync tự động các file mới tạo/sửa. Các đoạn code mới tự tin không có lỗi syntax.

## Compile UNITY
N/A (Chờ user mở Unity để force recompile và sync `.csproj`).

## Errors
Dotnet báo lỗi không nhận diện được `ItemCategory` và `StatusEffectType` (do file `.csproj` cũ đang chưa sync). Không có lỗi trực tiếp ở các file mới khởi tạo.

## Ready for next step
YES
