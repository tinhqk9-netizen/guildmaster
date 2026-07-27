# Item Definition Schema
1. **Vai trò:** Định nghĩa vật phẩm cố định.
2. **Java source liên quan:** Item.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - iconKey: string (required)
   - uyPrice: int
   - sellPrice: int
4. **Reference sang schema khác:** Không
5. **Validation rules:** price >= 0
6. **JSON example:**
`json
{ "id": "plant_fiber", "nameKey": "item_plant_fiber", "iconKey": "plant_fiber", "buyPrice": 10 }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Sai sót giá trị mua bán
