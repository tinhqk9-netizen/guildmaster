# 04. Offline Reopen Apply Summary Backend Flow

- **Production caller:** `OfflineProgressService.ApplyOfflineTime`.
- **Apply trước hay sau load:** Sau load, during startup.
- **Apply một lần hay double:** Protected against double apply by `LastAccess` update.
- **Cap có đúng 12 giờ:** Yes, `Mathf.Clamp(time, 0, 12h)`.
- **Handler hiện có:** Workshop, Market, Tavern (partial).
- **UI có summary popup không:** MISSING. (Just applies silently).
- **Timer refresh:** Requires UI to listen to offline complete event.
- **Failure path:** Exception during apply skips the rest of the systems.

| Offline step | Backend method | Applied systems | State mutation | Current UI | Required UI | Issue | Evidence |
|---|---|---|---|---|---|---|---|
| Calculate Delta | `ApplyOfflineTime` | Global | Time cap | None | None | - | `D:\Tinh\Rebuild_GuildMaster\Assets\_Game\Scripts\Runtime\Services\OfflineProgressService.cs` |
| Apply Rewards | `Market.Simulate` | Market/Workshop | Items added | None | Summary Popup | Silent apply | `D:\Tinh\Rebuild_GuildMaster\Assets\_Game\Scripts\Runtime\Services\OfflineProgressService.cs` |
