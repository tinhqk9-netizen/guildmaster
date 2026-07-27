# S6.5A-001D — DAD Decompile: Dungeon Tick / Turn Loop
**Tool:** androguard 4.1.4 (DAD decompiler) · **Nguồn:** DEX từ `it.paranoidsquirrels.idleguildmaster.apk` (XAPK v2.147)
> Hằng số double/float in ra dạng **bit-pattern long thô**. Giải mã: `struct.unpack('<d', struct.pack('<Q', bits))`.
> Mọi đoạn DAD render nghi ngờ phải đối chiếu file smali tương ứng.

---

## `Area.tick`

```java
public void tick()
    {
        if (!this.adventurersExploringIds.isEmpty()) {
            if (!this.terminationRequested) {
                if ((this.adventurersExploring.isEmpty()) || (this.restartRequested)) {
                    this.setupArea();
                }
                androidx.fragment.app.FragmentActivity v0_2 = this.action;
                if (v0_2 != null) {
                    v0_2.nextTurn();
                    this.adventureRecap.addSecondPassed();
                } else {
                    this.resetAdventurers(1);
                    this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(0);
                    this.refreshHpBars();
                    this.refreshActionDisplayed();
                    this.refreshDialog();
                    this.setupInitialDarkness();
                }
                if (this.action.finished()) {
                    if (this.needsRealignment()) {
                        this.realignToMain();
                    }
                    this.invertLogColor();
                    try {
                        this.performAction();
                    } catch (androidx.fragment.app.FragmentActivity v0_11) {
                        v0_11.printStackTrace();
                        if (it.paranoidsquirrels.idleguildmaster.MainActivity.headquartersFragment != null) {
                            it.paranoidsquirrels.idleguildmaster.MainActivity.headquartersFragment.getActivity().finish();
                        }
                        System.exit(0);
                    }
                    this.refreshHpBars();
                    this.refreshActionDisplayed();
                }
                return;
            } else {
                this.terminate();
                return;
            }
        } else {
            return;
        }
    }
```

## `Area.performAction`

```java
private void performAction()
    {
        int v3 = 4;
        switch (this.action.getType()) {
            case 0:
                this.triggerEvent("enter_dungeon");
                this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(1);
                break;
            case 1:
                if (this.getAreaType() != 0) {
                    this.incrementProgress();
                }
                this.enterRoom();
                if (this.adventurersAlive() != 0) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Action v0_11 = this.rollEnemies();
                    this.enemies = v0_11;
                    if (!v0_11.isEmpty()) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Action v0_14 = this.enemies.iterator();
                        while (v0_14.hasNext()) {
                            it.paranoidsquirrels.idleguildmaster.MainActivity.data.getSeenEnemies().add(((it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy) v0_14.next()).getTrueClass());
                        }
                        this.triggerEvent("fight_start");
                        this.initializeFight();
                        this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(2);
                    } else {
                        if (this.getAreaType() != 0) {
                            v3 = 1;
                        }
                        this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(v3);
                    }
                } else {
                    if (this.getAreaType() != 0) {
                        this.terminationRequested = 1;
                    }
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Action v0_22 = new Object[0];
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 2, v0_22);
                    this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(5);
                }
                break;
            case 2:
                if (this.adventurersAlive() != 0) {
                    if (!this.enemies.isEmpty()) {
                        if ((this.turnsFighting < 400) || (this.getAreaType() != 0)) {
                            it.paranoidsquirrels.idleguildmaster.storage.data.places.Action v0_51 = this.corpses.size();
                            this.fightTurn();
                            this.petAttack();
                            this.petHeal();
                            this.petExecution();
                            this.petCast();
                            if ((this.corpses.size() - v0_51) >= 4) {
                                it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.tabulaRasa, 1);
                            }
                            this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(2);
                        } else {
                            this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(6);
                        }
                    } else {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Action v0_57 = new Object[0];
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 5, v0_57);
                        this.triggerEvent("victory");
                        this.collectExperience();
                        this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(3);
                    }
                } else {
                    if (this.getAreaType() != 0) {
                        this.terminationRequested = 1;
                    }
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Action v0_5 = new Object[0];
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 2, v0_5);
                    this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(5);
                }
                break;
            case 3:
                this.loot();
                if (this.getAreaType() != 0) {
                    v3 = 1;
                }
                this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(v3);
                break;
            case 4:
                this.searchRoom();
                if (this.adventurersAlive() != 0) {
                    this.refreshLoot();
                    if (this.getAreaType() == 0) {
                        this.incrementProgress();
                    }
                    this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(1);
                } else {
                    if (this.getAreaType() != 0) {
                        this.terminationRequested = 1;
                    }
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Action v0_40 = new Object[0];
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 2, v0_40);
                    this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(5);
                }
                break;
            case 5:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Action v0_29 = new Object[0];
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 4, v0_29);
                this.respawn();
                if (this.progress < 250) {
                    this.progress = 0;
                }
                this.triggerEvent("respawn");
                this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(1);
                break;
            case 6:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Action v0_1 = new Object[0];
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 3, v0_1);
                this.triggerEvent("flee");
                this.clearEnemies();
                this.action = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Action(1);
                break;
            default:
        }
        this.refreshDialog();
        return;
    }
```

## `Area.fightTurn`

```java
private void fightTurn()
    {
        this.turnEndRequested = 0;
        this.turnsFighting = (this.turnsFighting + 1);
        this.selectNextActing();
        java.util.Iterator v1_16 = this.resolveStatus(this.acting);
        try {
            if ((this.acting.getCurrentHp() > 0) && (v1_16 != 2)) {
                java.util.Iterator v1_0;
                if (v1_16 == 1) {
                    v1_0 = 0;
                } else {
                    v1_0 = this.increaseMana(this.acting);
                }
                java.util.Iterator v1_6;
                if (v1_0 == null) {
                    if (!this.acting.isHealer()) {
                        java.util.Iterator v1_5 = this.acting;
                        v1_6 = this.selectTargets(v1_5, this.attackTargetStrategy(v1_5));
                        if (v1_6 != null) {
                            this.dealDamage(this.acting, ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v1_6.get(0)), 0, 0);
                        } else {
                            return;
                        }
                    } else {
                        v1_6 = this.selectTargets(this.acting, "lowest_relative_ally");
                        if (v1_6 != null) {
                            this.heal(this.acting, ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v1_6.get(0)), 0);
                        } else {
                            return;
                        }
                    }
                } else {
                    v1_6 = this.cast(this.acting);
                }
                if ((v1_6 != null) && ((!v1_6.isEmpty()) && (!this.turnEndRequested))) {
                    java.util.Iterator v1_14 = this.acting.endOfTurnActions().iterator();
                    while (v1_14.hasNext()) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v3_12 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.EndOfTurnAction) v1_14.next());
                        if (v3_12 != it.paranoidsquirrels.idleguildmaster.storage.data.entities.EndOfTurnAction.STUN_SELF_NOT_CLEANSABLE) {
                            if (v3_12 != it.paranoidsquirrels.idleguildmaster.storage.data.entities.EndOfTurnAction.FALSE_LIFE) {
                                if (!v3_12.shields) {
                                    if ((v3_12.procsOnMelee == null) || (v3_12.procsOnMelee.booleanValue() != this.acting.isRanged())) {
                                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_16 = this.acting;
                                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_17 = this.selectTargets(v4_16, this.attackTargetStrategy(v4_16));
                                        if (v4_17 != null) {
                                            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_19 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v4_17.get(0));
                                            if (v4_19.getCurrentHp() > 0) {
                                                this.dealDamage(this.acting, v4_19, 0, v3_12);
                                            }
                                        }
                                    }
                                } else {
                                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_1 = this.selectTargets(this.acting, "lowest_shield_ally");
                                    if (v4_1 != null) {
                                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_3 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v4_1.get(0));
                                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v7_1 = v4_3.getCurrentShield();
                                        v4_3.setCurrentShield(Math.min((it.paranoidsquirrels.idleguildmaster.Utils.round((((double) v3_12.damage) * this.acting.calculateHealingModifier())) + v7_1), ((int) (((double) v4_3.calculateTotalMaxHp()) * 4596373779694328218))));
                                        int v8_7 = (v4_3.getCurrentShield() - v7_1);
                                        if (v8_7 > 0) {
                                            it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 120, new Object[] {this.acting, v4_3, Integer.valueOf(v8_7)}));
                                        }
                                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v7_4 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                                        v7_4(v3_12.effect.getType(), this.acting, (v3_12.effect.getTurnsLeft() + this.acting.getInspireExaltBonusTurns()), v3_12.effect.getProbability());
                                        this.applyStatus(v4_3, v7_4, 0);
                                    }
                                }
                            } else {
                                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v3_4 = this.acting;
                                it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_4 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v9_7 = this.acting;
                                v4_4(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.FALSE_LIFE, v9_7, 999, (((double) ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v9_7).getDoctrine().falseLifeChance()) * 4576918229304087675));
                                this.applyStatus(v3_4, v4_4, 0);
                            }
                        } else {
                            it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v3_5 = this.acting;
                            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_5 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                            v4_5(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN_NOT_CLEANSABLE, this.acting, 1, 4607182418800017408);
                            this.applyStatus(v3_5, v4_5, 0);
                        }
                    }
                }
            }
        } catch (Exception) {
        }
        return;
    }
```

## `Area.enterRoom`

```java
private void enterRoom()
    {
        int v0_6;
        this.triggerEvent("enter_room");
        int v0_1 = this.petExploring;
        if (v0_1 == 0) {
            v0_6 = 0;
        } else {
            v0_6 = v0_1.getBright();
        }
        int v2_0 = this.adventurersExploring.iterator();
        while (v2_0.hasNext()) {
            long v3_3 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v2_0.next());
            if (v3_3.getCurrentHp() > 0) {
                this.resolveStatus(v3_3);
                if (v3_3.getCurrentHp() > 0) {
                    v0_6 += v3_3.darknessReduction();
                    if (v3_3.isHealer()) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_5 = this.selectTargets(v3_3, "lowest_relative_ally");
                        if (v4_5 != null) {
                            int v5_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v4_5.get(0));
                            if ((v5_1.getCurrentHp() < v5_1.calculateTotalMaxHp()) || ((v3_3.isCleanser()) && (v5_1.getNegativeStatusEffects().size() > 0))) {
                                this.heal(v3_3, ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v4_5.get(0)), 0);
                            }
                        }
                    }
                    this.petHeal();
                }
            }
        }
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.incrementToValue(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.lightBringer, ((long) v0_6));
        this.localDarkness = Math.max(0, (this.getDarkness() - v0_6));
        this.refreshDarkness();
        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 1, new Object[] {Integer.valueOf(this.localDarkness)}));
        return;
    }
```

## `Area.initializeFight`

```java
private void initializeFight()
    {
        this.turnsFighting = -1;
        this.acting = 0;
        this.savedActingEntity = 0;
        this.corpses = new java.util.concurrent.CopyOnWriteArrayList();
        this.fightRarity = it.paranoidsquirrels.idleguildmaster.UIUtils.getFightRarity(this.enemies);
        this.fightingGroup = new java.util.ArrayList();
        return;
    }
```

## `Area.decideTurnsOrder`

```java
private void decideTurnsOrder()
    {
        this.fightingGroup.clear();
        this.fightingGroup.addAll(this.adventurersExploring);
        this.fightingGroup.addAll(this.enemies);
        this.fightingGroup.removeIf(new it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$$ExternalSyntheticLambda5());
        it.paranoidsquirrels.idleguildmaster.Utils.orderByTurnsPriority(this.fightingGroup);
        return;
    }
```

## `Area.selectNextActing`

```java
private void selectNextActing()
    {
        if (this.fightingGroup.isEmpty()) {
            this.decideTurnsOrder();
        }
        int v1 = 1;
        if (this.acting == null) {
            Integer v0_18 = this.savedActingEntity;
            if ((v0_18 == null) || (v0_18.intValue() > (this.fightingGroup.size() - 1))) {
                Integer v0_2 = this.fightingGroup;
                this.acting = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v0_2.get((v0_2.size() - 1)));
            } else {
                this.acting = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) this.fightingGroup.get(this.savedActingEntity.intValue()));
            }
            this.savedActingEntity = 0;
        }
        while (v1 < this.fightingGroup.size()) {
            Integer v0_15 = ((this.fightingGroup.indexOf(this.acting) + v1) % this.fightingGroup.size());
            int v2_12 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) this.fightingGroup.get(v0_15));
            if (v2_12.getCurrentHp() <= 0) {
                v1++;
            } else {
                this.acting = v2_12;
                this.savedActingEntity = Integer.valueOf(v0_15);
                break;
            }
        }
        return;
    }
```

## `Area.resolveStatus`

```java
private int resolveStatus(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p20)
    {
        long v1_30 = new java.util.ArrayList();
        v1_30.addAll(p20.getPositiveStatusEffects());
        v1_30.addAll(p20.getNegativeStatusEffects());
        int v9 = p20.calculateTotalMaxHp();
        int v2_6 = p20.calculateTotalRegeneration();
        boolean v10 = (p20 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer);
        if (v10) {
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v3_2 = this.petExploring;
            if (v3_2 != null) {
                v2_6 += v3_2.getRegeneration();
            }
        }
        long v11_1 = v1_30.iterator();
        int v12_1 = 0;
        int v13 = v2_6;
        long v1_8 = 0;
        int v14 = 0;
        long v15_0 = 0;
        while (v11_1.hasNext()) {
            int v18;
            int v5_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v11_1.next());
            if ((v5_1.getTurnsLeft() > 0) && ((v5_1.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT) || (v5_1.getCause().getCurrentHp() > 0))) {
                long v11_2;
                int v12_0;
                long v11_0;
                v5_1.setTurnsLeft((v5_1.getTurnsLeft() - 1));
                switch (it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$1.$SwitchMap$it$paranoidsquirrels$idleguildmaster$storage$data$entities$StatusEffectType[v5_1.getType().ordinal()]) {
                    case 1:
                        v12_0 = v5_1;
                        v18 = v11_1;
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 13, new Object[] {p20, v12_0}));
                        break;
                    case 2:
                        v12_0 = v5_1;
                        v18 = v11_1;
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 14, new Object[] {p20, v12_0}));
                        break;
                    case 3:
                        long v1_29;
                        v12_0 = v5_1;
                        v18 = v11_1;
                        long v1_28 = v12_0.getCause();
                        if (v1_28 == 0) {
                            v1_29 = 0;
                        } else {
                            v1_29 = v1_28.getFreezeBonusDamage();
                        }
                        int v5_5;
                        int v2_31 = this.petExploring;
                        if ((v2_31 == 0) || (!v10)) {
                            v5_5 = 0;
                        } else {
                            v5_5 = v2_31.getBarrier();
                        }
                        v11_2 = 1;
                        long v1_33 = p20.applyDamage(((double) (v1_29 + 10)), 0, v5_5, 0);
                        if (!v10) {
                            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.slowBurn, ((long) v1_33));
                        }
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 50, new Object[] {p20, v12_0, Integer.valueOf(v1_33)}));
                        v1_8 = v11_2;
                        break;
                    case 4:
                    case 5:
                        v12_0 = v5_1;
                        v18 = v11_1;
                        v11_0 = 2;
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 15, new Object[] {p20, v12_0}));
                        v15_0 = v11_0;
                        break;
                    case 6:
                        v12_0 = v5_1;
                        v18 = v11_1;
                        v11_0 = 2;
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 111, new Object[] {p20, v12_0}));
                        break;
                    case 7:
                        v12_0 = v5_1;
                        v18 = v11_1;
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 16, new Object[] {p20, v12_0}));
                        if (v15_0 == 2) {
                        } else {
                            v15_0 = 1;
                        }
                        break;
                    case 8:
                        long v1_17;
                        v12_0 = v5_1;
                        v18 = v11_1;
                        v11_2 = 1;
                        long v1_16 = v12_0.getCause();
                        if (v1_16 == 0) {
                            v1_17 = 0;
                        } else {
                            v1_17 = (((double) v1_16.getOnFireBonusDamage()) * 4576918229304087675);
                        }
                        int v5_4;
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v3_11 = this.magicDamageAmplification();
                        int v5_3 = this.petExploring;
                        if ((v5_3 == 0) || (!v10)) {
                            v5_4 = 0;
                        } else {
                            v5_4 = v5_3.getBarrier();
                        }
                        long v1_25 = p20.applyDamage(((double) it.paranoidsquirrels.idleguildmaster.Utils.round((((v1_17 + 4587366580439587226) * ((double) v9)) * v3_11))), 1, v5_4, 0);
                        if (!v10) {
                            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.slowBurn, ((long) v1_25));
                        }
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 17, new Object[] {p20, v12_0, Integer.valueOf(v1_25)}));
                        break;
                    case 9:
                        long v15_1;
                        long v1_9 = this.magicDamageAmplification();
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v3_8 = this.petExploring;
                        if ((v3_8 == null) || (!v10)) {
                            v15_1 = v12_1;
                        } else {
                            v15_1 = v3_8.getBarrier();
                        }
                        v12_0 = v5_1;
                        v18 = v11_1;
                        v11_0 = 2;
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 118, new Object[] {p20, v12_0, Integer.valueOf(p20.applyDamage(((double) it.paranoidsquirrels.idleguildmaster.Utils.round(((((double) v9) * 4596373779694328218) * v1_9))), 1, v15_1, 0))}));
                        v1_8 = 1;
                        break;
                    case 10:
                        int v2_18 = v5_1.getCause();
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v3_4 = 4588807732320345784;
                        if (v2_18 != 0) {
                            v3_4 = (4588807732320345784 + (((double) v2_18.getRegenerationBonus()) * 4576918229304087675));
                        }
                        v13 += it.paranoidsquirrels.idleguildmaster.Utils.round((v3_4 * ((double) v9)));
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 18, new Object[] {p20, v5_1}));
                        v12_0 = v5_1;
                        v18 = v11_1;
                        break;
                    case 11:
                        long v1_5 = (v5_1.getTurnsLeft() + 1);
                        p20.setCurrentHp(Math.max(v12_1, (p20.getCurrentHp() - v1_5)));
                        if (!v10) {
                            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.slowBurn, ((long) v1_5));
                        }
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 19, new Object[] {p20, v5_1, Integer.valueOf(v1_5)}));
                        v12_0 = v5_1;
                        v1_8 = 1;
                        break;
                    case 12:
                        if (p20.getCurrentMana() < 100) {
                            p20.setCurrentHp(v12_1);
                            p20.setCurrentShield(v12_1);
                            it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 110, new Object[] {p20, p20}));
                        } else {
                        }
                        break;
                    default:
                }
                if ((v12_0.getCause() != null) && ((v12_0.getCause().getDamagePerTurnPerStatus() > 0) && (v12_0.getType().negative))) {
                    v14 += v12_0.getCause().getDamagePerTurnPerStatus();
                }
            } else {
                int v2_51;
                int v12_3 = v5_1;
                v18 = v11_1;
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 10, new Object[] {p20, v12_3.getType()}));
                if (!v12_3.getType().negative) {
                    v2_51 = p20.getPositiveStatusEffects();
                } else {
                    v2_51 = p20.getNegativeStatusEffects();
                }
                v2_51.remove(v12_3);
            }
            v11_1 = v18;
            v12_1 = 0;
        }
        int v4_6;
        if (!v10) {
            v4_6 = 0;
        } else {
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v3_19 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) p20).decay();
            if (v3_19 < 1) {
            } else {
                it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fallingApart, ((long) v3_19));
                v4_6 = 0;
                p20.setCurrentHp(Math.max(0, (p20.getCurrentHp() - v3_19)));
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 20, new Object[] {((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) p20), Integer.valueOf(v3_19)}));
                v1_8 = 1;
            }
        }
        int v7_1;
        if (v14 <= 0) {
            v7_1 = v1_8;
        } else {
            int v5_6;
            long v1_42 = this.magicDamageAmplification();
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v3_20 = this.petExploring;
            if ((v3_20 == null) || (!v10)) {
                v5_6 = v4_6;
            } else {
                v5_6 = v3_20.getBarrier();
            }
            it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 116, new Object[] {p20, Integer.valueOf(p20.applyDamage((((double) v14) * v1_42), 1, v5_6, 0))}));
            v7_1 = 1;
        }
        if ((v13 > 0) && ((p20.getCurrentHp() > 0) && (p20.getCurrentHp() < v9))) {
            long v1_49 = p20.getCurrentHp();
            int v2_58 = Math.min(v9, (v1_49 + v13));
            p20.setCurrentHp(v2_58);
            if (v10) {
                it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.soothingRemedy, ((long) (v2_58 - v1_49)));
            }
            it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 49, new Object[] {p20, Integer.valueOf(v13)}));
        }
        if (v7_1 != 0) {
            this.checkDeath(p20);
        }
        if (p20.getCurrentHp() <= 0) {
            v15_0 = 2;
        }
        return v15_0;
    }
```

## `Area.increaseMana`

```java
private boolean increaseMana(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p5)
    {
        if ((p5.getActiveSkill() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.ACTIVE_NONE) && (p5.getActiveSkill() != null)) {
            if (p5.getCurrentMana() < 100) {
                p5.setCurrentMana(Math.min(100, (p5.getCurrentMana() + p5.calculateManaRegen())));
            } else {
                p5.setCurrentMana(0);
                return 1;
            }
        }
        return 0;
    }
```

## `Area.incrementProgress`

```java
private void incrementProgress()
    {
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.longMarch, 1);
        this.adventureRecap.addAreaCleared();
        java.util.Iterator v0_5 = this.progress;
        if (v0_5 < 250) {
            java.util.Iterator v0_6 = (v0_5 + 1);
            this.progress = v0_6;
            if (this.maxProgress < v0_6) {
                this.maxProgress = v0_6;
            }
            java.util.Iterator v0_3 = this.listAreasUnlocked().entrySet().iterator();
            while (v0_3.hasNext()) {
                Object[] v1_4 = ((java.util.Map$Entry) v0_3.next());
                int v2_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.places.Area) v1_4.getKey());
                Object[] v1_7 = ((Integer) v1_4.getValue()).intValue();
                if (!v2_1.isUnlocked()) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 55, new Object[] {Integer.valueOf(v2_1.getName()), Integer.valueOf(this.progress), Integer.valueOf(v1_7)}));
                    if (this.progress >= v1_7) {
                        it.paranoidsquirrels.idleguildmaster.UIUtils.unlockArea(v2_1);
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 56, new Object[] {Integer.valueOf(v2_1.getName())}));
                    }
                }
            }
            return;
        } else {
            return;
        }
    }
```

## `Area.terminate`

```java
private void terminate()
    {
        this.adventurersExploringIds.clear();
        this.petExploringId = 0;
        this.adventurersExploring.clear();
        this.petExploring = 0;
        this.enemies.clear();
        this.corpses.clear();
        this.fightingGroup.clear();
        this.acting = 0;
        this.action = 0;
        this.turnsFighting = 0;
        this.terminationRequested = 0;
        this.event = 0;
        if (this.progress < 250) {
            this.progress = 0;
        }
        this.refreshActionDisplayed();
        this.refreshAdventurers();
        return;
    }
```

## `Area.setupArea`

```java
private void setupArea()
    {
        this.restartRequested = 0;
        this.enemies.clear();
        this.corpses.clear();
        this.fightingGroup.clear();
        this.acting = 0;
        this.action = 0;
        this.turnsFighting = 0;
        this.event = 0;
        if ((this.progress < 250) || (this.getAreaType() != 0)) {
            this.progress = 0;
        }
        this.setupAdventurers(it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAdventurers(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getPets());
        this.refreshActionDisplayed();
        this.refreshAdventurers();
        this.refreshDialog();
        return;
    }
```

## `Area.respawn`

```java
private void respawn()
    {
        this.adventureRecap.addWipe();
        this.resetAdventurers(0);
        this.clearEnemies();
        return;
    }
```

## `Area.adventurersAlive`

```java
private int adventurersAlive()
    {
        java.util.Iterator v0_1 = this.adventurersExploring.iterator();
        int v1 = 0;
        while (v0_1.hasNext()) {
            boolean v2_0 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v0_1.next());
            if ((v2_0.getCurrentHp() > 0) && (!v2_0.isSummonedMinion())) {
                v1++;
            }
        }
        return v1;
    }
```
