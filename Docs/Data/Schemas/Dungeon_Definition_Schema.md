# Dungeon Definition Schema
1. **Vai trò:** Hầm ngục.
2. **Java source liên quan:** Area.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - ecommendedLevel: int
   - dropTable: array of WeightedEntry
4. **Reference sang schema khác:** Enemy_Definition, Item_Definition
5. **Validation rules:** tổng weight > 0
6. **JSON example:**
`json
{ "id": "the_green_forest", "nameKey": "area_forest", "recommendedLevel": 1, "dropTable": [{"id": "wood", "weight": 100}] }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Lỗi tham chiếu ID kẻ thù
