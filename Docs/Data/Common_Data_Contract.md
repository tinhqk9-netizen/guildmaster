# Common Data Contract

## SchemaHeader
- **Mục đích:** Header chuẩn cho mọi file JSON tĩnh.
- **Fields:** \definitionsVersion\ (string, required), \createdAt\ (string, optional).

## LocalizedTextReference
- **Mục đích:** Tham chiếu đến bảng string đa ngôn ngữ.
- **Kiểu:** \string\ (key, VD: \"item_sword_name"\).

## AssetReference
- **Mục đích:** Tham chiếu ảnh/prefab.
- **Kiểu:** \string\ (VD: \"icon_sword"\).

## StatBlock
- **Mục đích:** Cụm chỉ số nhân vật/trang bị.
- **Fields:** \hp\ (int), \mp\ (int), \str\ (int), \gi\ (int), \int\ (int). Mặc định là 0.

## StatModifier
- **Mục đích:** Thay đổi chỉ số (Buff/Debuff).
- **Fields:** \statId\ (string), \alue\ (float), \	ype\ (enum: FLAT, PERCENT).

## Requirement
- **Mục đích:** Điều kiện cần để chế tạo/vào map.
- **Fields:** \itemId\ (string, optional), \mount\ (int, default 1), \level\ (int, optional).

## WeightedEntry
- **Mục đích:** Dùng cho Drop Tables (gacha/loot).
- **Fields:** \id\ (string), \weight\ (int).

## RewardDefinition
- **Mục đích:** Phần thưởng rớt ra.
- **Fields:** \gold\ (int), \exp\ (int), \drops\ (List<WeightedEntry>).
