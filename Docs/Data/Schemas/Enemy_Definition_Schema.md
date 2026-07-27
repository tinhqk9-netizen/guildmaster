# Enemy Definition Schema
1. **Vai trò:** Thông số quái vật.
2. **Java source liên quan:** Enemy.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - aseHp: int
   - aseDamage: int
4. **Reference sang schema khác:** StatusEffect_Definition
5. **Validation rules:** baseHp > 0
6. **JSON example:**
`json
{ "id": "green_slime", "nameKey": "enemy_slime", "baseHp": 50 }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Drop table phức tạp
