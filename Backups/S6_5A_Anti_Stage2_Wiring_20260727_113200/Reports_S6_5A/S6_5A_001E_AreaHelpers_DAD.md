# S6.5A-001E — Area Helpers + Utils (DAD)

**DEX:** classes3.dex · **Tool:** androguard 4.1.4 DAD

---

## `Area.cast(Lit/paranoidsquirrels/idleguildmaster/storage/data/entities/Entity;)Ljava/util/List;`

```java
private java.util.List cast(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p23)
    {
        if (((p23 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) && (this.magicDamageAmplification() >= 4609884578576439706)) {
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.laroxianPower, 1);
        }
        java.util.List v1_193;
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_6 = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill(this, p23);
        long v11_12 = 4621819117588971520;
        int v16 = 0;
        switch (it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$1.$SwitchMap$it$paranoidsquirrels$idleguildmaster$storage$data$entities$Skills[p23.getActiveSkill().ordinal()]) {
            case 2:
                v1_193 = v8_6.setDamageAmplification(4611686018427387904).execute();
                break;
            case 3:
                return v8_6.setDamageAmplification(4612811918334230528).execute();
            case 4:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_21 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_21(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT, p23, 2, 4607182418800017408);
                return v8_6.setStatusEffect(v7_21).applyEffectOnDodge().setDamageAmplification(4611686018427387904).execute();
            case 5:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_20 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_20(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT, p23, 4, 4607182418800017408);
                return v8_6.setStatusEffect(v7_20).applyEffectOnDodge().setDamageAmplification(4611686018427387904).execute();
            case 6:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_19 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_19(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT, p23, 8, 4607182418800017408);
                return v8_6.setStatusEffect(v7_19).applyEffectOnDodge().setDamageAmplification(4611686018427387904).execute();
            case 7:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_18 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_18(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT, p23, 8, 4607182418800017408);
                return v8_6.setStatusEffect(v7_18).applyEffectOnDodge().setDamageAmplification(4618441417868443648).execute();
            case 8:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_17 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_17(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.DEFENSIVE_STANCE, p23, 999, 4607182418800017408);
                this.applyStatus(p23, v7_17, 0);
                return v8_6.setDamageAmplification(4611686018427387904).execute();
            case 9:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_16 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_16(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 1, 4604480259023595110);
                return v8_6.setStatusEffect(v7_16).setDamageAmplification(4613937818241073152).execute();
            case 10:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_15 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_11 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_11(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 1, 4604480259023595110);
                return v7_15.setStatusEffect(v8_11).setDamageAmplification(4613937818241073152).execute();
            case 11:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_14 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_10 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_10(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 1, 4607182418800017408);
                return v7_14.setStatusEffect(v8_10).setDamageAmplification(4613937818241073152).execute();
            case 12:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_13 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_9 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_9(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 1, 4607182418800017408);
                return v7_13.setStatusEffect(v8_9).setDamageAmplification(4616189618054758400).execute();
            case 13:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_12 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_12(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.SILENCE, p23, 1, 4607182418800017408);
                return v8_6.setStatusEffect(v7_12).applyEffectOnDodge().setDamageAmplification(4612811918334230528).execute();
            case 14:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_11 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_8 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_8(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.SILENCE, p23, 1, 4607182418800017408);
                return v7_11.setStatusEffect(v8_8).applyEffectOnDodge().setDamageAmplification(4612811918334230528).execute();
            case 15:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_10 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_7 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_7(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.SILENCE, p23, 2, 4607182418800017408);
                return v7_10.setStatusEffect(v8_7).applyEffectOnDodge().setDamageAmplification(4612811918334230528).execute();
            case 16:
                return v8_6.setTargetSelectionMode("2").execute();
            case 17:
                if (p23.getTrueClass().equals("EldritchAlchemist")) {
                    java.util.List v1_119 = p23.getPositiveStatusEffects().iterator();
                    while (v1_119.hasNext()) {
                        if (((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v1_119.next()).getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.FEEBLE_TETHER) {
                            v16 = 1;
                            break;
                        }
                    }
                }
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill vtmp403 = v8_6.setTargetSelectionMode("3");
                if (v16 == 0) {
                    v11_12 = 4607182418800017408;
                }
                return vtmp403.setDamageAmplification(v11_12).execute();
            case 18:
                return v8_6.setTargetSelectionMode("4").execute();
            case 19:
                return v8_6.setTargetSelectionMode("5").execute();
            case 20:
                return v8_6.setTargetSelectionMode("6").execute();
            case 21:
                return v8_6.setTargetSelectionMode("7").execute();
            case 22:
                return v8_6.setTargetSelectionMode("9").execute();
            case 23:
                return v8_6.setTargetSelectionMode("11").execute();
            case 24:
                v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4602678819172646912);
                v8_6.execute();
                return v8_6.noLog().execute();
            case 25:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_9 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_5 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_5(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 1, 4607182418800017408);
                return v7_9.setStatusEffect(v8_5).applyEffectOnDodge().setDamageAmplification(4611686018427387904).setForceRange(Boolean.valueOf(1)).execute();
            case 26:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_8 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_4 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_4(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 1, 4607182418800017408);
                return v7_8.setStatusEffect(v8_4).applyEffectOnDodge().setDamageAmplification(4613937818241073152).setForceRange(Boolean.valueOf(1)).execute();
            case 27:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_6 = v8_6.setTargetSelectionMode("all_enemies");
                long v9_0 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v9_0(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 1, 4607182418800017408);
                v7_6.setStatusEffect(v9_0).applyEffectOnDodge().setDamageAmplification(4610334938539176755).setForceRange(Boolean.valueOf(1)).execute();
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_7 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_7(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.FROZEN, p23, 2, 4607182418800017408);
                return v8_6.setStatusEffect(v7_7).setDamageAmplification(4610334938539176755).noLog().execute();
            case 28:
                return v8_6.setCriticalAmplification(4609434218613702656).execute();
            case 29:
                return v8_6.setCriticalAmplification(4611686018427387904).execute();
            case 30:
                return v8_6.setCriticalAmplification(4613937818241073152).execute();
            case 31:
                return v8_6.setCriticalAmplification(4609434218613702656).setDamageAmplification((((((double) this.localDarkness) * 4576918229304087675) + 4607182418800017408) * 4611686018427387904)).execute();
            case 32:
                return v8_6.setCriticalAmplification(4609434218613702656).setDamageAmplification((((((double) this.localDarkness) * 4576918229304087675) + 4607182418800017408) * 4617315517961601024)).execute();
            case 33:
                return v8_6.setCriticalAmplification(4611686018427387904).setDamageAmplification((((((double) this.localDarkness) * 4576918229304087675) + 4607182418800017408) * 4618441417868443648)).execute();
            case 34:
                return v8_6.setTargetSelectionMode("lowest_absolute_enemy").setCriticalAmplification(4613937818241073152).recastOnKill().execute();
            case 35:
                return v8_6.setTargetSelectionMode("lowest_relative_enemy").setCriticalAmplification(4613937818241073152).setExecutionThreshold(4591870180066957722).recastOnKill().execute();
            case 36:
                return v8_6.setTargetSelectionMode("lowest_relative_enemy").setCriticalAmplification(4613937818241073152).setExecutionThreshold(4596373779694328218).recastOnKill().execute();
            case 37:
                return v8_6.setTargetSelectionMode("lowest_relative_enemy").setCriticalAmplification(4613937818241073152).setExecutionThreshold(4598175219545276416).recastOnKill().execute();
            case 38:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_5 = v8_6.setTargetSelectionMode("random_enemy").setCriticalAmplification(4609434218613702656);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_2 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_2(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 1, 4607182418800017408);
                return v7_5.setStatusEffect(v8_2).execute();
            case 39:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_4 = v8_6.setTargetSelectionMode("random_enemy").setCriticalAmplification(4609434218613702656).setForceRange(Boolean.valueOf(1));
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_1 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_1(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.PETRIFY, p23, 1, 4607182418800017408);
                return v7_4.setStatusEffect(v8_1).execute();
            case 40:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_2 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_2(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.BLEED, p23, 0, 4607182418800017408);
                return v8_6.setStatusEffect(v7_2).setCriticalAmplification(4613937818241073152).execute();
            case 41:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_1 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_1(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.BLEED, p23, 0, 4607182418800017408);
                return v8_6.setStatusEffect(v7_1).setCriticalAmplification(4613937818241073152).execute();
            case 42:
                return v8_6.setDamageAmplification(4609434218613702656).setForceRange(Boolean.valueOf(1)).execute();
            case 43:
                return v8_6.setDamageAmplification(4611686018427387904).setForceRange(Boolean.valueOf(1)).execute();
            case 44:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_52 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_52(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 1, 4607182418800017408);
                return v8_6.setStatusEffect(v7_52).setForceRange(Boolean.valueOf(1)).setDamageAmplification(4611686018427387904).execute();
            case 45:
                long v9_10 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v9_10(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 1, 4607182418800017408);
                return v8_6.setStatusEffect(v9_10).setTargetSelectionMode("all_enemies").setForceRange(Boolean.valueOf(1)).setDamageAmplification(4611686018427387904).execute();
            case 46:
                long v9_9 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v9_9(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 1, 4607182418800017408);
                return v8_6.setStatusEffect(v9_9).setTargetSelectionMode("all_enemies").setForceRange(Boolean.valueOf(1)).setDamageAmplification(4612361558371493478).execute();
            case 47:
                long v9_8 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v9_8(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 2, 4607182418800017408);
                return v8_6.setStatusEffect(v9_8).setTargetSelectionMode("all_enemies").setForceRange(Boolean.valueOf(1)).setDamageAmplification(4612361558371493478).execute();
            case 48:
                return v8_6.setTargetSelectionMode("lowest_relative_ally").healing().setDamageAmplification(4611686018427387904).execute();
            case 49:
                return v8_6.setTargetSelectionMode("all_allies").healing().setDamageAmplification(4611686018427387904).execute();
            case 50:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_51 = v8_6.setTargetSelectionMode("all_allies").healing();
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_32 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_32(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.REGENERATION, p23, 2, 4607182418800017408);
                return v7_51.setStatusEffect(v8_32).setDamageAmplification(4611686018427387904).execute();
            case 51:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_49 = v8_6.setTargetSelectionMode("all_allies").healing();
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_30 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_30(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.REGENERATION, p23, 3, 4607182418800017408);
                return v7_49.setStatusEffect(v8_30).setDamageAmplification(4612361558371493478).execute();
            case 52:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_48 = v8_6.setTargetSelectionMode("all_allies").healing();
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_29 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_29(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.REGENERATION, p23, 3, 4607182418800017408);
                return v7_48.setStatusEffect(v8_29).setDamageAmplification(4612361558371493478).setReviveProbability(4585925428558828667).execute();
            case 53:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_47 = v8_6.setTargetSelectionMode("all_allies").healing();
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_28 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_28(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.REGENERATION, p23, 3, 4607182418800017408);
                return v7_47.setStatusEffect(v8_28).setDamageAmplification(4613037098315599053).setReviveProbability(4588807732320345784).execute();
            case 54:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_46 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_46(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.LESSER_CURSE, p23, 999, 4607182418800017408);
                return v8_6.setStatusEffect(v7_46).setDamageAmplification(4613937818241073152).execute();
            case 55:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_45 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_45(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.CURSE, p23, 999, 4607182418800017408);
                return v8_6.setStatusEffect(v7_45).setDamageAmplification(4614500768194494464).execute();
            case 56:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_44 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_44(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.GREATER_CURSE, p23, 999, 4607182418800017408);
                return v8_6.setStatusEffect(v7_44).setDamageAmplification(4615063718147915776).execute();
            case 57:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_43 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_43(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.OMINOUS_CURSE, p23, 999, 4607182418800017408);
                return v8_6.setStatusEffect(v7_43).setDamageAmplification(4615626668101337088).execute();
            case 58:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_42 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_42(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABHORRENT_CURSE, p23, 999, 4607182418800017408);
                return v8_6.setStatusEffect(v7_42).setDamageAmplification(4616189618054758400).execute();
            case 59:
                return v8_6.setTargetSelectionMode("random_except_self").setDamageAmplification(4621819117588971520).setForceRange(Boolean.valueOf(0)).execute();
            case 60:
                return v8_6.setTargetSelectionMode("all_except_self").setDamageAmplification(4621819117588971520).setForceRange(Boolean.valueOf(0)).execute();
            case 61:
                return v8_6.setTargetSelectionMode("all_except_self").setDamageAmplification(4626322717216342016).setForceRange(Boolean.valueOf(0)).execute();
            case 62:
                return v8_6.setTargetSelectionMode("all_except_self").setDamageAmplification(4629137466983448576).setForceRange(Boolean.valueOf(0)).execute();
            case 63:
                v8_6.setTargetSelectionMode("all_except_self").setDamageAmplification(4629137466983448576).setForceRange(Boolean.valueOf(0)).execute();
                return v8_6.setTargetSelectionMode("random_except_self").setForceRange(Boolean.valueOf(1)).noLog().execute();
            case 64:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_40 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_26 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_26(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 1, 4607182418800017408);
                return v7_40.setStatusEffect(v8_26).execute();
            case 65:
                this.enemies.remove(p23);
                this.fightingGroup.remove(p23);
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 43, new Object[] {Integer.valueOf(p23.getIdName())}));
                v1_193 = 0;
                break;
            case 66:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_39 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_25 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_25(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 3, 4603579539098121011);
                return v7_39.setStatusEffect(v8_25).applyEffectOnDodge().execute();
            case 67:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_38 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_38(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.SILENCE, p23, 4, 4607182418800017408);
                return v8_6.setStatusEffect(v7_38).setDamageAmplification(4611686018427387904).execute();
            case 68:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_37 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_24 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_24(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.SILENCE, p23, 5, 4605380978949069210);
                return v7_37.setStatusEffect(v8_24).setDamageAmplification(4602678819172646912).applyEffectOnDodge().execute();
            case 69:
                return v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4606281698874543309).execute();
            case 70:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_35 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_22 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_22(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT, p23, 2, 4607182418800017408);
                return v7_35.setStatusEffect(v8_22).setDamageAmplification(4591870180066957722).applyEffectOnDodge().execute();
            case 71:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_34 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_21 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_21(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 1, 4602678819172646912);
                return v7_34.setStatusEffect(v8_21).setDamageAmplification(4612811918334230528).execute();
            case 72:
                return v8_6.setDamageAmplification(4616189618054758400).execute();
            case 73:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_33 = v8_6.setDamageAmplification(4611686018427387904).setForceRange(Boolean.valueOf(1));
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_20 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_20(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 1, 4607182418800017408);
                return v7_33.setStatusEffect(v8_20).execute();
            case 74:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_32 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v7_32(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.FROZEN, p23, 20, 4607182418800017408);
                return v8_6.setStatusEffect(v7_32).setDamageAmplification(4621819117588971520).execute();
            case 75:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_31 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_19 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_19(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.FROZEN, p23, 2, 4607182418800017408);
                return v7_31.setStatusEffect(v8_19).execute();
            case 76:
                return v8_6.setTargetSelectionMode("12").execute();
            case 77:
                return v8_6.setTargetSelectionMode("4").setDamageAmplification(4609434218613702656).execute();
            case 78:
                return v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4652601045120188416).setCriticalAmplification(4604119971053405471).execute();
            case 79:
                return v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4604930618986332160).setCriticalAmplification(4604119971053405471).execute();
            case 80:
                p23.setCurrentHp(Math.max(1, (p23.getCurrentHp() + -5000)));
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 101, new Object[] {Integer.valueOf(it.paranoidsquirrels.idleguildmaster.R$string.log_the_cultist_rebels_fragmentation)}));
                return v8_6.setTargetSelectionMode("5").setForceRange(Boolean.valueOf(1)).execute();
            case 81:
                return v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4600877379321698714).execute();
            case 82:
                return v8_6.setDamageAmplification(4636737291354636288).setCriticalAmplification(4604119971053405471).execute();
            case 83:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_29 = v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4600877379321698714);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_17 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_17(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.SILENCE, p23, 3, 4607182418800017408);
                return v7_29.setStatusEffect(v8_17).applyEffectOnDodge().execute();
            case 84:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_28 = v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4591870180066957722);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_16 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_16(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 2, 4607182418800017408);
                return v7_28.setStatusEffect(v8_16).execute();
            case 85:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_27 = v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4599976659396224614);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_15 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_15(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.BLEED, p23, 40, 4607182418800017408);
                return v7_27.setStatusEffect(v8_15).execute();
            case 86:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_25 = v8_6.setTargetSelectionMode("all_enemies");
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_14 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_14(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT, p23, 5, 4607182418800017408);
                return v7_25.setStatusEffect(v8_14).setDamageAmplification(4591870180066957722).applyEffectOnDodge().execute();
            case 87:
                java.util.List v1_227 = this.enemies.iterator();
                while (v1_227.hasNext()) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v2_78 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy) v1_227.next());
                    if ((v2_78 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.units.MagicArmor)) {
                    }
                    if (v2_78 != null) {
                        v2_78.setCurrentHp(Math.max(1, (v2_78.getCurrentHp() + -300)));
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 101, new Object[] {Integer.valueOf(it.paranoidsquirrels.idleguildmaster.R$string.log_hidden_city_of_larox_overdrive)}));
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_24 = v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4604480259023595110);
                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_13 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                        v8_13(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 1, 4607182418800017408);
                        return v7_24.setStatusEffect(v8_13).execute();
                    } else {
                        return 0;
                    }
                }
                v2_78 = 0;
            case 88:
                return v8_6.setTargetSelectionMode("8").setDamageAmplification(4600877379321698714).execute();
            case 89:
                return v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4681608360884174848).setCriticalAmplification(4604119971053405471).execute();
            case 90:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_23 = v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4594572339843380019);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_12 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_12(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 1, (((double) this.localDarkness) * 4576918229304087675));
                return v7_23.setStatusEffect(v8_12).applyEffectOnDodge().execute();
            case 91:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 29, new Object[] {p23, p23}));
                if (this.event == null) {
                    this.event = new it.paranoidsquirrels.idleguildmaster.storage.data.places.Event("summon_smoldering_titan");
                }
                java.util.List v1_203 = this.event;
                v1_203.setProgress(Math.min(100, ((v1_203.getProgress() + 1) + ((int) (it.paranoidsquirrels.idleguildmaster.Utils.random() * 4617315517961601024)))));
                java.util.List v1_205 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v1_205(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, this.acting, 3, 4607182418800017408);
                this.applyStatus(p23, v1_205, 0);
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 113, new Object[] {Integer.valueOf(this.event.getProgress())}));
                return 0;
            case 92:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 115, new Object[] {p23, p23}));
                return 0;
            case 93:
                p23.setCurrentHp(Math.min(p23.calculateTotalMaxHp(), (p23.getCurrentHp() + 10000)));
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 24, new Object[] {Integer.valueOf(0), p23, p23, Integer.valueOf(10000)}));
                return v8_6.setTargetSelectionMode("10").setDamageAmplification(4611686018427387904).execute();
            case 94:
                return v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4602678819172646912).execute();
            case 95:
                return v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4602678819172646912).execute();
            case 96:
                return v8_6.setTargetSelectionMode("lowest_relative_enemy").setDamageAmplification(4621819117588971520).execute();
            case 97:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_3 = v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4587366580439587226);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_0 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_0(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.POISON, p23, 3, 4607182418800017408);
                return v7_3.setStatusEffect(v8_0).applyEffectOnDodge().execute();
            case 98:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_50 = v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4596373779694328218);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_31 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_31(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 4, 4607182418800017408);
                return v7_50.setStatusEffect(v8_31).execute();
            case 99:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_41 = v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4602678819172646912);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_27 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_27(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE, p23, 2, 4607182418800017408);
                return v7_41.setStatusEffect(v8_27).execute();
            case 100:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_36 = v8_6.setTargetSelectionMode("10").setDamageAmplification(4602678819172646912);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_23 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_23(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p23, 4, 4607182418800017408);
                return v7_36.setStatusEffect(v8_23).execute();
            case 101:
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill v7_30 = v8_6.setTargetSelectionMode("all_enemies").setDamageAmplification(4596373779694328218);
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v8_18 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v8_18(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TERRIFY, p23, 1, 4607182418800017408);
                return v7_30.setStatusEffect(v8_18).execute();
            default:
        }
        return v1_193;
    }
```

## `Area.heal(Lit/paranoidsquirrels/idleguildmaster/storage/data/entities/Entity; Lit/paranoidsquirrels/idleguildmaster/storage/data/entities/Entity; Lit/paranoidsquirrels/idleguildmaster/storage/data/places/Area$Skill;)V`

```java
private void heal(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p20, it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p21, it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill p22)
    {
        int v8_0;
        double v4_18 = p20.calculateHealingModifier();
        if (p22 != null) {
            v8_0 = it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$000(p22);
        } else {
            v8_0 = 4607182418800017408;
        }
        int v8_8;
        double v4_0 = (v4_18 * v8_0);
        double v10 = this.calculateCriticalMultiplier(p20, p22, 0);
        double v12_0 = (p20 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer);
        if (v12_0 == 0) {
            v8_8 = 0;
        } else {
            int v15_0 = this.petExploring;
            if ((v15_0 == 0) || ((v10 <= 4607182418800017408) || ((v15_0.getSavage() <= 0) || (it.paranoidsquirrels.idleguildmaster.Utils.random() >= (this.petExploring.getSavage() / 4636737291354636288))))) {
            } else {
                v10 *= v10;
                v8_8 = 1;
            }
        }
        int v9_1 = p20.getIncreaseHealingAgainst();
        if ((v9_1 != 0) && (p21.getTrueClass().equals(v9_1.getKey()))) {
            v4_0 *= ((Double) v9_1.getValue()).doubleValue();
        }
        double v4_21 = Math.max(1, it.paranoidsquirrels.idleguildmaster.Utils.round((((p20.rollAttackDamage() * v10) * v4_0) * 4602678819172646912)));
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v5_6 = p21.getCurrentHp();
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v6_21 = p21.calculateTotalMaxHp();
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v7_5 = Math.min(v6_21, (v5_6 + v4_21));
        p21.setCurrentHp(v7_5);
        if (v12_0 != 0) {
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.medic, ((long) (v7_5 - v5_6)));
        }
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v5_7;
        if (p20.getMaxOverheal() <= 0) {
            v5_7 = 0;
        } else {
            v5_7 = 0;
            p21.setCurrentShield(Math.max(p21.getCurrentShield(), Math.min((p21.getCurrentShield() + Math.max(0, ((v4_21 - v6_21) + v5_6))), it.paranoidsquirrels.idleguildmaster.Utils.round(((((double) v6_21) * 4576918229304087675) * ((double) p20.getMaxOverheal()))))));
        }
        int v13_0;
        if (v8_8 == 0) {
            if (v10 <= 4607182418800017408) {
                v13_0 = v5_7;
            } else {
                v13_0 = 1;
            }
        } else {
            v13_0 = 2;
        }
        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 24, new Object[] {Integer.valueOf(v13_0), p20, p21, Integer.valueOf(v4_21)}));
        if (p20.isCleanser()) {
            if (!(p20 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.units.ChiefScientistAva)) {
                double v4_6 = p21.getNegativeStatusEffects().iterator();
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v6_3 = 0;
                while (v4_6.hasNext()) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v7_3 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v4_6.next());
                    if ((v6_3 == null) || (v6_3.getTurnsLeft() < v7_3.getTurnsLeft())) {
                        v6_3 = v7_3;
                    }
                }
                if (v6_3 != null) {
                    p21.getNegativeStatusEffects().remove(v6_3);
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 10, new Object[] {p21, v6_3.getType()}));
                }
            } else {
                double v4_11 = p21.getNegativeStatusEffects().iterator();
                while (v4_11.hasNext()) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 10, new Object[] {p21, ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v4_11.next()).getType()}));
                }
                p21.getNegativeStatusEffects().clear();
            }
        }
        double v4_14 = p20.onTargetHitEffects().iterator();
        while (v4_14.hasNext()) {
            this.applyStatus(p21, ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v4_14.next()), (p20.calculateIgnoreImmunityToStatus() * 4576918229304087675));
        }
        if ((p22 != null) && (!it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$100(p22))) {
            this.applyStatus(p21, it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$200(p22), (p20.calculateIgnoreImmunityToStatus() * 4576918229304087675));
        }
        return;
    }
```

## `Area.retaliate(Lit/paranoidsquirrels/idleguildmaster/storage/data/entities/Entity; Lit/paranoidsquirrels/idleguildmaster/storage/data/entities/Entity; Z I)V`

```java
private void retaliate(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p19, it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p20, boolean p21, int p22)
    {
        void v0 = this;
        if ((p20.getCurrentHp() > 0) && (!p21)) {
            int v1_8;
            int v12 = p20.calculateRetaliationPhysicalDamage();
            int v13 = (p20.calculateRetaliationMagicalDamage() + p22);
            int v1_7 = this.petExploring;
            if ((v1_7 == 0) || (!(p19 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer))) {
                v1_8 = 0;
            } else {
                v1_8 = v1_7.getBarrier();
            }
            int v14 = v1_8;
            if (v12 > 0) {
                int v1_10 = p19.applyDamage(((double) v12), 0, v14, p20.getArmorIgnored());
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 46, new Object[] {p19, Integer.valueOf(v1_10)}));
                if ((p20 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.spiky, ((long) v1_10));
                }
            }
            long v10;
            if (v13 <= 0) {
                v10 = 1;
            } else {
                if ((p22 > 0) && ((p20 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer))) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.activeDeterrent, 1);
                }
                v10 = 1;
                int v1_18 = p19.applyDamage(((double) it.paranoidsquirrels.idleguildmaster.Utils.round((this.magicDamageAmplification() * ((double) v13)))), 1, v14, p20.getArmorIgnored());
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 46, new Object[] {p19, Integer.valueOf(v1_18)}));
                if ((p20 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.spiky, ((long) v1_18));
                }
            }
            if ((v12 > 0) || (v13 > 0)) {
                this.checkDeath(p19);
            }
            int v1_20 = p20.onSelfHitEffects().iterator();
            while (v1_20.hasNext()) {
                boolean v2_4 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v1_20.next());
                if (v2_4.getType().negative) {
                    this.applyStatus(p19, v2_4, (p20.calculateIgnoreImmunityToStatus() * 4576918229304087675));
                }
            }
            int v1_22;
            int v1_21 = this.petExploring;
            if ((v1_21 == 0) || (!(p20 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer))) {
                v1_22 = 0;
            } else {
                v1_22 = (v1_21.getCounterattack() / 4636737291354636288);
            }
            if ((p19.isForcesTargetToCounterattack()) || (it.paranoidsquirrels.idleguildmaster.Utils.random() < (p20.calculateCounterattackChance() + v1_22))) {
                if ((p20 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.expertDuelist, v10);
                }
                v0 = this.dealDamage(p20, p19, 0, 0);
            }
        }
        int v1_6 = p20.onSelfHitEffects().iterator();
        while (v1_6.hasNext()) {
            boolean v2_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v1_6.next());
            if (!v2_2.getType().negative) {
                v0.applyStatus(p20, v2_2, 0);
            }
        }
        return;
    }
```

## `Area.searchRoom()V`

```java
protected abstract void searchRoom();
```

## `Area.rollEnemies()Ljava/util/List;`

```java
protected abstract java.util.List rollEnemies();
```

## `Area.applyStatus(Lit/paranoidsquirrels/idleguildmaster/storage/data/entities/Entity; Lit/paranoidsquirrels/idleguildmaster/storage/data/entities/StatusEffect; D)V`

```java
protected void applyStatus(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p4, it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect p5, double p6)
    {
        if ((p5 != 0) && (p4 != null)) {
            if (p4.getPassiveSkill() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.PASSIVE_BEND_REALITY) {
                Integer v6_2 = p4.addStatusEffect(p5, p6);
                if (v6_2 > null) {
                    if ((p4 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy)) {
                        if (p5.getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN) {
                            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.shocking, 1);
                        }
                        if (p5.getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE) {
                            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.smokingHot, 1);
                        }
                    }
                    if (v6_2 >= 999) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 12, new Object[] {p4, p5.getType()}));
                    } else {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 11, new Object[] {p4, p5.getType(), Integer.valueOf(v6_2)}));
                    }
                }
            } else {
                if ((p5.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT) && ((p5.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.LESSER_CURSE) && ((p5.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.CURSE) && ((p5.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.GREATER_CURSE) && ((p5.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.OMINOUS_CURSE) && (p5.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABHORRENT_CURSE)))))) {
                    this.applyStatus(p5.getCause(), p5, p6);
                }
                return;
            }
        }
        return;
    }
```

## `Area.healingNova()V`

```java
private void healingNova()
    {
        Object[] v0_5 = this.adventurersExploring.iterator();
        double v3 = 0;
        while (v0_5.hasNext()) {
            int v5_4 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v0_5.next());
            if (v5_4.getCurrentHp() > 0) {
                v3 += (((double) v5_4.getHealMissingHpOnEnemyDeath()) * v5_4.calculateHealingModifier());
            }
        }
        if (v3 != 0) {
            Object[] v0_2 = this.adventurersExploring.iterator();
            while (v0_2.hasNext()) {
                int v1_3 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v0_2.next());
                if (v1_3.getCurrentHp() > 0) {
                    int v2_1 = v1_3.calculateTotalMaxHp();
                    v1_3.setCurrentHp(Math.min(v2_1, (v1_3.getCurrentHp() + it.paranoidsquirrels.idleguildmaster.Utils.round(((4576918229304087675 * v3) * ((double) (v2_1 - v1_3.getCurrentHp())))))));
                }
            }
            it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 117, new Object[] {Integer.valueOf(((int) v3))}));
            return;
        } else {
            return;
        }
    }
```

## `Area.reanimate(Lit/paranoidsquirrels/idleguildmaster/storage/data/entities/enemies/Enemy;)V`

```java
private void reanimate(it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy p20)
    {
        if (!p20.getNegativeStatusEffects().isEmpty()) {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v1_32 = p20.getNegativeStatusEffects().iterator();
            int v2_0 = 0;
            java.util.List v3_0 = 0;
            while(true) {
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v7_4;
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v1_28;
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v5_0 = 0;
                if (!v1_32.hasNext()) {
                    v7_4 = v3_0;
                    v1_28 = 0;
                } else {
                    long v4_10 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v1_32.next());
                    if (v4_10.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABHORRENT_CURSE) {
                        if (v4_10.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.OMINOUS_CURSE) {
                            if (v4_10.getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.GREATER_CURSE) {
                                break;
                            }
                            if (v4_10.getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.CURSE) {
                                v2_0 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v4_10.getCause());
                                v3_0 = "Skeleton";
                            }
                            if ((v4_10.getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.LESSER_CURSE) && (v3_0 == null)) {
                                v2_0 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v4_10.getCause());
                                v3_0 = "Zombie";
                            }
                        } else {
                            v2_0 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v4_10.getCause());
                            v7_4 = "BoneNightmare";
                            v1_28 = 0;
                            v5_0 = 1;
                        }
                    } else {
                        v2_0 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v4_10.getCause());
                        v7_4 = "BoneHydra";
                        v1_28 = 1;
                        v5_0 = 1;
                    }
                }
                if ((v7_4 != null) && ((v2_0 != 0) && (v2_0.getCurrentHp() > 0))) {
                    java.util.List v3_6 = this.adventurersExploring.iterator();
                    while (v3_6.hasNext()) {
                        long v4_13 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v3_6.next());
                        if (v4_13.isSummonedMinion()) {
                            this.adventurersExploring.remove(v4_13);
                            this.fightingGroup.remove(v4_13);
                            break;
                        }
                    }
                    long v4_0;
                    java.util.List v3_1 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer.getInstance(v7_4, -100, 1, 0, 0, 0, 0, 0, 0, new it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.PotionsDrank(), 0, 0);
                    if (v5_0 == null) {
                        v4_0 = "DecomposedLimb";
                    } else {
                        v4_0 = "SerpentJaws";
                    }
                    v3_1.setWeapon(((it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Weapon) it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Weapon.getInstance(v4_0)));
                    if (v1_28 != null) {
                        v3_1.setArmor(((it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Armor) it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Armor.getInstance("SpikedSkeleton")));
                    }
                    if ("WickedScepter".equals(v2_0.getWeapon().getTrueClass())) {
                        v3_1.setAccessory(((it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Accessory) it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Accessory.getInstance("EyeOfUr")));
                    }
                    if ("CursedScepter".equals(v2_0.getWeapon().getTrueClass())) {
                        v3_1.setAccessory(((it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Accessory) it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Accessory.getInstance("AncientEye")));
                    }
                    v3_1.setCurrentHp(v3_1.calculateTotalMaxHp());
                    v2_0.setMinionBound(v3_1);
                    if ((v2_0.getAccessory() != null) && ((v2_0.getAccessory() instanceof it.paranoidsquirrels.idleguildmaster.storage.data.items.instances.SkeletonKey))) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v1_19 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                        v1_19(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.SKELETON_KEY, v2_0, 999, 4607182418800017408);
                        this.applyStatus(v3_1, v1_19, 0);
                    }
                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect v1_20 = this.fightingGroup;
                    v1_20.add((v1_20.indexOf(v2_0) + 1), v3_1);
                    this.adventurersExploring.add(v3_1);
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 39, new Object[] {Integer.valueOf(v3_1.getIdName()), Integer.valueOf(v2_0.getIdName())}));
                }
                return;
            }
            v2_0 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v4_10.getCause());
            v3_0 = "BoneHorror";
        } else {
            return;
        }
    }
```

## `Area.petAttack()V`

```java
private void petAttack()
    {
        if ((this.petExploring != null) && ((!this.enemies.isEmpty()) && (((this.acting instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) && ((this.petExploring.getFighter() > 0) && (!this.turnEndRequested))))) {
            int v0_7 = (this.petExploring.getFighter() * ((((double) this.acting.getLivingCompanionBonusDamage()) * 4576918229304087675) + 4607182418800017408));
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v2_6 = this.selectPetTarget();
            if (v2_6 != null) {
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 105, new Object[] {Integer.valueOf(it.paranoidsquirrels.idleguildmaster.R$string.log_damage_dealt), this.petExploring, v2_6, Integer.valueOf(v2_6.applyDamage(((double) it.paranoidsquirrels.idleguildmaster.Utils.round(Math.max(4607182418800017408, ((4606281698874543309 * v0_7) + ((it.paranoidsquirrels.idleguildmaster.Utils.random() * v0_7) * 4596373779694328218))))), 0, 0, 0))}));
                this.checkDeath(v2_6);
                this.retaliate(0, v2_6, 1, 0);
            }
        }
        return;
    }
```

## `Area.petHeal()V`

```java
private void petHeal()
    {
        Object[] v0_0 = this.petExploring;
        if ((v0_0 != null) && (((this.acting instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) && ((v0_0.getHealer() > 0) && (!this.turnEndRequested)))) {
            Object[] v0_3 = this.petExploring.getHealer();
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v2_0 = this.selectPetHealingTarget();
            if ((v2_0 != null) && (v2_0.getCurrentHp() < v2_0.calculateTotalMaxHp())) {
                Object[] v0_7 = it.paranoidsquirrels.idleguildmaster.Utils.round(Math.max(4607182418800017408, ((4606281698874543309 * v0_3) + ((it.paranoidsquirrels.idleguildmaster.Utils.random() * v0_3) * 4596373779694328218))));
                int v1_2 = v2_0.getCurrentHp();
                int v3_5 = Math.min(v2_0.calculateTotalMaxHp(), (v1_2 + v0_7));
                v2_0.setCurrentHp(v3_5);
                it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.medic, ((long) (v3_5 - v1_2)));
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 106, new Object[] {this.petExploring, v2_0, Integer.valueOf(v0_7)}));
            }
        }
        return;
    }
```

## `Area.petCast()V`

```java
private void petCast()
    {
        if ((this.acting instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v0_8 = this.petExploring;
            if ((v0_8 != null) && ((v0_8.getStatusEffectChance() > 0) && (!this.turnEndRequested))) {
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v0_16;
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v0_3 = it.paranoidsquirrels.idleguildmaster.Utils.random();
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v5 = 0;
                if (v0_3 >= 4591870180066957722) {
                    if (v0_3 >= 4596373779694328218) {
                        if (v0_3 >= 4599075939470750516) {
                            if (v0_3 >= 4600877379321698714) {
                                if (v0_3 >= 4602678819172646912) {
                                    if (v0_3 >= 4603579539098121012) {
                                        if (v0_3 >= 4604480259023595111) {
                                            if (v0_3 >= 4605380978949069210) {
                                                if (v0_3 >= 4606281698874543309) {
                                                    if (this.petExploring.getStatusEffectChance() >= 4624633867356078080) {
                                                        if (this.petExploring.getStatusEffectChance() >= 4629137466983448576) {
                                                            v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.GREATER_CURSE;
                                                        } else {
                                                            v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.CURSE;
                                                        }
                                                    } else {
                                                        v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.LESSER_CURSE;
                                                    }
                                                    java.util.Iterator v1_13 = this.adventurersExploring.iterator();
                                                    while (v1_13.hasNext()) {
                                                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_23 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v1_13.next());
                                                        if ((v4_23.getCurrentHp() > 0) && ((v5 == null) || (v4_23.calculateTotalIntelligence() > v5.calculateTotalIntelligence()))) {
                                                            v5 = v4_23;
                                                        }
                                                    }
                                                } else {
                                                    v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.FROZEN;
                                                }
                                            } else {
                                                v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.BLEED;
                                            }
                                        } else {
                                            v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.REGENERATION;
                                        }
                                    } else {
                                        v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.POISON;
                                    }
                                } else {
                                    v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ABLAZE;
                                }
                            } else {
                                v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.SILENCE;
                            }
                        } else {
                            v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN;
                        }
                    } else {
                        v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.DEFENSIVE_STANCE;
                    }
                } else {
                    v0_16 = it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.TAUNT;
                    java.util.Iterator v1_15 = this.adventurersExploring.iterator();
                    while (v1_15.hasNext()) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v4_13 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v1_15.next());
                        if ((v4_13.getCurrentHp() > 0) && ((v5 == null) || (v4_13.getThreat() > v5.getThreat()))) {
                            v5 = v4_13;
                        }
                        return;
                    }
                }
                java.util.Iterator v1_5;
                Integer v2_0;
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v0_19 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                v0_19(v0_16, v5, this.petExploring.getStatusEffectTurns(), (this.petExploring.getStatusEffectChance() / 4636737291354636288));
                if (!v0_19.getType().negative) {
                    java.util.Iterator v1_3 = new java.util.ArrayList(this.adventurersExploring);
                    v1_3.removeIf(new it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$$ExternalSyntheticLambda1());
                    if (!v1_3.isEmpty()) {
                        v1_5 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) v1_3.get(((int) (it.paranoidsquirrels.idleguildmaster.Utils.random() * ((double) v1_3.size())))));
                        v2_0 = v1_5.addStatusEffect(v0_19, 0);
                    } else {
                        return;
                    }
                } else {
                    if (!this.enemies.isEmpty()) {
                        v1_5 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity) this.enemies.get(((int) (it.paranoidsquirrels.idleguildmaster.Utils.random() * ((double) this.enemies.size())))));
                        v2_0 = v1_5.addStatusEffect(v0_19, 0);
                    } else {
                        return;
                    }
                }
                if (v2_0 > null) {
                    if (v2_0 >= 999) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 12, new Object[] {v1_5, v0_19.getType()}));
                    } else {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 11, new Object[] {v1_5, v0_19.getType(), Integer.valueOf(v2_0)}));
                    }
                }
            }
        }
        return;
    }
```

## `Area.petExecution()V`

```java
private void petExecution()
    {
        java.util.Iterator v0_0 = this.petExploring;
        if ((v0_0 != null) && ((v0_0.getOpportunist() > 0) && (!this.enemies.isEmpty()))) {
            java.util.Iterator v0_2 = this.enemies.iterator();
            while (v0_2.hasNext()) {
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy v1_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy) v0_2.next());
                if ((v1_2.getCurrentHp() > 0) && ((((double) v1_2.getCurrentHp()) / ((double) v1_2.calculateTotalMaxHp())) < (this.petExploring.getOpportunist() / 4636737291354636288))) {
                    v1_2.setCurrentHp(0);
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 108, new Object[] {v1_2, this.petExploring}));
                    this.checkDeath(v1_2);
                }
            }
        }
        return;
    }
```

## `Utils.rollFromWeightedMap(Ljava/util/Map;)Ljava/lang/Object;`

```java
public static Object rollFromWeightedMap(java.util.Map p7)
    {
        if ((p7 != null) && (!p7.isEmpty())) {
            double v1_2 = (it.paranoidsquirrels.idleguildmaster.Utils.random() * 4652007308841189376);
            Object v7_2 = p7.entrySet().iterator();
            int v3_0 = 0;
            while (v7_2.hasNext()) {
                java.util.Map$Entry v4_2 = ((java.util.Map$Entry) v7_2.next());
                v3_0 += ((Integer) v4_2.getValue()).intValue();
                if (v1_2 < ((double) v3_0)) {
                    return v4_2.getKey();
                }
            }
        }
        return 0;
    }
```

## `Utils.round(D)I`

```java
public static int round(double p2)
    {
        return ((int) (p2 + 4547007122018943789));
    }
```

## `Utils.random()D`

```java
public static double random()
    {
        return it.paranoidsquirrels.idleguildmaster.Utils.randomGenerator.nextDouble();
    }
```

## `Utils.calculateNewAdventurerId()I`

```java
public static int calculateNewAdventurerId()
    {
        java.util.Iterator v0_2 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAdventurers().iterator();
        int v1_0 = -1;
        while (v0_2.hasNext()) {
            it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer v2_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v0_2.next());
            if (v2_1.getId() > v1_0) {
                v1_0 = v2_1.getId();
            }
        }
        return (v1_0 + 1);
    }
```

## `Utils.collectDrops(Landroidx/fragment/app/Fragment; Lit/paranoidsquirrels/idleguildmaster/storage/data/places/Area;)V`

```java
public static void collectDrops(androidx.fragment.app.Fragment p7, it.paranoidsquirrels.idleguildmaster.storage.data.places.Area p8)
    {
        android.content.Context v0_1 = new java.util.ArrayList();
        Integer v1_8 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getPets().iterator();
        while (v1_8.hasNext()) {
            int v2_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.pets.Pet) v1_8.next());
            if (v2_2.isFavourite()) {
                v0_1.add(v2_2);
            }
        }
        String v3_1;
        Integer v1_2 = v0_1.size();
        if (v1_2 <= null) {
            v3_1 = 0;
        } else {
            v3_1 = 1;
        }
        int v5_3 = new it.paranoidsquirrels.idleguildmaster.storage.data.items.Item[0];
        String v3_3 = it.paranoidsquirrels.idleguildmaster.Utils.remainingInventorySpaceAfterCollecting(v3_1, ((it.paranoidsquirrels.idleguildmaster.storage.data.items.Item[]) p8.getDrops().toArray(v5_3)));
        if (v3_3 >= null) {
            String v3_5 = new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogCollectDrops();
            v3_5.setCancelable(0);
            v3_5.drops = new java.util.ArrayList(p8.getDrops());
            v3_5.sourceArea = p7.getString(p8.getName());
            v3_5.recap = p8.getAdventureRecap();
            p8.setAdventureRecap(new it.paranoidsquirrels.idleguildmaster.storage.data.places.AdventureRecap());
            v3_5.show(p7.getParentFragmentManager(), "dialog_collect_drops");
            String v3_7 = p8.getDrops().iterator();
            int v4_0 = 0;
            while (v3_7.hasNext()) {
                int v5_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.items.Item) v3_7.next());
                if ((v1_2 <= null) || (!(v5_1 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Food))) {
                    it.paranoidsquirrels.idleguildmaster.Utils.collectItem(v5_1, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getItems());
                } else {
                    v4_0 += (((it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Food) v5_1).getFeedPower() * v5_1.getStack());
                }
            }
            if (v1_2 > null) {
                int v4_12 = (v4_0 / v1_2);
                android.content.Context v0_4 = v0_1.iterator();
                while (v0_4.hasNext()) {
                    ((it.paranoidsquirrels.idleguildmaster.storage.data.pets.Pet) v0_4.next()).feed(v4_12);
                }
            }
            p8.getDrops().clear();
            p8.refreshLoot();
            if ((p8.getAreaType() == 2) && (p8.completed())) {
                ((it.paranoidsquirrels.idleguildmaster.ui.raids.RaidsFragment) p7).refreshRaidVisibility();
                it.paranoidsquirrels.idleguildmaster.UIUtils.getInfoDialog(p7.getContext(), Integer.valueOf(it.paranoidsquirrels.idleguildmaster.R$string.epic_raid_completed_title), String.format(p7.getString(it.paranoidsquirrels.idleguildmaster.R$string.epic_raid_completed_body), new Object[] {p7.getString(p8.getName())})), 0).show();
            }
            it.paranoidsquirrels.idleguildmaster.MainActivity.headquartersFragment.refresh();
            return;
        } else {
            if (it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogFullStorage == null) {
                it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogFullStorage = it.paranoidsquirrels.idleguildmaster.UIUtils.getInfoDialog(p7.getContext(), Integer.valueOf(it.paranoidsquirrels.idleguildmaster.R$string.no_storage_space_title), String.format(p7.getString(it.paranoidsquirrels.idleguildmaster.R$string.no_storage_space_body_loot), new Object[] {Integer.valueOf((v3_3 * -1))})), 0);
                it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogFullStorage.setOnDismissListener(new it.paranoidsquirrels.idleguildmaster.Utils$$ExternalSyntheticLambda4());
                it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogFullStorage.show();
                return;
            } else {
                return;
            }
        }
    }
```
