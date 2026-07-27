# Localization Schema Schema
1. **Vai trò:** Bảng dịch thuật.
2. **Java source liên quan:** strings.xml
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - en: string
4. **Reference sang schema khác:** Không
5. **Validation rules:** không rỗng
6. **JSON example:**
`json
{ "id": "item_wood_name", "en": "Wood" }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Ký tự đặc biệt thoát sai
