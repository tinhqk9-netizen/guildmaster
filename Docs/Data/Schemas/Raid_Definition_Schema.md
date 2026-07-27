# Raid Definition Schema
1. **Vai trò:** Phó bản khó.
2. **Java source liên quan:** Area.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - maxParticipants: int
4. **Reference sang schema khác:** Enemy_Definition
5. **Validation rules:** maxParticipants > 0
6. **JSON example:**
`json
{ "id": "dark_citadel", "nameKey": "area_citadel", "maxParticipants": 5 }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** UNKNOWN requirements logic
