# S6.5A-001E — DialogQuests (Quest CLAIM rule)

**Class:** `Lit/paranoidsquirrels/idleguildmaster/ui/dialogs/DialogQuests;`  ·  **DEX:** classes3.dex  ·  **Tool:** androguard 4.1.4 DAD

---

## `formatStars(I Z)Ljava/lang/String;`

```java
private String formatStars(int p4, boolean p5)
    {
        StringBuilder v0_1 = new StringBuilder();
        int v1 = 0;
        while (v1 < p4) {
            if ((p5) && ((v1 == 2) || (v1 == 4))) {
                v0_1.append("\n");
            }
            v0_1.append("\u2605");
            v1++;
        }
        return v0_1.toString();
    }
```

## `notificationValue()Z`

```java
private boolean notificationValue()
    {
        int v0_13 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKingsQuests().iterator();
        while (v0_13.hasNext()) {
            long v1_18 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_13.next());
            if (v1_18.getProgress() >= v1_18.getTargetProgress()) {
                return 1;
            }
        }
        int v0_2 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionQuests().iterator();
        while (v0_2.hasNext()) {
            long v1_14 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_2.next());
            if (v1_14.getProgress() >= v1_14.getTargetProgress()) {
                return 1;
            }
        }
        int v0_6 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlQuests().iterator();
        while (v0_6.hasNext()) {
            long v1_11 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_6.next());
            if (v1_11.getProgress() >= v1_11.getTargetProgress()) {
                return 1;
            }
        }
        int v0_9 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeQuests().iterator();
        while (v0_9.hasNext()) {
            long v1_8 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_9.next());
            if (v1_8.getProgress() >= v1_8.getTargetProgress()) {
                return 1;
            }
        }
        int v0_12 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceQuests().iterator();
        while (v0_12.hasNext()) {
            long v1_5 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_12.next());
            if (v1_5.getProgress() >= v1_5.getTargetProgress()) {
                return 1;
            }
        }
        int v0_16 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionQuests().iterator();
        while (v0_16.hasNext()) {
            long v1_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_16.next());
            if (v1_2.getProgress() >= v1_2.getTargetProgress()) {
                return 1;
            }
        }
        int v0_19 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeQuests().iterator();
        while (v0_19.hasNext()) {
            long v1_35 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_19.next());
            if (v1_35.getProgress() >= v1_35.getTargetProgress()) {
                return 1;
            }
        }
        int v0_22 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinQuests().iterator();
        while (v0_22.hasNext()) {
            long v1_32 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_22.next());
            if (v1_32.getProgress() >= v1_32.getTargetProgress()) {
                return 1;
            }
        }
        int v0_25 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarQuests().iterator();
        while (v0_25.hasNext()) {
            long v1_29 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v0_25.next());
            if (v1_29.getProgress() >= v1_29.getTargetProgress()) {
                return 1;
            }
        }
        return 0;
    }
```

## `refreshLpInfo()V`

```java
private void refreshLpInfo()
    {
        this.setupLpInfo(this.binding.afflictionLpBonus, this.binding.afflictionProgress, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionLevel(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionProgress());
        this.setupLpInfo(this.binding.controlLpBonus, this.binding.controlProgress, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlLevel(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlProgress());
        this.setupLpInfo(this.binding.fortitudeLpBonus, this.binding.fortitudeProgress, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeLevel(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeProgress());
        this.setupLpInfo(this.binding.graceLpBonus, this.binding.graceProgress, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceLevel(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceProgress());
        this.setupLpInfo(this.binding.illusionLpBonus, this.binding.illusionProgress, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionLevel(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionProgress());
        this.setupLpInfo(this.binding.knowledgeLpBonus, this.binding.knowledgeProgress, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeLevel(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeProgress());
        this.setupLpInfo(this.binding.ruinLpBonus, this.binding.ruinProgress, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinLevel(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinProgress());
        this.setupLpInfo(this.binding.warLpBonus, this.binding.warProgress, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarLevel(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarProgress());
        return;
    }
```

## `rewardFromRarity(I Z)I`

```java
private int rewardFromRarity(int p3, boolean p4)
    {
        int v0 = 1;
        if (p3 == 1) {
            if (p4) {
                v0 = 10;
            }
            return v0;
        } else {
            int v1_2 = 2;
            if (p3 == 2) {
                if (p4) {
                    v1_2 = 20;
                }
                return v1_2;
            } else {
                int v1_0 = 3;
                if (p3 == 3) {
                    if (p4) {
                        v1_0 = 40;
                    }
                    return v1_0;
                } else {
                    if (p3 == 4) {
                        int v3_1;
                        if (!p4) {
                            v3_1 = 5;
                        } else {
                            v3_1 = 100;
                        }
                        return v3_1;
                    } else {
                        return 1;
                    }
                }
            }
        }
    }
```

## `setupLpInfo(Landroid/widget/TextView; Landroid/widget/TextView; I I)V`

```java
private void setupLpInfo(android.widget.TextView p3, android.widget.TextView p4, int p5, int p6)
    {
        android.content.res.Resources$Theme v0_1;
        if (p5 != 0) {
            v0_1 = String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.dialog_quests_lp_formatted), new Object[] {Integer.valueOf(p5)}));
        } else {
            v0_1 = "";
        }
        p3.setText(v0_1);
        p4.setText(String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.dialog_quests_progress_formatted), new Object[] {Integer.valueOf((it.paranoidsquirrels.idleguildmaster.Formulas.totalStarsToNextLp(p5) - p6))})));
        if (p5 >= 10) {
            p3.setTextColor(this.getResources().getColor(it.paranoidsquirrels.idleguildmaster.R$color.ascended_unit, this.getContext().getTheme()));
            p4.setTextColor(this.getResources().getColor(it.paranoidsquirrels.idleguildmaster.R$color.ascended_unit, this.getContext().getTheme()));
            p4.setText(it.paranoidsquirrels.idleguildmaster.R$string.max);
        }
        return;
    }
```

## `setupQuests(Landroidx/constraintlayout/widget/ConstraintLayout; Landroid/widget/LinearLayout; Ljava/util/List;)V`

```java
private void setupQuests(androidx.constraintlayout.widget.ConstraintLayout p17, android.widget.LinearLayout p18, java.util.List p19)
    {
        int v12;
        int v10_1 = 1;
        int v11_1 = 0;
        if (p17 != this.binding.containerKingsQuests) {
            v12 = 0;
        } else {
            v12 = 1;
        }
        it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogQuests v1_7;
        if (p19.size() <= 0) {
            v1_7 = 0;
        } else {
            v1_7 = 1;
        }
        int v2_5;
        if (v1_7 == null) {
            v2_5 = 8;
        } else {
            v2_5 = 0;
        }
        androidx.constraintlayout.widget.ConstraintLayout v0_8;
        p17.setVisibility(v2_5);
        if (v1_7 == null) {
            v0_8 = 8;
        } else {
            v0_8 = 0;
        }
        p18.setVisibility(v0_8);
        p18.removeAllViews();
        java.util.Iterator v14 = p19.iterator();
        while (v14.hasNext()) {
            it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogQuests v1_25;
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v7_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v14.next());
            it.paranoidsquirrels.idleguildmaster.databinding.LayoutQuestBinding v15 = it.paranoidsquirrels.idleguildmaster.databinding.LayoutQuestBinding.inflate(this.getLayoutInflater(), p18, v11_1);
            this.updateList.put(v15.questProgress, v7_1);
            v15.questName.setText(this.getString(v7_1.getIdName()));
            v15.questDescription.setText(String.format(this.getString(v7_1.getIdDescription()), new Object[] {Long.valueOf(v7_1.getTargetProgress())})));
            v15.questProgress.setProgress(Math.round((Math.min(1065353216, ((float) (((double) v7_1.getProgress()) / ((double) v7_1.getTargetProgress())))) * 1120403456)));
            int v3_2 = this.rewardFromRarity(v7_1.getRarity(), v12);
            androidx.constraintlayout.widget.ConstraintLayout v0_17 = v15.questReward;
            if (v12 == 0) {
                v1_25 = this.formatStars(v3_2, v11_1);
            } else {
                v1_25 = String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.dialog_quests_gems_formatted), new Object[] {Integer.valueOf(v3_2)}));
            }
            it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogQuests v1_0;
            v0_17.setText(v1_25);
            androidx.constraintlayout.widget.ConstraintLayout v0_18 = v15.questRewardClickableText;
            if (v12 == 0) {
                v1_0 = this.formatStars(v3_2, v10_1);
            } else {
                v1_0 = String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.dialog_quests_gems_formatted), new Object[] {Integer.valueOf(v3_2)}));
            }
            it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogQuests v1_4;
            v0_18.setText(v1_0);
            if (v12 == 0) {
                v1_4 = 8;
            } else {
                v1_4 = v11_1;
            }
            androidx.constraintlayout.widget.ConstraintLayout v0_4;
            v15.questRewardClickableGems.setVisibility(v1_4);
            if (v7_1.getProgress() < v7_1.getTargetProgress()) {
                v0_4 = v11_1;
            } else {
                v0_4 = v10_1;
            }
            int v2_2;
            if (v0_4 == null) {
                v2_2 = 8;
            } else {
                v2_2 = v11_1;
            }
            int v2_3;
            v15.questRewardClickable.setVisibility(v2_2);
            if (v0_4 == null) {
                v2_3 = v11_1;
            } else {
                v2_3 = 8;
            }
            androidx.constraintlayout.widget.ConstraintLayout v0_5;
            v15.questReward.setVisibility(v2_3);
            if ((v12 == 0) || (v0_4 != null)) {
                v0_5 = 8;
            } else {
                v0_5 = v11_1;
            }
            v15.questRewardGems.setVisibility(v0_5);
            android.widget.LinearLayout v5_0 = new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogQuests$$ExternalSyntheticLambda0;
            int v10_0 = v5_0;
            int v11_0 = v15.questRewardClickable;
            v5_0(this, v12, v3_2, p19, p18, v15, v7_1);
            v11_0.setOnClickListener(v10_0);
            p18.addView(v15.getRoot());
            v10_1 = 1;
            v11_1 = 0;
        }
        return;
    }
```

## `attachListeners()V`

```java
protected void attachListeners()
    {
        this.binding.refresh.setOnClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogQuests$$ExternalSyntheticLambda1(this));
        this.binding.close.setOnClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogQuests$$ExternalSyntheticLambda2(this));
        return;
    }
```

## `getTitle()Ljava/lang/String;`

```java
protected String getTitle()
    {
        return this.getString(it.paranoidsquirrels.idleguildmaster.R$string.dialog_quests_title);
    }
```

## `inflate(Landroid/view/LayoutInflater; Landroid/view/ViewGroup; Z)Landroidx/viewbinding/ViewBinding;`

```java
protected androidx.viewbinding.ViewBinding inflate(android.view.LayoutInflater p1, android.view.ViewGroup p2, boolean p3)
    {
        it.paranoidsquirrels.idleguildmaster.databinding.DialogQuestsBinding v1_1 = it.paranoidsquirrels.idleguildmaster.databinding.DialogQuestsBinding.inflate(p1, p2, p3);
        this.binding = v1_1;
        return v1_1;
    }
```

## `initialize(Landroid/os/Bundle;)V`

```java
protected void initialize(android.os.Bundle p4)
    {
        boolean v0_20;
        this.refreshLpInfo();
        this.updateList.clear();
        this.setupQuests(this.binding.containerKingsQuests, this.binding.kingsQuestsList, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKingsQuests());
        this.setupQuests(this.binding.containerAfflictionQuests, this.binding.afflictionQuestsList, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionQuests());
        this.setupQuests(this.binding.containerControlQuests, this.binding.controlQuestsList, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlQuests());
        this.setupQuests(this.binding.containerFortitudeQuests, this.binding.fortitudeQuestsList, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeQuests());
        this.setupQuests(this.binding.containerGraceQuests, this.binding.graceQuestsList, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceQuests());
        this.setupQuests(this.binding.containerIllusionQuests, this.binding.illusionQuestsList, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionQuests());
        this.setupQuests(this.binding.containerKnowledgeQuests, this.binding.knowledgeQuestsList, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeQuests());
        this.setupQuests(this.binding.containerRuinQuests, this.binding.ruinQuestsList, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinQuests());
        this.setupQuests(this.binding.containerWarQuests, this.binding.warQuestsList, it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarQuests());
        int v1_0 = 8;
        if (!this.updateList.isEmpty()) {
            v0_20 = 0;
        } else {
            v0_20 = 8;
        }
        boolean v0_23;
        this.binding.scrollView.setVisibility(v0_20);
        if (!this.updateList.isEmpty()) {
            v0_23 = 8;
        } else {
            v0_23 = 0;
        }
        this.binding.noQuestsMessage.setVisibility(v0_23);
        if (!it.paranoidsquirrels.idleguildmaster.MainActivity.data.isQuestsRefreshed()) {
            v1_0 = 0;
        }
        this.binding.refresh.setVisibility(v1_0);
        return;
    }
```

## `lambda$attachListeners$1$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogQuests(Landroid/view/View;)V`

```java
synthetic void lambda$attachListeners$1$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogQuests(android.view.View p3)
    {
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogRefreshQuests == null) {
            it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogRefreshQuests = new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogRefreshQuests();
            it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogRefreshQuests.show(this.getParentFragmentManager(), "dialog_refresh_quests");
            return;
        } else {
            return;
        }
    }
```

## `lambda$attachListeners$2$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogQuests(Landroid/view/View;)V`

```java
synthetic void lambda$attachListeners$2$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogQuests(android.view.View p1)
    {
        this.dismiss();
        return;
    }
```

## `lambda$setupQuests$0$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogQuests(Z I Ljava/util/List; Landroid/widget/LinearLayout; Lit/paranoidsquirrels/idleguildmaster/databinding/LayoutQuestBinding; Lit/paranoidsquirrels/idleguildmaster/storage/data/quests/Quest; Landroid/view/View;)V`

```java
synthetic void lambda$setupQuests$0$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogQuests(boolean p5, int p6, java.util.List p7, android.widget.LinearLayout p8, it.paranoidsquirrels.idleguildmaster.databinding.LayoutQuestBinding p9, it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest p10, android.view.View p11)
    {
        if (p5 == null) {
            if (p7 != it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionQuests()) {
                if (p7 != it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlQuests()) {
                    if (p7 != it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeQuests()) {
                        if (p7 != it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceQuests()) {
                            if (p7 != it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionQuests()) {
                                if (p7 != it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeQuests()) {
                                    if (p7 != it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinQuests()) {
                                        if (p7 == it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarQuests()) {
                                            it.paranoidsquirrels.idleguildmaster.storage.data.Data v5_55 = (it.paranoidsquirrels.idleguildmaster.Formulas.totalStarsToNextLp(it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarLevel()) - it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarProgress());
                                            if (v5_55 > p6) {
                                                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setWarProgress((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarProgress() + p6));
                                            } else {
                                                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setWarLevel((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getWarLevel() + 1));
                                                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setWarProgress((p6 - v5_55));
                                            }
                                        }
                                    } else {
                                        it.paranoidsquirrels.idleguildmaster.storage.data.Data v5_61 = (it.paranoidsquirrels.idleguildmaster.Formulas.totalStarsToNextLp(it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinLevel()) - it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinProgress());
                                        if (v5_61 > p6) {
                                            it.paranoidsquirrels.idleguildmaster.MainActivity.data.setRuinProgress((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinProgress() + p6));
                                        } else {
                                            it.paranoidsquirrels.idleguildmaster.MainActivity.data.setRuinLevel((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getRuinLevel() + 1));
                                            it.paranoidsquirrels.idleguildmaster.MainActivity.data.setRuinProgress((p6 - v5_61));
                                        }
                                    }
                                } else {
                                    it.paranoidsquirrels.idleguildmaster.storage.data.Data v5_68 = (it.paranoidsquirrels.idleguildmaster.Formulas.totalStarsToNextLp(it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeLevel()) - it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeProgress());
                                    if (v5_68 > p6) {
                                        it.paranoidsquirrels.idleguildmaster.MainActivity.data.setKnowledgeProgress((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeProgress() + p6));
                                    } else {
                                        it.paranoidsquirrels.idleguildmaster.MainActivity.data.setKnowledgeLevel((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnowledgeLevel() + 1));
                                        it.paranoidsquirrels.idleguildmaster.MainActivity.data.setKnowledgeProgress((p6 - v5_68));
                                    }
                                }
                            } else {
                                it.paranoidsquirrels.idleguildmaster.storage.data.Data v5_6 = (it.paranoidsquirrels.idleguildmaster.Formulas.totalStarsToNextLp(it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionLevel()) - it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionProgress());
                                if (v5_6 > p6) {
                                    it.paranoidsquirrels.idleguildmaster.MainActivity.data.setIllusionProgress((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionProgress() + p6));
                                } else {
                                    it.paranoidsquirrels.idleguildmaster.MainActivity.data.setIllusionLevel((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getIllusionLevel() + 1));
                                    it.paranoidsquirrels.idleguildmaster.MainActivity.data.setIllusionProgress((p6 - v5_6));
                                }
                            }
                        } else {
                            it.paranoidsquirrels.idleguildmaster.storage.data.Data v5_12 = (it.paranoidsquirrels.idleguildmaster.Formulas.totalStarsToNextLp(it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceLevel()) - it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceProgress());
                            if (v5_12 > p6) {
                                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setGraceProgress((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceProgress() + p6));
                            } else {
                                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setGraceLevel((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGraceLevel() + 1));
                                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setGraceProgress((p6 - v5_12));
                            }
                        }
                    } else {
                        it.paranoidsquirrels.idleguildmaster.storage.data.Data v5_18 = (it.paranoidsquirrels.idleguildmaster.Formulas.totalStarsToNextLp(it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeLevel()) - it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeProgress());
                        if (v5_18 > p6) {
                            it.paranoidsquirrels.idleguildmaster.MainActivity.data.setFortitudeProgress((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeProgress() + p6));
                        } else {
                            it.paranoidsquirrels.idleguildmaster.MainActivity.data.setFortitudeLevel((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getFortitudeLevel() + 1));
                            it.paranoidsquirrels.idleguildmaster.MainActivity.data.setFortitudeProgress((p6 - v5_18));
                        }
                    }
                } else {
                    it.paranoidsquirrels.idleguildmaster.storage.data.Data v5_25 = (it.paranoidsquirrels.idleguildmaster.Formulas.totalStarsToNextLp(it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlLevel()) - it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlProgress());
                    if (v5_25 > p6) {
                        it.paranoidsquirrels.idleguildmaster.MainActivity.data.setControlProgress((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlProgress() + p6));
                    } else {
                        it.paranoidsquirrels.idleguildmaster.MainActivity.data.setControlLevel((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getControlLevel() + 1));
                        it.paranoidsquirrels.idleguildmaster.MainActivity.data.setControlProgress((p6 - v5_25));
                    }
                }
            } else {
                it.paranoidsquirrels.idleguildmaster.storage.data.Data v5_31 = (it.paranoidsquirrels.idleguildmaster.Formulas.totalStarsToNextLp(it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionLevel()) - it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionProgress());
                if (v5_31 > p6) {
                    it.paranoidsquirrels.idleguildmaster.MainActivity.data.setAfflictionProgress((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionProgress() + p6));
                } else {
                    it.paranoidsquirrels.idleguildmaster.MainActivity.data.setAfflictionLevel((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getAfflictionLevel() + 1));
                    it.paranoidsquirrels.idleguildmaster.MainActivity.data.setAfflictionProgress((p6 - v5_31));
                }
            }
        } else {
            it.paranoidsquirrels.idleguildmaster.MainActivity.data.setGems((it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGems() + ((long) p6)));
            ((it.paranoidsquirrels.idleguildmaster.MainActivity) it.paranoidsquirrels.idleguildmaster.MainActivity.dungeonsFragment.getActivity()).refreshGems();
        }
        p8.removeView(p9.getRoot());
        p7.remove(p10);
        this.updateList.remove(p9.questProgress, p10);
        this.refreshLpInfo();
        this.completedInThisInstance = (this.completedInThisInstance + 1);
        if (this.updateList.isEmpty()) {
            this.initialize(0);
        }
        ((it.paranoidsquirrels.idleguildmaster.MainActivity) this.getActivity()).refreshIcons();
        return;
    }
```

## `onResume()V`

```java
public void onResume()
    {
        super.onResume();
        it.paranoidsquirrels.idleguildmaster.Utils.refreshCooldowns(it.paranoidsquirrels.idleguildmaster.TrueTimeUtils.millis());
        return;
    }
```

## `onStart()V`

```java
public void onStart()
    {
        super.onStart();
        this.completedInThisInstance = 0;
        it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogQuests = this;
        return;
    }
```

## `onStop()V`

```java
public void onStop()
    {
        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.QUEST_NOTIFICATION = this.notificationValue();
        ((it.paranoidsquirrels.idleguildmaster.MainActivity) this.getActivity()).refreshIcons();
        it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogQuests = 0;
        int v0_1 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getQuestsCompleted();
        if (v0_1 < 150) {
            it.paranoidsquirrels.idleguildmaster.storage.data.Data v2_0 = this.completedInThisInstance;
            if (v2_0 > null) {
                if (v0_1 < 25) {
                    it.paranoidsquirrels.idleguildmaster.AchievementsUtils.increment("CgkIttPX_-AEEAIQDQ", v2_0);
                }
                it.paranoidsquirrels.idleguildmaster.AchievementsUtils.increment("CgkIttPX_-AEEAIQDg", this.completedInThisInstance);
                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setQuestsCompleted(Math.min(150, (v0_1 + this.completedInThisInstance)));
            }
        }
        super.onStop();
        return;
    }
```

## `reInitialize()V`

```java
public void reInitialize()
    {
        this.initialize(0);
        return;
    }
```

## `refreshCooldowns(I I I)V`

```java
public void refreshCooldowns(int p3, int p4, int p5)
    {
        this.binding.newQuestsTime.setText(String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.time_days_hours_minutes), new Object[] {Integer.valueOf(p3), Integer.valueOf(p4), Integer.valueOf(p5)})));
        return;
    }
```

## `setBinding(Landroidx/viewbinding/ViewBinding;)V`

```java
protected void setBinding(androidx.viewbinding.ViewBinding p1)
    {
        this.binding = ((it.paranoidsquirrels.idleguildmaster.databinding.DialogQuestsBinding) p1);
        return;
    }
```

## `setLayout()V`

```java
protected void setLayout()
    {
        this.getDialog().getWindow().setLayout(((int) (((double) this.getResources().getDisplayMetrics().widthPixels) * 4606281698874543309)), -2);
        return;
    }
```

## `update()V`

```java
public void update()
    {
        java.util.Iterator v0_2 = this.updateList.entrySet().iterator();
        while (v0_2.hasNext()) {
            android.widget.ProgressBar v1_1 = ((java.util.Map$Entry) v0_2.next());
            int v2_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest) v1_1.getValue());
            ((android.widget.ProgressBar) v1_1.getKey()).setProgress(Math.round((Math.min(1065353216, ((float) (((double) v2_1.getProgress()) / ((double) v2_1.getTargetProgress())))) * 1120403456)));
        }
        return;
    }
```

## `show` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `dismiss` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getString` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getResources` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getContext` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getLayoutInflater` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getParentFragmentManager` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getActivity` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getDialog` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'
