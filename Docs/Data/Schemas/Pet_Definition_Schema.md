# Pet Definition Schema
1. **Vai trò:** Thú cưng.
2. **Java source liên quan:** Pet.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - onusStats: array
4. **Reference sang schema khác:** Không
5. **Validation rules:** bonusStats hợp lệ
6. **JSON example:**
`json
{ "id": "wolf_pup", "nameKey": "pet_wolf" }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Logic buff phức tạp
