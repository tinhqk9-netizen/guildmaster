# Recipe Definition Schema
1. **Vai trò:** Công thức chế tạo.
2. **Java source liên quan:** Recipes.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - esultItemId: string
   - ingredients: array of Requirement
4. **Reference sang schema khác:** Item_Definition
5. **Validation rules:** ingredients không rỗng
6. **JSON example:**
`json
{ "id": "recipe_cloth", "resultItemId": "cloth", "ingredients": [{"itemId":"plant_fiber", "amount":4}] }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Sợ lặp vòng (Circular Dependency)
