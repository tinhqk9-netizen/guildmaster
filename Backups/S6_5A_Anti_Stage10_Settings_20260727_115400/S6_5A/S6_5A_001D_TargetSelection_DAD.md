# S6.5A-001D — DAD Decompile: Target Selection
**Tool:** androguard 4.1.4 (DAD decompiler) · **Nguồn:** DEX từ `it.paranoidsquirrels.idleguildmaster.apk` (XAPK v2.147)
> Hằng số double/float in ra dạng **bit-pattern long thô**. Giải mã: `struct.unpack('<d', struct.pack('<Q', bits))`.
> Mọi đoạn DAD render nghi ngờ phải đối chiếu file smali tương ứng.

---

## `Area.selectTargets`

```java
private java.util.List selectTargets(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p8, String p9)
    {
        double v0_1 = new java.util.ArrayList();
        p9.hashCode();
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_3 = 0;
        int v5_0 = -1;
        switch (p9.hashCode()) {
            case -2010700031:
                if (p9.equals("lowest_shield_ally")) {
                    v5_0 = 0;
                } else {
                }
                break;
            case -1014930783:
                if (p9.equals("most_negative_conditions_or_lowest_relative_ally")) {
                    v5_0 = 1;
                } else {
                }
                break;
            case -967548685:
                if (p9.equals("lowest_absolute_ally")) {
                    v5_0 = 2;
                } else {
                }
                break;
            case -938285885:
                if (p9.equals("random")) {
                    v5_0 = 3;
                } else {
                }
                break;
            case -700116748:
                if (p9.equals("random_ally")) {
                    v5_0 = 4;
                } else {
                }
                break;
            case -225036020:
                if (p9.equals("random_enemy")) {
                    v5_0 = 5;
                } else {
                }
                break;
            case -198196606:
                if (p9.equals("lowest_relative_enemy")) {
                    v5_0 = 6;
                } else {
                }
                break;
            case -89587402:
                if (p9.equals("random_except_self")) {
                    v5_0 = 7;
                } else {
                }
                break;
            case 96673:
                if (p9.equals("all")) {
                    v5_0 = 8;
                } else {
                }
                break;
            case 74508525:
                if (p9.equals("lowest_absolute_enemy")) {
                    v5_0 = 9;
                } else {
                }
                break;
            case 861851016:
                if (p9.equals("all_enemies")) {
                    v5_0 = 10;
                } else {
                }
                break;
            case 1020021236:
                if (p9.equals("all_allies")) {
                    v5_0 = 11;
                } else {
                }
                break;
            case 1392064788:
                if (p9.equals("all_except_self")) {
                    v5_0 = 12;
                } else {
                }
                break;
            case 1933148350:
                if (p9.equals("lowest_relative_ally")) {
                    v5_0 = 13;
                } else {
                }
                break;
            case 2047699943:
                if (p9.equals("random_ally_except_self")) {
                    v5_0 = 14;
                } else {
                }
                break;
            default:
        }
        switch (v5_0) {
            case 0:
                java.util.List v8_28 = this.selectLowestRelativeShieldAlly(p8);
                if (v8_28 != null) {
                    v0_1.add(v8_28);
                    return v0_1;
                } else {
                    return 0;
                }
            case 1:
                int v9_7;
                if (!(p8 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
                    v9_7 = this.enemies;
                } else {
                    v9_7 = this.adventurersExploring;
                }
                if (!v9_7.isEmpty()) {
                    int v9_8 = v9_7.iterator();
                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v3_3 = 0;
                    while (v9_8.hasNext()) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v9_8.next());
                        if ((v4_2.getCurrentHp() > 0) && ((v3_3 == null) || (v4_2.getNegativeStatusEffects().size() > v3_3.getNegativeStatusEffects().size()))) {
                            v3_3 = v4_2;
                        }
                    }
                    if (v3_3 != null) {
                        if (v3_3.getNegativeStatusEffects().size() != 0) {
                            v0_1.add(v3_3);
                            return v0_1;
                        } else {
                            return this.selectTargets(p8, "lowest_relative_ally");
                        }
                    } else {
                        return 0;
                    }
                } else {
                    return 0;
                }
            case 2:
                java.util.List v8_26 = this.selectLowestHpTarget(p8, 0, 1);
                if (v8_26 != null) {
                    v0_1.add(v8_26);
                    return v0_1;
                } else {
                    return 0;
                }
            case 3:
                java.util.List v8_25 = this.selectRandomTarget(p8, 0);
                if (v8_25 != null) {
                    v0_1.add(v8_25);
                    return v0_1;
                } else {
                    return 0;
                }
            case 4:
                java.util.List v8_22;
                if (!(p8 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
                    v8_22 = this.enemies;
                } else {
                    v8_22 = this.adventurersExploring;
                }
                if (!v8_22.isEmpty()) {
                    v0_1.add(((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v8_22.get(((int) (it.paranoidsquirrels.idleguildmaster.Utils.random() * ((double) v8_22.size()))))));
                    return v0_1;
                } else {
                    return 0;
                }
            case 5:
                java.util.List v8_20 = this.selectEnemyTarget(p8);
                if (v8_20 != null) {
                    v0_1.add(v8_20);
                    return v0_1;
                } else {
                    return 0;
                }
            case 6:
                java.util.List v8_19 = this.selectLowestHpTarget(p8, 1, 0);
                if (v8_19 != null) {
                    v0_1.add(v8_19);
                    return v0_1;
                } else {
                    return 0;
                }
            case 7:
                java.util.List v8_18 = this.selectRandomTarget(p8, 1);
                if (v8_18 != null) {
                    v0_1.add(v8_18);
                    return v0_1;
                } else {
                    return 0;
                }
            case 8:
                v0_1.addAll(this.adventurersExploring);
                v0_1.addAll(this.enemies);
                if (v0_1.isEmpty()) {
                    v0_1 = 0;
                }
                return v0_1;
            case 9:
                java.util.List v8_14 = this.selectLowestHpTarget(p8, 1, 1);
                if (v8_14 != null) {
                    v0_1.add(v8_14);
                    return v0_1;
                } else {
                    return 0;
                }
            case 10:
                java.util.List v8_12;
                if (!(p8 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
                    v8_12 = this.adventurersExploring;
                } else {
                    v8_12 = this.enemies;
                }
                v0_1.addAll(v8_12);
                if (v0_1.isEmpty()) {
                    v0_1 = 0;
                }
                return v0_1;
            case 11:
                java.util.List v8_9;
                if (!(p8 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
                    v8_9 = this.enemies;
                } else {
                    v8_9 = this.adventurersExploring;
                }
                v0_1.addAll(v8_9);
                if (v0_1.isEmpty()) {
                    v0_1 = 0;
                }
                return v0_1;
            case 12:
                v0_1.addAll(this.adventurersExploring);
                v0_1.addAll(this.enemies);
                v0_1.remove(p8);
                if (v0_1.isEmpty()) {
                    v0_1 = 0;
                }
                return v0_1;
            case 13:
                java.util.List v8_6 = this.selectLowestHpTarget(p8, 0, 0);
                if (v8_6 != null) {
                    v0_1.add(v8_6);
                    return v0_1;
                } else {
                    return 0;
                }
            case 14:
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v2_3;
                if (!(p8 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
                    v2_3 = this.enemies;
                } else {
                    v2_3 = this.adventurersExploring;
                }
                int v9_13 = new java.util.ArrayList(v2_3);
                v9_13.remove(p8);
                if (!v9_13.isEmpty()) {
                    v9_13.removeIf(new it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$$ExternalSyntheticLambda3());
                    if (!v9_13.isEmpty()) {
                        v0_1.add(((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v9_13.get(((int) (it.paranoidsquirrels.idleguildmaster.Utils.random() * ((double) v9_13.size()))))));
                        return v0_1;
                    } else {
                        return 0;
                    }
                } else {
                    return 0;
                }
            default:
                int v9_11 = Integer.parseInt(p9);
                while (v4_3 < v9_11) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v2_0 = this.selectEnemyTarget(p8);
                    if (v2_0 != null) {
                        v0_1.add(v2_0);
                        v4_3++;
                    } else {
                        return 0;
                    }
                }
                return v0_1;
        }
    }
```

## `Area.attackTargetStrategy`

```java
private String attackTargetStrategy(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p6)
    {
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills v0_1;
        int v2 = 0;
        if ((p6.getPassiveSkill() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.PASSIVE_CHAOTIC) && (p6.getPassiveSkill() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.PASSIVE_PRIMORDIAL_HUNGER)) {
            v0_1 = 0;
        } else {
            v0_1 = 1;
        }
        if ((!(this instanceof it.paranoidsquirrels.idleguildmaster.storage.data.places.dungeons.LostLands)) || ((p6.getPassiveSkill() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.PASSIVE_PREHISTORIC_AVIAN) && (p6.getPassiveSkill() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.PASSIVE_PREHISTORIC_COLOSSUS))) {
            v2 = v0_1;
        } else {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills v0_3 = this.enemies.iterator();
            while (v0_3.hasNext()) {
                if (((it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy) v0_3.next()).getPassiveSkill() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.PASSIVE_NATURAL_EMPATHY) {
                }
            }
            v2 = 1;
        }
        if (v2 == 0) {
            if ((p6.getPassiveSkill() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.PASSIVE_DESPISE_WEAKNESS) && (p6.getPassiveSkill() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.PASSIVE_WICKED_APPETITE)) {
                return "random_enemy";
            } else {
                return "lowest_relative_enemy";
            }
        } else {
            return "random_except_self";
        }
    }
```

## `Area.selectEnemyTarget`

```java
private it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity selectEnemyTarget(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p13)
    {
        double v2_4;
        int v0_0 = (p13 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer);
        if (v0_0 == 0) {
            v2_4 = this.adventurersExploring;
        } else {
            v2_4 = this.enemies;
        }
        java.util.List v1_4 = new java.util.ArrayList(v2_4);
        if (p13.getTeam() != 0) {
            double v2_0 = this.enemies.iterator();
            while (v2_0.hasNext()) {
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy v3_3 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy) v2_0.next());
                if (v3_3.getTeam() != p13.getTeam()) {
                    v1_4.add(v3_3);
                }
            }
        }
        double v2_2 = new java.util.ArrayList(v1_4);
        if (!v2_2.isEmpty()) {
            java.util.List v1_2 = this.tauntedBy(p13, v2_2);
            if (v1_2 == null) {
                java.util.List v1_3 = this.weightedSelection(v2_2);
                if (!v1_3.isEmpty()) {
                    if (v0_0 == 0) {
                        int v0_1 = this.petExploring;
                        if ((v0_1 != 0) && ((v0_1.getDecoy() > 0) && (it.paranoidsquirrels.idleguildmaster.Utils.random() < (this.petExploring.getDecoy() / (((double) v1_3.size()) + this.petExploring.getDecoy()))))) {
                            it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 107, new Object[] {this.petExploring, p13}));
                            return 0;
                        }
                    }
                    return ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v1_3.get(((int) (it.paranoidsquirrels.idleguildmaster.Utils.random() * ((double) v1_3.size())))));
                } else {
                    return 0;
                }
            } else {
                return v1_2;
            }
        } else {
            return 0;
        }
    }
```

## `Area.selectRandomTarget`

```java
private it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity selectRandomTarget(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p5, boolean p6)
    {
        double v0_1 = new java.util.ArrayList();
        v0_1.addAll(this.enemies);
        v0_1.addAll(this.adventurersExploring);
        if (p6 != 0) {
            v0_1.remove(p5);
        }
        if (!v0_1.isEmpty()) {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v5_1 = this.tauntedBy(p5, v0_1);
            if (v5_1 == null) {
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v5_2 = this.weightedSelection(v0_1);
                if (!v5_2.isEmpty()) {
                    return ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v5_2.get(((int) (it.paranoidsquirrels.idleguildmaster.Utils.random() * ((double) v5_2.size())))));
                } else {
                    return 0;
                }
            } else {
                return v5_1;
            }
        } else {
            return 0;
        }
    }
```

## `Area.weightedSelection`

```java
private java.util.List weightedSelection(java.util.List p5)
    {
        java.util.ArrayList v0_1 = new java.util.ArrayList();
        java.util.Iterator v5_1 = p5.iterator();
        while (v5_1.hasNext()) {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v1_0 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v5_1.next());
            if (v1_0.getCurrentHp() > 0) {
                int v2_1 = 0;
                while (v2_1 < v1_0.getThreat()) {
                    v0_1.add(v1_0);
                    v2_1++;
                }
            }
        }
        return v0_1;
    }
```

## `Area.tauntedBy`

```java
private it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity tauntedBy(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p6, java.util.List p7)
    {
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v0_2 = p6.getNegativeStatusEffects().iterator();
        while (v0_2.hasNext()) {
            int v1_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v0_2.next());
            if (v1_1.getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT) {
            }
            if (v1_1 != 0) {
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v0_1 = v1_1.getCause();
                if (!p7.contains(v0_1)) {
                    p6.getNegativeStatusEffects().remove(v1_1);
                } else {
                    return v0_1;
                }
            }
            return 0;
        }
        v1_1 = 0;
    }
```

## `Area.selectLowestHpTarget`

```java
private it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity selectLowestHpTarget(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p8, boolean p9, boolean p10)
    {
        if (((!(p8 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) || (p9 != null)) && ((!(p8 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy)) || (p9 == null))) {
            int v1_8 = this.enemies;
        } else {
            v1_8 = this.adventurersExploring;
        }
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v0_4 = new java.util.ArrayList(v1_8);
        if (p9 == null) {
            v0_4.sort(new it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$$ExternalSyntheticLambda6());
        }
        java.util.Iterator v8_1 = this.tauntedBy(p8, v0_4);
        if (v8_1 == null) {
            java.util.Iterator v8_2 = v0_4.iterator();
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v9_3 = 0;
            while (v8_2.hasNext()) {
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v0_3 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v8_2.next());
                if ((v0_3.getCurrentHp() > 0) && (((v9_3 == null) || ((!p10) && ((((double) v9_3.getCurrentHp()) / ((double) v9_3.calculateTotalMaxHp())) > (((double) v0_3.getCurrentHp()) / ((double) v0_3.calculateTotalMaxHp()))))) || ((p10) && (v9_3.getCurrentHp() > v0_3.getCurrentHp())))) {
                    v9_3 = v0_3;
                }
            }
            return v9_3;
        } else {
            return v8_1;
        }
    }
```

## `Area.selectLowestRelativeShieldAlly`

```java
private it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity selectLowestRelativeShieldAlly(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p9)
    {
        java.util.Iterator v9_2;
        if (!(p9 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
            v9_2 = this.enemies;
        } else {
            v9_2 = this.adventurersExploring;
        }
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v0_2 = new java.util.ArrayList(v9_2);
        java.util.Collections.shuffle(v0_2);
        java.util.Iterator v9_3 = v0_2.iterator();
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v0_1 = 0;
        while (v9_3.hasNext()) {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v1_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v9_3.next());
            if (v1_2.getCurrentHp() > 0) {
                if (v0_1 != null) {
                    double v2_2 = ((double) v1_2.calculateTotalMaxHp());
                    if ((v1_2.getCurrentShield() < ((int) (4596373779694328218 * v2_2))) && ((((double) v1_2.getCurrentShield()) / v2_2) < (((double) v0_1.getCurrentShield()) / ((double) v0_1.calculateTotalMaxHp())))) {
                    }
                }
                v0_1 = v1_2;
            }
        }
        return v0_1;
    }
```

## `Area.selectPetTarget`

```java
private it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity selectPetTarget()
    {
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v0_4 = new java.util.ArrayList(this.enemies);
        if (!v0_4.isEmpty()) {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v0_5 = this.weightedSelection(v0_4);
            return ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v0_5.get(((int) (it.paranoidsquirrels.idleguildmaster.Utils.random() * ((double) v0_5.size())))));
        } else {
            return 0;
        }
    }
```

## `Area.selectPetHealingTarget`

```java
private it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity selectPetHealingTarget()
    {
        java.util.Iterator v0_1 = new java.util.ArrayList(this.adventurersExploring);
        v0_1.sort(new it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$$ExternalSyntheticLambda4());
        java.util.Iterator v0_2 = v0_1.iterator();
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v1_2 = 0;
        while (v0_2.hasNext()) {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v2_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v0_2.next());
            if ((v2_2.getCurrentHp() > 0) && ((v1_2 == null) || ((((double) v1_2.getCurrentHp()) / ((double) v1_2.calculateTotalMaxHp())) > (((double) v2_2.getCurrentHp()) / ((double) v2_2.calculateTotalMaxHp()))))) {
                v1_2 = v2_2;
            }
        }
        return v1_2;
    }
```
