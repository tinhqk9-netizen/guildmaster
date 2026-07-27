# Bản Đồ Formula & Logic (Formula Map)

*Phân tích chủ yếu từ file `Formulas.java`, `Entity.java` và `Area.java`.*

| Formula/Logic | Java source | Input | Output | Rủi ro khi port |
|---|---|---|---|---|
| **Damage (Sát thương)** | `Entity.java` (dealDamage) | Attack, Defense, Skill Multiplier, Type (Phys/Mag) | HP deduction (Trừ máu thực tế) | Sai số float/int, logic làm tròn, thứ tự apply buff/debuff. |
| **Defense Mitigation** | `Formulas.java` / `Entity.java` | Raw Damage, Defense stat | Mitigated Damage (Sát thương sau giảm trừ) | Giới hạn tối đa (Cap) của giảm sát thương (thường là 80% hoặc 90%). |
| **Healing (Hồi máu)** | `Entity.java` (heal) | Heal Amount, Heal Multiplier | Máu hồi phục | Vượt quá Max HP, logic chặn hồi máu (anti-heal effect). |
| **EXP (Kinh nghiệm)** | `Formulas.java` (experienceToNextLevel) | Cấp độ hiện tại (Level), Hệ số tiến hóa (Evo) | EXP cần để lên cấp tiếp theo | Cần đảm bảo y hệt để không phá vỡ timeline cày cuốc của người chơi. |
| **Building Scaling** | `Formulas.java` (getQuartersPrice, getTavernCapacityPrice...) | Cấp độ tòa nhà (Building Level) | Giá vàng nâng cấp | Tràn số (Overflow) nếu dùng kiểu `int` thay vì `long` (hoặc `BigInteger` nếu giá quá lớn). |
| **Capacity (Sức chứa)** | `Formulas.java` (getQuartersCapacity, storageSpaces) | Cấp độ tòa nhà | Sức chứa tối đa (Slots) | Đơn giản, rủi ro thấp. |
| **Offline Progress** | `TrueTimeUtils.java` / `MainActivity.java` | Timestamp cũ, Timestamp mới, Stats team | Vàng/EXP/Vật phẩm nhận được | Cực kỳ phức tạp. Tính toán giả lập combat offline dễ gây treo máy nếu lặp quá nhiều lần (while loop). Cần dùng công thức thống kê thay vì giả lập từng hit đánh. |
| **Drop/Reward** | `Area.java` / `Utils.java` | Drop Table (Tỉ lệ rớt), Random Seed | Danh sách Item rớt ra | Trọng số ngẫu nhiên (Weighted Random) cần dùng thuật toán chuẩn, tránh việc người chơi "khai thác" random seed. |
| **Quest Progress** | `QuestsManager.java` | Hành động của người chơi (Giết quái, nhặt đồ) | Update thanh tiến trình Quest | Cần dùng Event System (Observer pattern) thay vì check liên tục ở `Update()`. |
| **Timers (Chợ/Quán rượu)**| `Formulas.java` (getTavernVisitorInterval, marketListings) | Nâng cấp liên quan | Thời gian refresh (Mili-giây) | Đồng bộ offline/online time. |

**Đánh giá rủi ro chung:** 
Toàn bộ logic game gốc phụ thuộc rất lớn vào các phép tính toán học. Việc port từ Java sang C# yêu cầu cẩn trọng về **kiểu dữ liệu** (hạn chế dùng `float` cho tiền tệ hoặc máu nếu số quá lớn, ưu tiên `double` hoặc `long`) và **thứ tự thực hiện phép tính** (PEMDAS).
