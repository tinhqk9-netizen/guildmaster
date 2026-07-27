# Quest Definition Schema
1. **Vai trò:** Nhiệm vụ.
2. **Java source liên quan:** Quest.java
3. **Danh sách field:**
   - id: string (required)
   - 
ameKey: string (required) - LocalizedTextReference
   - 	argetId: string
   - ewardId: string
4. **Reference sang schema khác:** Item_Definition
5. **Validation rules:** targetId hợp lệ
6. **JSON example:**
`json
{ "id": "quest_kill_slimes", "nameKey": "quest_slime", "targetId": "green_slime", "rewardId": "gold" }
`
7. **Những field chưa chắc chắn:** OPTIONAL_PENDING_AUDIT các field ẩn.
8. **Rủi ro khi convert:** Điều kiện hoàn thành đa dạng khó quy về JSON chung
