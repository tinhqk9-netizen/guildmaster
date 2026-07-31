# Pre-APK Build Audit — GuildMaster
**Ngày:** 2026-08-01
**Phạm vi:** Backend/Services, Save/Load, UI/Frontend, Build Readiness (Android)
**Phương pháp:** 3 agent audit song song, chỉ đọc code, không sửa. Mọi finding có evidence file:line.

---

## 1. CÓ THỂ BUILD APK NGAY HAY CHƯA?

**Kỹ thuật: CÓ THỂ COMPILE.** Không có Missing Script, không có scene reference vỡ, Android SDK/NDK/JDK đã cài đủ, IL2CPP + ARM64 đã cấu hình, Input System mới + CanvasScaler đã đúng chuẩn mobile portrait (1080x1920).

**Thực tế: CHƯA NÊN BUILD để test hoặc release.** APK sẽ cài và chạy được, nhưng:
- Game có thể **treo cứng ngay lúc mở lại app** nếu người chơi offline đủ lâu (CRIT-01).
- Nhân vật mới tuyển từ Tavern **mất vũ khí khởi đầu** trong phiên chơi hiện tại (CRIT-02).
- **Hệ thống Quest hoàn toàn không hoạt động** trên save mới — quest không bao giờ chuyển sang trạng thái "đang làm" nên tiến độ luôn bị bỏ qua (HIGH-02, coi như blocker vì phá vỡ 1 core loop).
- Nếu wiring lỗi ở bất kỳ đâu khác, màn hình báo lỗi (ErrorPopup) — chính cơ chế cứu app — cũng có thể tự crash (CRIT-01 UI).

→ **Kết luận: build được về mặt kỹ thuật, nhưng phải fix ít nhất nhóm Critical bên dưới trước khi build bản test đầu tiên có ý nghĩa.**

---

## 2. BLOCKER BẮT BUỘC PHẢI SỬA (trước khi build test)

| # | Mức | Vấn đề | File:Line | Hậu quả |
|---|-----|--------|-----------|---------|
| B1 | CRITICAL | Offline catch-up loop gọi `SaveService.Save()` (file I/O) hàng trăm nghìn lần trong vòng lặp tick | `GameLoopService.cs:59`, `DungeonService.cs:326,646` | App **treo/đơ khi mở lại** sau thời gian offline dài (tối đa 12h → ~129,600 lần ghi file) |
| B2 | CRITICAL | Weapon của Tavern guest được thêm thẳng vào save data, không qua `InventoryService.AddItem()` | `TavernService.cs:162-170`, `CharacterService.cs:140` | Nhân vật mới recruit **mất vũ khí starter** trong session hiện tại (chỉ phục hồi sau khi save+reload) |
| B3 | CRITICAL | `ErrorPopup` không null-check button trước khi subscribe, nằm trong đúng catch-block xử lý lỗi boot | `ErrorPopup.cs:21,24`, `UIRuntimeBootstrap.cs:140-149` | Nếu bất kỳ lỗi boot nào xảy ra, màn hình cứu hộ tự crash theo → **soft-lock kép, không cách nào recover** |
| B4 | HIGH (nhưng phá core loop) | `QuestRuntime.IsActive` yêu cầu state `InProgress`, nhưng không có method nào set state này khi bắt đầu quest | `QuestRuntime.cs:27`, `QuestService.cs:133` | **Toàn bộ hệ thống Quest không hoạt động** trên new game — mọi tiến độ bị drop âm thầm |

---

## 3. BUG / RỦI RO CÒN LẠI (không chặn build nhưng cần fix sớm)

### Backend — HIGH
- **Craft/Merchant queue chỉ hoàn thành 1 item mỗi lần tính offline progress**, các item còn lại trong hàng đợi không nhận giây nào (`CraftService.cs:144-158`, `MerchantService.cs:149-164`)
- **Consume nguyên liệu craft có thể ăn vào item đang trang bị** — không check `IsLocked` (`InventoryService.cs:200-229`)
- **Overflow Money/Gems delta**: cast `long` → `int` khi tính offline summary, sai số âm ở late-game (`OfflineProgressSummaryBuilder.cs:78-79`)
- **Sát thương phép không bao giờ áp dụng** — `IsMagic` luôn `false`, mọi class phép (Apprentice, Cleric, Lich...) đánh damage vật lý (`CombatService.cs:306`)
- **Craft time hardcode 10 giây**, bỏ qua `FormulaService.GetSecondsToCraft()` đã implement đúng nhưng không được gọi (`CraftService.cs:19`)
- **Rollback không đầy đủ khi save thất bại** trong DungeonService — `ActiveExpeditions` không được rollback cùng `ActiveDungeon`, gây state không nhất quán (`DungeonService.cs:642-653`)
- **Promotion (`IsAscended`) không sync vào save data**, stat multiplier bị mất sau reload (`PromotionService.cs:88`)

### Backend — MEDIUM
- `new Random()` tạo mới mỗi lần roll trong MerchantService → bias kết quả (`MerchantService.cs:63`)
- Pet level không có giới hạn trên, có thể overflow bonus (`PetService.cs:70-89`)
- Timestamp offline ghi trước khi apply effect — nếu craft/merchant throw, giây offline bị "mất" mà không apply (`OfflineProgressService.cs:40-41`)
- `UseConsumable` cap HP theo `BaseMaxHp` thay vì tổng stat thật (`InventoryService.cs:182`)
- Bare `catch {}` swallow exception trong tính stat, che giấu lỗi database (`CharacterService.cs:168-176`)
- Hai service tính offline progress dùng 2 timestamp khác nhau (`LastAccess` vs `SaveTimeUnix`) — rủi ro double-process (`GameLoopService.cs` vs `OfflineProgressService.cs`)

### UI — HIGH
- `UIService.ShowScreen` không guard chống push trùng màn hình liên tiếp → double-tap có thể khiến nút Back phải bấm 2 lần (`UIService.cs:19-39`)
- WelcomeModal/RecoveryWarningPopup/OfflineSummaryPopup cùng thiếu null-check button như ErrorPopup — cùng họ rủi ro với B3 (`WelcomeModal.cs:12`, v.v.)

### UI — MEDIUM/LOW
- DungeonScreen re-wire button mỗi 0.5s trong auto-refresh loop, tốn CPU/GC nhỏ liên tục, có thể giật nhẹ trên máy yếu (`DungeonScreen.cs:321-353`)
- EquipmentPopup so khớp slot bằng string thay vì enum — dễ vỡ âm thầm nếu enum đổi tên sau này (`EquipmentPopup.cs:66`)

### Build/Android — HIGH (trước khi release, không chặn build debug)
- `applicationIdentifier` chưa có entry riêng cho Android, vẫn `DefaultCompany` (`ProjectSettings.asset:169-176`)
- Chưa cấu hình keystore release (`androidUseCustomKeystore: 0`)

### Build/Android — MEDIUM
- `AndroidTargetSdkVersion: 0` (Automatic) — nên set tường minh
- Icon Android toàn bộ rỗng, dùng icon mặc định Unity
- Chưa có test coverage riêng cho: SaveService/SaveData end-to-end, FormulaService, EquipmentService, LootService, SkillService, StatusEffectService

---

## 4. CHECKLIST SỬA THEO THỨ TỰ

1. **[B1]** Sửa `GameLoopService.ProcessOfflineCatchup` — không gọi `Save()` bên trong vòng lặp tick; tick logic in-memory rồi save 1 lần duy nhất sau khi xử lý xong toàn bộ offline catch-up.
2. **[B2]** Sửa `TavernService.GenerateVisitor` — thêm weapon qua `InventoryService.AddItem()` thay vì ghi thẳng vào `data.Items`.
3. **[B3]** Thêm null-check cho toàn bộ button trong `ErrorPopup`, `WelcomeModal`, `RecoveryWarningPopup`, `OfflineSummaryPopup` trước khi `RemoveAllListeners()`/`AddListener()` — đồng bộ pattern với các UIScreen khác.
4. **[B4]** Thêm cơ chế `StartQuest`/chuyển `QuestState` sang `InProgress` khi quest được nhận (accept) — kiểm tra toàn bộ flow từ QuestScreen đến QuestService.
5. **[HIGH]** Sửa `CraftService.ProgressWorkshop` và `MerchantService.ProgressMarket` — loop xử lý hết hàng đợi thay vì dừng ở item đầu tiên.
6. **[HIGH]** Thêm `IsLocked` check trong `InventoryService.ConsumeByDefinitionId`.
7. **[HIGH]** Đổi `OfflineProgressSummaryBuilder` dùng `long` thay vì cast `int` cho Money/Gems delta.
8. **[HIGH]** Implement `IsMagic` theo weapon/class thật thay vì hardcode `false`; wire `FormulaService.GetSecondsToCraft()` vào `IFormulaService` và gọi trong CraftService.
9. **[HIGH]** Sửa rollback đầy đủ trong `DungeonService.SaveDungeonState` khi save fail (rollback cả `ActiveExpeditions`).
10. **[HIGH]** Sync `IsAscended` vào `CharacterSaveData` trong `PromotionService.Promote`.
11. **[MEDIUM]** Dọn các vấn đề Medium theo độ ưu tiên nghiệp vụ (Random bias, pet level cap, timestamp order, HP cap, exception swallowing).
12. **[Android]** Set `applicationIdentifier.Android`, cấu hình keystore release, set `AndroidTargetSdkVersion` tường minh, gán icon thật — làm trước khi build bản release/nộp store (không bắt buộc cho build debug nội bộ).
13. Chạy lại toàn bộ EditMode + PlayMode test sau mỗi nhóm fix để đảm bảo không phá vỡ gì khác.

---

## 5. QUY TRÌNH TEST TRƯỚC KHI BUILD APK

1. **Compile check trong Unity Editor** (không qua `dotnet build` — csproj chỉ dùng cho IntelliSense, Unity dùng compiler nội bộ). Mở Editor, xem Console không còn lỗi đỏ.
2. **Chạy toàn bộ EditMode tests** qua Unity Test Runner (`Window > General > Test Runner`) — 15 file test hiện có (Stage1–11 + Database/Verification/MultiDungeon).
3. **Chạy toàn bộ PlayMode tests** — 4 file (RuntimeSmokeTest, RuntimeActionSmokeTest, DungeonCombatLootActionTests, PlayerFacingUIActionTests).
4. **Smoke test thủ công trong Editor** theo đúng golden path:
   - New game → Tavern recruit → kiểm tra **vũ khí có xuất hiện đúng** (verify B2 đã fix)
   - Nhận quest → hoàn thành điều kiện → xác nhận **quest tiến độ tăng và complete được** (verify B4)
   - Vào dungeon → thoát app → simulate offline nhiều giờ (chỉnh clock hoặc mock timestamp) → mở lại app → xác nhận **không treo, thời gian tải hợp lý** (verify B1)
   - Trigger 1 lỗi giả (throw exception có kiểm soát trong flow boot) → xác nhận **ErrorPopup hiện ra bình thường, nút Retry/Reset hoạt động** (verify B3)
   - Craft/bán hàng loạt item vào queue → offline vài phút → quay lại → xác nhận **toàn bộ hàng đợi được xử lý**, không chỉ 1 item
5. **Build APK Development/Debug trước** (không phải Release) — cài lên thiết bị Android thật hoặc emulator, kiểm tra:
   - Touch input hoạt động đúng trên toàn bộ UI (đã xác nhận dùng Input System mới + InputSystemUIInputModule)
   - Layout đúng trên nhiều tỷ lệ màn hình (CanvasScaler đã set 1080x1920 Scale With Screen Size — test trên ít nhất 2 device tỷ lệ khác nhau)
   - Save/load hoạt động đúng khi tắt/mở lại app trên thiết bị thật (không chỉ trong Editor)
6. Chỉ sau khi bước 5 pass ổn định mới cấu hình keystore + icon + package id thật để build **Release APK**.

---

*Đánh giá dựa trên đọc code tĩnh (static review) qua 3 agent chuyên biệt — Backend, UI/Frontend, Build Readiness. Không chạy được Unity Test Runner thực tế trong phiên audit này (cần mở Unity Editor).*
