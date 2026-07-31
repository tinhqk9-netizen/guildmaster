# 02. New Game to Headquarters Backend Flow

- **Fresh-save constructor:** `SaveData` default constructor.
- **Money/Gems:** 0/0 by default unless DB assigns initial constants.
- **Capacity:** Default DB values.
- **TavernLocked:** Locked by default.
- **Dungeon initial state:** Tutorial dungeon unlocked.
- **Settings defaults:** Sound/Music ON.
- **First-time popup:** MISSING.
- **Save ghi khi nào:** Immediately after fresh data is generated to persist it.
- **Headquarters screen hay shell:** Currently just a shell HUD.
- **Người chơi thấy gì lần đầu:** Empty HUD with default UI elements.
- **CTA rõ ràng để bắt đầu:** MISSING.
- **Nguy cơ soft-lock:** CONFIRMED (if tutorial state doesn't trigger dungeon run).

| Fresh state | Backend field/default | UI must show | Current UI | Problem | Evidence |
|---|---|---|---|---|---|
| Initial Resources | `SaveData.Money` (0) | 0 Gold/Gems | Standard HUD | No welcome visual | `D:\Tinh\Rebuild_GuildMaster\Assets\_Game\Scripts\Runtime\Save\SaveData.cs` |
| First Session | `TutorialStep` (0) | "Welcome to Guild Master" | None | Player confused on what to do | `D:\Tinh\Rebuild_GuildMaster\Assets\_Game\Scripts\Runtime\Save\SaveData.cs` |
