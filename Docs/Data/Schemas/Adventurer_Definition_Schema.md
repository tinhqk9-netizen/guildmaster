# Adventurer Definition Schema
1. **Vai trò:** Thông số gốc của anh hùng.
2. **Java source liên quan:** Adventurer.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - aseHp: int
   - aseStr: int
4. **Reference sang schema khác:** Skill_Definition
5. **Validation rules:** baseHp > 0
6. **JSON example:**
`json
{ "id": "fighter", "nameKey": "class_fighter", "baseHp": 100 }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Chỉ số base có thể bị tính toán ngầm bằng công thức Java
