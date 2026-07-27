# Formula Port Report

## 1. Formula Đã Port Thành Công
- `ExperienceToNextLevel(int currentLevel, bool isAdventurer)`
- `FoodToNextLevel(int currentLevel)`
- `GetQuartersPrice(int level)`
- `GetTavernCapacityPrice(int level)`
- `GetStorageCapacityPrice(int level)`
- Trình trợ giúp ẩn: `TruncatePrice(long price)` (Dựa trên `Utils.truncatePrice`).

## 2. Formula Không Có Trong Java (Sprint S2)
- Không có Enemy scaling formula (Stat địch lấy trực tiếp từ Base stats tĩnh).
- Không có Skill cost, Skill cooldown formula trong model tĩnh.
- Không có Status damage/duration formula động (turn set tĩnh từ đầu hoặc cộng dồn tuần tự).

## 3. Formula Chưa Port (Deferred to S3)
- **CalculateDamage_ManualPortRequired:** (Combat loop S3)
- **Combat Formulas:** (Armor damage reduction, Dodge, Lifesteal, Status damage tick - deferredToS3Combat)

## 4. ManualRuleRequired
- Drop table generation/Loot roll (Chưa đủ dependency loot/item sinh ngẫu nhiên).
- Skill target rule (Ai đánh ai, tầm đánh).

