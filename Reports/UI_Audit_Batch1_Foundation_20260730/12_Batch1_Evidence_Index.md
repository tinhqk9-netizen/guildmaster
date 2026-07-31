# 12. Batch 1 Evidence Index (Đã Cập Nhật Thực Tế)

Bảng mục lục bằng chứng đối chiếu chi tiết đường dẫn tệp tin, vị trí dòng code (line number) và vai trò chức năng tương ứng đã kiểm chứng trực tiếp trong dự án GuildMaster.

## Chỉ Mục Dẫn Chứng Code & Trạng Thái Xác Minh

| File Component | Class / Interface | Vị Trí Dòng Code (Line) | Chức Năng Đã Xác Minh (Audited Functionality) | Trạng Thái |
| :--- | :--- | :--- | :--- | :--- |
| `Assets/_Game/Scripts/Runtime/Boot/Bootstrapper.cs` | `Bootstrapper` | Dòng 24-27 (hàm `Start`), Dòng 29-75 (`InitializePipeline`) | Điểm bắt đầu khởi động boot game, tạo Service Container và gán SaveService nhưng không tự load dữ liệu. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs` | `UIRuntimeBootstrap` | Dòng 55-60 (hàm `Start`), Dòng 156-178 (`PersistSave`) | Quản lý boot luồng UI, xử lý event Pause / Quit và lưu dữ liệu an toàn. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/Save/SaveService.cs` | `SaveService` | Dòng 27-89 (hàm `Load`), Dòng 91-122 (hàm `Save`) | Logic nạp/lấy save game đồng bộ từ disk, waing file backup và restore default data khi xảy ra exception. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/Save/SaveData.cs` | `SaveData` | Dòng 130 (`long Money`), Dòng 295-321 (`NormalizeAfterLoad`) | Lược đồ (Schema) Save Model. `NormalizeAfterLoad` vá lỗi khuyết thiếu trường bằng cách khởi tạo list rỗng. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/Services/ServiceContainer.cs` | `ServiceContainer` | Dòng 53-56 (Constructor), Dòng 64-85 (Order of Services) | Điểm nạp save đồng bộ khi khởi dựng container, thứ tự nạp dồn DI các service phụ thuộc. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/Core/GameLoopRunner.cs` | `GameLoopRunner` | Dòng 40-47 (hàm `Update`) | Tự động lưu game định kỳ. Trì hoãn auto-save lần đầu 8.0 giây phòng ngừa Init Race. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/Services/TavernService.cs` | `TavernService` | Dòng 57-60 (`GetGuests`), Dòng 68-81 (`RecruitGuest`), Dòng 217-261 (Upgrades) | Logic chiêu mộ và nâng cấp Tavern, lưu trữ danh sách guests trực tiếp trong save data. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/UI/Tavern/TavernScreen.cs` | `TavernScreen` | Dòng 130-160 (`RefreshUpgrade`), Dòng 182-201 (`OnClickRecruitSelected`) | Phía Client UI bindings cho Tavern. Đồng bộ kiểu dữ liệu Vàng nâng cấp (`long`). | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/Services/QuestService.cs` | `QuestService` | Dòng 94-113 (`LoadQuests`), Dòng 176-202 (`ClaimReward`) | Trạng thái Quests load/save cùng Doctrine. Ép buộc Gems cho quest hiếm rarity >= 4. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/UI/Quest/QuestScreen.cs` | `QuestScreen` | Dòng 29 (`_doctrines`), Dòng 146-162 (`OnClickClaimSelected`) | UI bindings cho Quest. Chọn lựa Doctrine, không check rarity quest dẫn đến mismatch ở UI. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/UI/Character/CharacterScreen.cs` | `CharacterScreen` | Dòng 36 (`_partyIds`), Dòng 61-74 (`GetPartyMemberIds`), Dòng 118-138 (`BuildDetailText`) | Thiết kế quản lý Party hoàn toàn ở UI runtime, không lưu trữ trong Save model. | ĐÃ XÁC MINH |
| `Assets/_Game/Scripts/Runtime/Services/DoctrineService.cs` | `DoctrineService` | Dòng 31, 49, 105 (các trường Doctrine) | Xử lý tăng điểm phát triển cho các Doctrine "war", "economy", "growth". | ĐÃ XÁC MINH |
