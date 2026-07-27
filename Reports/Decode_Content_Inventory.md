# Bản Đồ Thống Kê Content (Content Inventory)
*Dựa trên việc đếm file và phân tích code từ bản decode.*

| Loại Content | Phân loại | Số lượng thực tế | Ghi chú |
|---|---|---|---|
| **Items** | Instances (Item cụ thể) | 607 | Định nghĩa vũ khí, vật phẩm, trang bị. |
| | Abstract Classes (Base) | 16 | Các base class (ví dụ: Sword, Armor, Material). |
| **Adventurers**| Units (Nhân vật) | 116 | Các class nghề nghiệp, anh hùng người chơi. |
| | Doctrines | 3 | Học thuyết / Nhánh kỹ năng. |
| **Enemies** | Units (Quái vật) | 121 | Mọi loại quái vật và boss. |
| **Places** | Dungeons (Ngục tối) | 11 | Các khu vực đi thám hiểm thường. |
| | Raids (Phó bản khó) | 12 | Các khu vực đi theo tổ đội lớn / boss đặc biệt. |
| **Quests** | Instances (Nhiệm vụ) | 56 | Các nhiệm vụ cụ thể có logic riêng. |
| **Pets** | Instances (Thú cưng) | 21 | Các loại pet hỗ trợ người chơi. |
| **Skills** | Enum Entries | 227 | Trong `Skills.java`, chứa cả Active và Passive. |
| **Status Effects**| Enum Entries | 25 | Trong `StatusEffectType.java` (Taunt, Stun, Bleed...). |
| **Recipes** | Enum Entries | 321 | Trong `Recipes.java` (Công thức chế đồ). |
| **UI Layouts** | XML Files | 201 | Toàn bộ các file layout giao diện gốc của Android. |
| **Assets (PNG)**| Drawable | 1035 | Chứa icon game, background, viền UI, nhân vật. |
| | Mipmap | 15 | Launcher icons của app. |
| **Assets (WebP)**| Hình ảnh nén | 5 | Một số ít ảnh dùng chuẩn WebP. |
| **Localization**| String Entries | 3930 | Trong `strings.xml`, dùng cho text, mô tả skill, tên item. |

*Lưu ý: Không đoán số. Mọi số liệu được grep và count trực tiếp từ filesystem của bản decode.*
