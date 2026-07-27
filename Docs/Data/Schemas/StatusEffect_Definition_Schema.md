# Status Effect Definition Schema
1. **Vai trò:** Hiệu ứng (Buff/Debuff).
2. **Java source liên quan:** StatusEffectType.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - isNegative: boolean
   - iconKey: string
4. **Reference sang schema khác:** Không
5. **Validation rules:** iconKey phải map đúng
6. **JSON example:**
`json
{ "id": "poison", "nameKey": "effect_poison", "isNegative": true }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Tham chiếu chéo với code combat
