# Skill Definition Schema
1. **Vai trò:** Kỹ năng (Active/Passive).
2. **Java source liên quan:** Skills.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - descriptionKey: string
   - 	ype: string (enum)
4. **Reference sang schema khác:** StatusEffect
5. **Validation rules:** type thuộc [ACTIVE, PASSIVE]
6. **JSON example:**
`json
{ "id": "active_heal", "nameKey": "skill_heal", "type": "ACTIVE" }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Logic tính toán skill nằm trong code, JSON chỉ lưu tên/mô tả
