# S6.5A-001D — DAD Decompile: Quest
**Tool:** androguard 4.1.4 (DAD decompiler) · **Nguồn:** DEX từ `it.paranoidsquirrels.idleguildmaster.apk` (XAPK v2.147)
> Hằng số double/float in ra dạng **bit-pattern long thô**. Giải mã: `struct.unpack('<d', struct.pack('<Q', bits))`.
> Mọi đoạn DAD render nghi ngờ phải đối chiếu file smali tương ứng.

---

## `QuestsManager.extractQuests`

```java
public static void extractQuests()
    {
        it.paranoidsquirrels.idleguildmaster.MainActivity v0_0 = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.calculateDifficulty();
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.clearQuests(v0_0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.setupDoctrineAmounts();
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.setupAccessibleQuests(v0_0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractAllQuests();
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.QUEST_NOTIFICATION = 1;
        ((it.paranoidsquirrels.idleguildmaster.MainActivity) it.paranoidsquirrels.idleguildmaster.MainActivity.dungeonsFragment.getActivity()).refreshIcons();
        return;
    }
```

## `QuestsManager.calculateDifficulty`

```java
public static int calculateDifficulty()
    {
        java.util.Iterator v0_1 = it.paranoidsquirrels.idleguildmaster.Utils.compileDungeonList().iterator();
        int v1 = 0;
        while ((v0_1.hasNext()) && (((it.paranoidsquirrels.idleguildmaster.storage.data.places.Area) v0_1.next()).isUnlocked())) {
            v1++;
        }
        return v1;
    }
```

## `QuestsManager.rollRarity`

```java
private static int rollRarity()
    {
        int v0_0 = it.paranoidsquirrels.idleguildmaster.Utils.random();
        if (v0_0 >= 4604480259023595110) {
            if (v0_0 >= 4606281698874543309) {
                if (v0_0 >= 4606912202822375178) {
                    return 4;
                } else {
                    return 3;
                }
            } else {
                return 2;
            }
        } else {
            return 1;
        }
    }
```

## `QuestsManager.setupAccessibleQuests`

```java
private static void setupAccessibleQuests(int p12)
    {
        java.util.List v1_5 = new it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest[56];
        v1_5[0] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.activeDeterrent;
        v1_5[1] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.andStayDead;
        v1_5[2] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.annihilator;
        v1_5[3] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.botchedRitual;
        v1_5[4] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.clashOfTitans;
        v1_5[5] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.conqueror;
        v1_5[6] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.coupDEtat;
        v1_5[7] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.criticalHit;
        v1_5[8] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.crystalClear;
        v1_5[9] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.darknessWithin;
        v1_5[10] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.delirious;
        v1_5[11] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.eldritchHorror;
        v1_5[12] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.endlessAgony;
        v1_5[13] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.exorcism;
        v1_5[14] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.expertDuelist;
        v1_5[15] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fallingApart;
        v1_5[16] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fastLearner;
        v1_5[17] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fromHell;
        v1_5[18] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.godFeared;
        v1_5[19] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.heavyArmor;
        v1_5[20] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.hitOrMiss;
        v1_5[21] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.iceBreaker;
        v1_5[22] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.innocence;
        v1_5[23] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.itsATrap;
        v1_5[24] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.laroxianPower;
        v1_5[25] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.lightBringer;
        v1_5[26] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.longMarch;
        v1_5[27] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.luckyRoll;
        v1_5[28] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.marathon;
        v1_5[29] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.masterCrafter;
        v1_5[30] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.medic;
        v1_5[31] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.miracle;
        v1_5[32] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.myopia;
        v1_5[33] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.niceTry;
        v1_5[34] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.protector;
        v1_5[35] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.psychiatrist;
        v1_5[36] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.pulverization;
        v1_5[37] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.ragingVolcano;
        v1_5[38] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.regicide;
        v1_5[39] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.paleontologist;
        v1_5[40] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.shocking;
        v1_5[41] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.slowBurn;
        v1_5[42] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.smartFighter;
        v1_5[43] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.smokingHot;
        v1_5[44] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.softAndFluffy;
        v1_5[45] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.soothingRemedy;
        v1_5[46] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.speedyHare;
        v1_5[47] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.spiky;
        v1_5[48] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.student;
        v1_5[49] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.tabulaRasa;
        v1_5[50] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.thalassophobia;
        v1_5[51] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.theEnd;
        v1_5[52] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.tormentor;
        v1_5[53] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.unscathed;
        v1_5[54] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.vampiricThirst;
        v1_5[55] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.warrior;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleQuests = new java.util.ArrayList(java.util.Arrays.asList(v1_5));
        java.util.List v1_2 = new it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest[7];
        v1_2[0] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.vampiricThirst;
        v1_2[1] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fallingApart;
        v1_2[2] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.theEnd;
        v1_2[3] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.innocence;
        v1_2[4] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.softAndFluffy;
        v1_2[5] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.tormentor;
        v1_2[6] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.delirious;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleAfflictionQuests = new java.util.ArrayList(java.util.Arrays.asList(v1_2));
        java.util.List v1_4 = new it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest[7];
        v1_4[0] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.smokingHot;
        v1_4[1] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.shocking;
        v1_4[2] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.slowBurn;
        v1_4[3] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.iceBreaker;
        v1_4[4] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.regicide;
        v1_4[5] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.crystalClear;
        v1_4[6] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.laroxianPower;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleControlQuests = new java.util.ArrayList(java.util.Arrays.asList(v1_4));
        java.util.List v1_7 = new it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest[7];
        v1_7[0] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.heavyArmor;
        v1_7[1] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.spiky;
        v1_7[2] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.protector;
        v1_7[3] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.speedyHare;
        v1_7[4] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.clashOfTitans;
        v1_7[5] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.unscathed;
        v1_7[6] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.godFeared;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleFortitudeQuests = new java.util.ArrayList(java.util.Arrays.asList(v1_7));
        java.util.List v1_9 = new it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest[7];
        v1_9[0] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.medic;
        v1_9[1] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.lightBringer;
        v1_9[2] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.soothingRemedy;
        v1_9[3] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.psychiatrist;
        v1_9[4] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.andStayDead;
        v1_9[5] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.miracle;
        v1_9[6] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.darknessWithin;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleGraceQuests = new java.util.ArrayList(java.util.Arrays.asList(v1_9));
        java.util.List v1_11 = new it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest[7];
        v1_11[0] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.hitOrMiss;
        v1_11[1] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.luckyRoll;
        v1_11[2] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.itsATrap;
        v1_11[3] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.niceTry;
        v1_11[4] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.eldritchHorror;
        v1_11[5] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.activeDeterrent;
        v1_11[6] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.marathon;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleIllusionQuests = new java.util.ArrayList(java.util.Arrays.asList(v1_11));
        java.util.List v1_13 = new it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest[7];
        v1_13[0] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.student;
        v1_13[1] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.myopia;
        v1_13[2] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.paleontologist;
        v1_13[3] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.masterCrafter;
        v1_13[4] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fromHell;
        v1_13[5] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fastLearner;
        v1_13[6] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.exorcism;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleKnowledgeQuests = new java.util.ArrayList(java.util.Arrays.asList(v1_13));
        java.util.List v1_15 = new it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest[7];
        v1_15[0] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.annihilator;
        v1_15[1] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.smartFighter;
        v1_15[2] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.criticalHit;
        v1_15[3] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.coupDEtat;
        v1_15[4] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.botchedRitual;
        v1_15[5] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.pulverization;
        v1_15[6] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.thalassophobia;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleRuinQuests = new java.util.ArrayList(java.util.Arrays.asList(v1_15));
        java.util.List v1_17 = new it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest[7];
        v1_17[0] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.expertDuelist;
        v1_17[1] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.warrior;
        v1_17[2] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.longMarch;
        v1_17[3] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.conqueror;
        v1_17[4] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.endlessAgony;
        v1_17[5] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.tabulaRasa;
        v1_17[6] = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.ragingVolcano;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleWarQuests = new java.util.ArrayList(java.util.Arrays.asList(v1_17));
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.prepareList(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleQuests, p12);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.prepareList(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleAfflictionQuests, p12);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.prepareList(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleControlQuests, p12);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.prepareList(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleFortitudeQuests, p12);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.prepareList(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleGraceQuests, p12);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.prepareList(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleIllusionQuests, p12);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.prepareList(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleKnowledgeQuests, p12);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.prepareList(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleRuinQuests, p12);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.prepareList(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleWarQuests, p12);
        return;
    }
```

## `QuestsManager.setupDoctrineAmounts`

```java
private static void setupDoctrineAmounts()
    {
        java.util.Iterator v0_1 = new java.util.HashMap();
        int v1_33 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionLevel();
        int v4_1 = Integer.valueOf(0);
        if (v1_33 < 10) {
            Object vtmp3 = v0_1.put("DoctrineOfAffliction", v4_1);
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlLevel() < 10) {
            Object vtmp5 = v0_1.put("DoctrineOfControl", v4_1);
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeLevel() < 10) {
            Object vtmp7 = v0_1.put("DoctrineOfFortitude", v4_1);
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceLevel() < 10) {
            Object vtmp9 = v0_1.put("DoctrineOfGrace", v4_1);
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionLevel() < 10) {
            Object vtmp11 = v0_1.put("DoctrineOfIllusion", v4_1);
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeLevel() < 10) {
            Object vtmp13 = v0_1.put("DoctrineOfKnowledge", v4_1);
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinLevel() < 10) {
            Object vtmp15 = v0_1.put("DoctrineOfRuin", v4_1);
        }
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarLevel() < 10) {
            Object vtmp17 = v0_1.put("DoctrineOfWar", v4_1);
        }
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountAffliction = 0;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountControl = 0;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountFortitude = 0;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountGrace = 0;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountIllusion = 0;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountKnowledge = 0;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountRuin = 0;
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountWar = 0;
        if (!v0_1.isEmpty()) {
            int v1_43 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAdventurers().iterator();
            int v5_24 = 2;
            while (v1_43.hasNext()) {
                int v15_2;
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.doctrines.Doctrine v13_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) v1_43.next());
                if ((v13_2.getId() >= 0) || (v5_24 <= 0)) {
                    v15_2 = 1;
                } else {
                    v15_2 = 2;
                }
                if (v15_2 > 1) {
                    v5_24--;
                }
                it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.incrementOrAssignToRandom(v0_1, v13_2.getDoctrine(), v15_2);
            }
            java.util.Iterator v0_3 = v0_1.entrySet().iterator();
            while (v0_3.hasNext()) {
                int v1_46 = ((java.util.Map$Entry) v0_3.next());
                int v5_27 = ((String) v1_46.getKey());
                v5_27.hashCode();
                int v15_0 = -1;
                switch (v5_27.hashCode()) {
                    case -2114116535:
                        if (v5_27.equals("DoctrineOfRuin")) {
                            v15_0 = 0;
                        } else {
                        }
                        break;
                    case -1928972419:
                        if (v5_27.equals("DoctrineOfKnowledge")) {
                            v15_0 = 1;
                        } else {
                        }
                        break;
                    case -1459475521:
                        if (v5_27.equals("DoctrineOfFortitude")) {
                            v15_0 = 2;
                        } else {
                        }
                        break;
                    case -1123359177:
                        if (v5_27.equals("DoctrineOfGrace")) {
                            v15_0 = 3;
                        } else {
                        }
                        break;
                    case -1115407422:
                        if (v5_27.equals("DoctrineOfIllusion")) {
                            v15_0 = 4;
                        } else {
                        }
                        break;
                    case -839784420:
                        if (v5_27.equals("DoctrineOfControl")) {
                            v15_0 = 5;
                        } else {
                        }
                        break;
                    case -188986502:
                        if (v5_27.equals("DoctrineOfAffliction")) {
                            v15_0 = 6;
                        } else {
                        }
                        break;
                    case 70354215:
                        if (v5_27.equals("DoctrineOfWar")) {
                            v15_0 = 7;
                        } else {
                        }
                        break;
                    default:
                }
                switch (v15_0) {
                    case 0:
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountRuin = (it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountRuin + ((Integer) v1_46.getValue()).intValue());
                        break;
                    case 1:
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountKnowledge = (it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountKnowledge + ((Integer) v1_46.getValue()).intValue());
                        break;
                    case 2:
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountFortitude = (it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountFortitude + ((Integer) v1_46.getValue()).intValue());
                        break;
                    case 3:
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountGrace = (it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountGrace + ((Integer) v1_46.getValue()).intValue());
                        break;
                    case 4:
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountIllusion = (it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountIllusion + ((Integer) v1_46.getValue()).intValue());
                        break;
                    case 5:
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountControl = (it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountControl + ((Integer) v1_46.getValue()).intValue());
                        break;
                    case 6:
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountAffliction = (it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountAffliction + ((Integer) v1_46.getValue()).intValue());
                        break;
                    case 7:
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountWar = (it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountWar + ((Integer) v1_46.getValue()).intValue());
                        break;
                    default:
                }
            }
            return;
        } else {
            return;
        }
    }
```

## `QuestsManager.increment`

```java
public static void increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest p4, long p5)
    {
        if ((p4 != 0) && ((p4.isActive()) && (p5 > 0))) {
            long v0_4 = p4.getProgress();
            if (v0_4 < p4.targetProgress) {
                long v0_0 = (v0_4 + p5);
                p4.setProgress(v0_0);
                if (v0_0 >= p4.targetProgress) {
                    it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.QUEST_COMPLETED_RECENTLY = 1;
                }
            }
        }
        return;
    }
```

## `QuestsManager.incrementToValue`

```java
public static void incrementToValue(it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest p4, long p5)
    {
        if ((p4 != 0) && ((p4.isActive()) && ((p5 > 0) && (p4.getProgress() < p4.targetProgress)))) {
            p4.setProgress(p5);
            if (p5 >= p4.targetProgress) {
                it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.QUEST_COMPLETED_RECENTLY = 1;
            }
        }
        return;
    }
```

## `QuestsManager.realignQuests`

```java
public static void realignQuests()
    {
        java.util.Iterator v0_13 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKingsQuests().iterator();
        while (v0_13.hasNext()) {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_13.next()).realignStaticReference();
        }
        java.util.Iterator v0_2 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionQuests().iterator();
        while (v0_2.hasNext()) {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_2.next()).realignStaticReference();
        }
        java.util.Iterator v0_6 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlQuests().iterator();
        while (v0_6.hasNext()) {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_6.next()).realignStaticReference();
        }
        java.util.Iterator v0_9 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeQuests().iterator();
        while (v0_9.hasNext()) {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_9.next()).realignStaticReference();
        }
        java.util.Iterator v0_12 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceQuests().iterator();
        while (v0_12.hasNext()) {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_12.next()).realignStaticReference();
        }
        java.util.Iterator v0_16 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionQuests().iterator();
        while (v0_16.hasNext()) {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_16.next()).realignStaticReference();
        }
        java.util.Iterator v0_19 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeQuests().iterator();
        while (v0_19.hasNext()) {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_19.next()).realignStaticReference();
        }
        java.util.Iterator v0_22 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinQuests().iterator();
        while (v0_22.hasNext()) {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_22.next()).realignStaticReference();
        }
        java.util.Iterator v0_25 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarQuests().iterator();
        while (v0_25.hasNext()) {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_25.next()).realignStaticReference();
        }
        return;
    }
```

## `QuestsManager.clearQuests`

```java
private static void clearQuests(int p1)
    {
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKingsQuests().clear();
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionQuests().clear();
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlQuests().clear();
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeQuests().clear();
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceQuests().clear();
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionQuests().clear();
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeQuests().clear();
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinQuests().clear();
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarQuests().clear();
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.initializeFields(p1);
        return;
    }
```

## `QuestsManager.initializeFields`

```java
public static void initializeFields(int p2)
    {
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.activeDeterrent = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("ActiveDeterrent", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.andStayDead = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("AndStayDead", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.annihilator = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Annihilator", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.botchedRitual = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("BotchedRitual", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.clashOfTitans = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("ClashOfTitans", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.conqueror = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Conqueror", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.coupDEtat = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("CoupDEtat", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.criticalHit = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("CriticalHit", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.crystalClear = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("CrystalClear", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.darknessWithin = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("DarknessWithin", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.delirious = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Delirious", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.eldritchHorror = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("EldritchHorror", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.endlessAgony = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("EndlessAgony", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.exorcism = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Exorcism", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.expertDuelist = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("ExpertDuelist", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fallingApart = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("FallingApart", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fastLearner = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("FastLearner", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.fromHell = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("FromHell", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.godFeared = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("GodFeared", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.heavyArmor = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("HeavyArmor", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.hitOrMiss = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("HitOrMiss", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.iceBreaker = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("IceBreaker", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.innocence = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Innocence", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.itsATrap = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("ItsATrap", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.laroxianPower = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("LaroxianPower", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.lightBringer = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("LightBringer", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.longMarch = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("LongMarch", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.luckyRoll = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("LuckyRoll", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.marathon = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Marathon", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.masterCrafter = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("MasterCrafter", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.medic = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Medic", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.miracle = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Miracle", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.myopia = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Myopia", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.niceTry = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("NiceTry", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.protector = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Protector", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.psychiatrist = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Psychiatrist", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.pulverization = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Pulverization", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.ragingVolcano = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("RagingVolcano", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.regicide = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Regicide", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.paleontologist = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Paleontologist", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.shocking = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Shocking", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.slowBurn = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("SlowBurn", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.smartFighter = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("SmartFighter", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.smokingHot = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("SmokingHot", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.softAndFluffy = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("SoftAndFluffy", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.soothingRemedy = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("SoothingRemedy", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.speedyHare = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("SpeedyHare", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.spiky = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Spiky", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.student = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Student", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.tabulaRasa = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("TabulaRasa", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.thalassophobia = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Thalassophobia", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.theEnd = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("TheEnd", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.tormentor = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Tormentor", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.unscathed = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Unscathed", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.vampiricThirst = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("VampiricThirst", 0, p2, 0);
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.warrior = it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest.createInstance("Warrior", 0, p2, 0);
        return;
    }
```

## `QuestsManager.extractAllQuests`

```java
private static void extractAllQuests()
    {
        if (!it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAdventurers().isEmpty()) {
            it.paranoidsquirrels.idleguildmaster.MainActivity.data.setQuestsSeen(1);
            it.paranoidsquirrels.idleguildmaster.MainActivity.data.setQuestsRefreshed(0);
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractSpecificQuests(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountAffliction, it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleAfflictionQuests, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionQuests());
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractSpecificQuests(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountControl, it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleControlQuests, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlQuests());
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractSpecificQuests(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountFortitude, it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleFortitudeQuests, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeQuests());
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractSpecificQuests(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountGrace, it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleGraceQuests, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceQuests());
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractSpecificQuests(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountIllusion, it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleIllusionQuests, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionQuests());
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractSpecificQuests(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountKnowledge, it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleKnowledgeQuests, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeQuests());
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractSpecificQuests(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountRuin, it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleRuinQuests, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinQuests());
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractSpecificQuests(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountWar, it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleWarQuests, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarQuests());
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.extractSpecificQuests(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.amountGeneral, it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleQuests, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKingsQuests());
            return;
        } else {
            return;
        }
    }
```

## `QuestsManager.extractSpecificQuests`

```java
private static void extractSpecificQuests(int p5, java.util.List p6, java.util.List p7)
    {
        int v1 = 0;
        while (v1 < p5) {
            int v2 = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.rollRarity();
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v3_1 = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.getFromListByRarity(p6, v2);
            if (v3_1 == null) {
                if (p6 != it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKingsQuests()) {
                    v3_1 = it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.getFromListByRarity(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleQuests, v2);
                }
                if (v3_1 == null) {
                    if (it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleQuests.size() <= 0) {
                        break;
                    }
                    v3_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.accessibleQuests.get(0));
                }
            }
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.removeFromAllLists(v3_1);
            if (v3_1.cannotAppearWith() != null) {
                it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.removeFromAllLists(v3_1.cannotAppearWith());
            }
            v3_1.setRarity(v2);
            v3_1.activate();
            p7.add(v3_1);
            v1++;
        }
        p7.sort(new it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager$$ExternalSyntheticLambda1());
        return;
    }
```

## `QuestsManager.getFromListByRarity`

```java
private static it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest getFromListByRarity(java.util.List p2, int p3)
    {
        int v2_1 = p2.iterator();
        while (v2_1.hasNext()) {
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v0_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v2_1.next());
            if (v0_2.defaultRarity == p3) {
                return v0_2;
            }
        }
        return 0;
    }
```
