# Merchant Offer Definition Schema
1. **Vai trò:** Giao dịch.
2. **Java source liên quan:** InsaneMerchant.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - costItem: string
   - eceiveItem: string
4. **Reference sang schema khác:** Item_Definition
5. **Validation rules:** costItem hợp lệ
6. **JSON example:**
`json
{ "id": "offer_ruby", "costItem": "ruby", "receiveItem": "gold" }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Thiếu data gốc rõ ràng
