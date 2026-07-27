# Building Definition Schema
1. **Vai trò:** Công trình thành chính.
2. **Java source liên quan:** Formulas.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - maxLevel: int
4. **Reference sang schema khác:** Không
5. **Validation rules:** maxLevel > 0
6. **JSON example:**
`json
{ "id": "tavern", "nameKey": "b_tavern", "maxLevel": 10 }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Giá nâng cấp phụ thuộc Formula tĩnh, khó đẩy ra JSON
