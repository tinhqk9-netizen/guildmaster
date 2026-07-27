# Runtime State Schema

Runtime state biểu diễn bộ nhớ động của Game khi đang chạy.

## Player Economy
- gold (long)
- gems (int)

## Inventory
- Mảng các InventoryEntry
  - itemId (string)
  - quantity (int)

## Adventurer Instance
- instanceId (guid string)
- definitionId (string) - Trỏ về JSON tĩnh
- level (int)
- exp (long)
- equippedItems (Dictionary<Slot, string_ItemId>)
- health (float)

## Dungeon Run
- ctiveDungeonId (string)
- currentFloor (int)
- dventurersInRun (List<instanceId>)

## Offline Timestamp
- lastOfflineTick (long - unix epoch)
