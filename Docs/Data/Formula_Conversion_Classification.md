# Phân loại Formula Conversion

Không convert `Formulas.java` thành C# tự động. Phân chia như sau:

## Phân loại
1. **TYPE_A_DATA_EXTRACTABLE**: hằng số, bảng giá, lookup, enum mapping, static table.
2. **TYPE_B_RULE_ENGINE**: rule có điều kiện, modifier, trait effect, skill behavior, target rule.
3. **TYPE_C_MANUAL_PORT**: combat formula, timing logic phức tạp, stateful logic, Android/thread dependent logic, logic có side effect.

| Formula/Method | Java Source | Type | Lý do | Output dự kiến |
|---|---|---|---|---|
| `totalStarsToNextLp` | Formulas.java | TYPE_A | Công thức tuyến tính / Lookup | Constant array / Math |
| `getQuartersPrice` | Formulas.java | TYPE_A | Công thức giá cả | Hàm math / Table |
| `getTavernCapacityPrice` | Formulas.java | TYPE_A | Công thức giá | Hàm math |
| `getTavernTimePrice` | Formulas.java | TYPE_A | Công thức giá | Hàm math |
| `getStorageCapacityPrice` | Formulas.java | TYPE_A | Công thức giá | Hàm math |
| `getMarketListingsPrice` | Formulas.java | TYPE_A | Công thức giá | Hàm math |
| `getMarketTimePrice` | Formulas.java | TYPE_A | Công thức giá | Hàm math |
| `getWorkshopQueuePrice` | Formulas.java | TYPE_A | Công thức giá | Hàm math |
| `getWorkshopTimePrice` | Formulas.java | TYPE_A | Công thức giá | Hàm math |
| `getShelterPrice` | Formulas.java | TYPE_A | Công thức giá | Hàm math |
| `getShelterAutofeedPrice` | Formulas.java | TYPE_A | Công thức giá | Hàm math |
| `getQuartersCapacity` | Formulas.java | TYPE_A | Chỉ số theo cấp | Lookup / Math |
| `getTavernVisitorInterval` | Formulas.java | TYPE_A | Chỉ số theo cấp | Lookup / Math |
| `getTavernCapacity` | Formulas.java | TYPE_A | Chỉ số theo cấp | Lookup / Math |
| `marketListings` | Formulas.java | TYPE_A | Chỉ số theo cấp | Lookup / Math |
| `workshopQueue` | Formulas.java | TYPE_A | Chỉ số theo cấp | Lookup / Math |
| `storageSpaces` | Formulas.java | TYPE_A | Chỉ số theo cấp | Lookup / Math |
| `shelterCapacity` | Formulas.java | TYPE_A | Chỉ số theo cấp | Lookup / Math |
| `experienceToNextLevel` | Formulas.java | TYPE_A | Yêu cầu exp level | Hàm math |
| `foodToNextLevel` | Formulas.java | TYPE_A | Yêu cầu thức ăn | Hàm math |
