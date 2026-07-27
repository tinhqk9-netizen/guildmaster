# S6.5A-001D — DAD Decompile: Merchant Roll / Price
**Tool:** androguard 4.1.4 (DAD decompiler) · **Nguồn:** DEX từ `it.paranoidsquirrels.idleguildmaster.apk` (XAPK v2.147)
> Hằng số double/float in ra dạng **bit-pattern long thô**. Giải mã: `struct.unpack('<d', struct.pack('<Q', bits))`.
> Mọi đoạn DAD render nghi ngờ phải đối chiếu file smali tương ứng.

---

## `Utils.rollPotion`

```java
public static it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer rollPotion()
    {
        int v0_2;
        int v0_0 = it.paranoidsquirrels.idleguildmaster.Utils.random();
        int v3 = 80;
        if (v0_0 >= 4591215111030249286) {
            if (v0_0 >= 4595718710657619782) {
                if (v0_0 >= 4598584637693219188) {
                    if (v0_0 >= 4600222310284990278) {
                        if (v0_0 >= 4601859982876761368) {
                            if (v0_0 >= 4603088237320589684) {
                                if (v0_0 >= 4603907073616475229) {
                                    if (v0_0 >= 4604725909912360774) {
                                        if (v0_0 >= 4605544746208246319) {
                                            if (v0_0 >= 4606363582504131864) {
                                                if (v0_0 >= 4607182418800017408) {
                                                    v0_2 = 0;
                                                    v3 = 0;
                                                } else {
                                                    v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfAgility");
                                                }
                                            } else {
                                                v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfImmunity");
                                                v3 = 70;
                                            }
                                        } else {
                                            v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfDarkness");
                                        }
                                    } else {
                                        v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfViciousness");
                                    }
                                } else {
                                    v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfPrecision");
                                }
                            } else {
                                v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfMagicDefense");
                                v3 = 100;
                            }
                        } else {
                            v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfDefense");
                            v3 = 110;
                        }
                    } else {
                        v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfHealth");
                    }
                } else {
                    v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfIntelligence");
                }
            } else {
                v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfDexterity");
            }
        } else {
            v0_2 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("PotionOfConstitution");
        }
        it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer v1_1 = new it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer(v0_2);
        v1_1.setPrice(((long) v3));
        v1_1.setGems(1);
        return v1_1;
    }
```

## `Utils.rollSpecialFoods`

```java
public static java.util.List rollSpecialFoods()
    {
        java.util.ArrayList v0_1 = new java.util.ArrayList();
        int v2 = 0;
        while (v2 < 3) {
            int v4_0 = 0;
            while ((v4_0 == 0) || (v0_1.contains(v4_0))) {
                java.util.Comparator v5_1;
                int v4_3;
                int v4_1 = it.paranoidsquirrels.idleguildmaster.Utils.random();
                if (v4_1 >= 4595172819793696085) {
                    if (v4_1 >= 4599676419421066581) {
                        if (v4_1 >= 4602678819172646912) {
                            if (v4_1 >= 4604180019048437077) {
                                if (v4_1 >= 4605681218924227242) {
                                    if (v4_1 >= 4607182418800017408) {
                                        v5_1 = 0;
                                        v4_3 = 0;
                                    } else {
                                        v4_3 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("CeremonialCake");
                                        v5_1 = 1500;
                                    }
                                } else {
                                    v4_3 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("Ambrosia");
                                    v5_1 = 800;
                                }
                            } else {
                                v4_3 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("Cheesecake");
                                v5_1 = 400;
                            }
                        } else {
                            v4_3 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("Maxxiburger");
                            v5_1 = 200;
                        }
                    } else {
                        v4_3 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("GourmetIcecream");
                        v5_1 = 100;
                    }
                } else {
                    v4_3 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("GlazedDonut");
                    v5_1 = 50;
                }
                long v6_12 = new it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer(v4_3);
                v6_12.setPrice(((long) v5_1));
                v6_12.setGems(1);
                v4_0 = v6_12;
            }
            v0_1.add(v4_0);
            v2++;
        }
        v0_1.sort(java.util.Comparator.comparingInt(new it.paranoidsquirrels.idleguildmaster.Utils$$ExternalSyntheticLambda2()));
        return v0_1;
    }
```

## `Utils.rollUpgrades`

```java
public static java.util.List rollUpgrades()
    {
        java.util.ArrayList v0_1 = new java.util.ArrayList();
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeMarketQueue() < 1) {
            v0_1.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeMarketQueue"));
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeMarketTime() < 2) {
            v0_1.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeMarketTime"));
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeQuarters() < 1) {
            v0_1.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeQuarters"));
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeShelter() < 1) {
            v0_1.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeShelter"));
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeStorage() < 10) {
            v0_1.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeStorage"));
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeTavernCapacity() < 1) {
            v0_1.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeTavernCapacity"));
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeTavernTime() < 2) {
            v0_1.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeTavernTime"));
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeWorkshopQueue() < 1) {
            v0_1.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeWorkshopQueue"));
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeWorkshopTime() < 2) {
            v0_1.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeWorkshopTime"));
        }
        java.util.ArrayList v1_36 = new java.util.ArrayList();
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUpgradeStorage() < 6) {
            int v3_13 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("UpgradeStorage");
            v0_1.remove(v3_13);
            it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer v4_6 = new it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer(v3_13);
            v4_6.setPrice(((long) ((it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Upgrade) v3_13).getGemPrice()));
            v4_6.setGems(1);
            v1_36.add(v4_6);
        }
        while ((!v0_1.isEmpty()) && (v1_36.size() < 3)) {
            int v3_7 = ((it.paranoidsquirrels.idleguildmaster.storage.data.items.Item) v0_1.get(((int) (it.paranoidsquirrels.idleguildmaster.Utils.random() * ((double) v0_1.size())))));
            v0_1.remove(v3_7);
            it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer v4_2 = new it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer(v3_7);
            v4_2.setPrice(((long) ((it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Upgrade) v3_7).getGemPrice()));
            v4_2.setGems(1);
            v1_36.add(v4_2);
        }
        return v1_36;
    }
```

## `Utils.truncatePrice`

```java
public static long truncatePrice(long p4)
    {
        if (p4 > 10000) {
            long v0_1;
            if (p4 > 1000000) {
                v0_1 = (p4 % 10000);
            } else {
                v0_1 = (p4 % 100);
            }
            return (p4 - v0_1);
        } else {
            return p4;
        }
    }
```

## `Area.rollMerchantRegularOffers`

```java
public java.util.LinkedHashMap rollMerchantRegularOffers()
    {
        return new java.util.LinkedHashMap();
    }
```

## `Area.rollMerchantSpecialOffers`

```java
public java.util.LinkedHashMap rollMerchantSpecialOffers()
    {
        return new java.util.LinkedHashMap();
    }
```
