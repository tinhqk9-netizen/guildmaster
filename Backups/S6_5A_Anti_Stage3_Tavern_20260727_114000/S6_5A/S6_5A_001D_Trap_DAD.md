# S6.5A-001D — DAD Decompile: Trap / Special Encounter
**Tool:** androguard 4.1.4 (DAD decompiler) · **Nguồn:** DEX từ `it.paranoidsquirrels.idleguildmaster.apk` (XAPK v2.147)
> Hằng số double/float in ra dạng **bit-pattern long thô**. Giải mã: `struct.unpack('<d', struct.pack('<Q', bits))`.
> Mọi đoạn DAD render nghi ngờ phải đối chiếu file smali tương ứng.

---

## `Area.trapEncounter`

```java
protected void trapEncounter(int p19, int p20, int p21, int p22, boolean p23)
    {
        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 101, new Object[] {Integer.valueOf(p19)}));
        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 25, new Object[] {Integer.valueOf(p20), Integer.valueOf(p21)}));
        Object[] v2_3 = new java.util.ArrayList(this.adventurersExploring);
        v2_3.sort(new it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$$ExternalSyntheticLambda2());
        Object[] v2_4 = v2_3.iterator();
        while (v2_4.hasNext()) {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer v3_7 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v2_4.next());
            if (v3_7.getCurrentHp() > 0) {
                Object[] v4_13;
                Object[] v4_12;
                if (!v3_7.isSaboteur()) {
                    if (p20 != it.paranoidsquirrels.idleguildmaster.R$string.constitution) {
                        if (p20 != it.paranoidsquirrels.idleguildmaster.R$string.dexterity) {
                            if (p20 != it.paranoidsquirrels.idleguildmaster.R$string.intelligence) {
                                v4_12 = 0;
                            } else {
                                v4_13 = v3_7.calculateTotalIntelligence();
                                v4_12 = ((double) v4_13);
                            }
                        } else {
                            v4_13 = v3_7.calculateTotalDexterity();
                        }
                    } else {
                        v4_13 = v3_7.calculateTotalConstitution();
                    }
                } else {
                    v4_13 = v3_7.calculateTotalDexterity();
                }
                double v6_1 = (((double) p21) / (v4_12 + ((double) p21)));
                int v8_0 = 4607182418800017408;
                if ((this.localDarkness > 0) && (!v3_7.isNightVision())) {
                    v6_1 -= ((((double) this.localDarkness) * 4576918229304087675) * (v6_1 - 4607182418800017408));
                }
                double v12_3 = v6_1;
                if (it.paranoidsquirrels.idleguildmaster.Utils.random() <= v12_3) {
                    Object[] v4_24;
                    Object[] v4_23 = this.petExploring;
                    if (v4_23 == null) {
                        v4_24 = 0;
                    } else {
                        v4_24 = v4_23.getBarrier();
                    }
                    int v10 = v4_24;
                    if (p23) {
                        v8_0 = this.magicDamageAmplification();
                    }
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 27, new Object[] {v3_7, Integer.valueOf(v3_7.applyDamage(((double) it.paranoidsquirrels.idleguildmaster.Utils.round((v8_0 * ((double) p22)))), p23, v10, 0)), Integer.valueOf((100 - ((int) (v12_3 * 4636737291354636288))))}));
                    this.checkDeath(v3_7);
                } else {
                    it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.itsATrap, 1);
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 26, new Object[] {v3_7, Integer.valueOf((100 - it.paranoidsquirrels.idleguildmaster.Utils.round((v12_3 * 4636737291354636288))))}));
                    if (v3_7.isSaboteur()) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 28, new Object[] {v3_7}));
                        break;
                    }
                }
            }
        }
        return;
    }
```

## `Area.triggerEvent`

```java
protected abstract void triggerEvent(String p0);
```
