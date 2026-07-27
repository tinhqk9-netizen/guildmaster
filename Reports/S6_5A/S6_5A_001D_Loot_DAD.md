# S6.5A-001D — DAD Decompile: Loot / Chest / Drop
**Tool:** androguard 4.1.4 (DAD decompiler) · **Nguồn:** DEX từ `it.paranoidsquirrels.idleguildmaster.apk` (XAPK v2.147)
> Hằng số double/float in ra dạng **bit-pattern long thô**. Giải mã: `struct.unpack('<d', struct.pack('<Q', bits))`.
> Mọi đoạn DAD render nghi ngờ phải đối chiếu file smali tương ứng.

---

## `Area.loot`

```java
private void loot()
    {
        if (!this.fullChest()) {
            int v0_7 = this.corpses.iterator();
            it.paranoidsquirrels.idleguildmaster.MainActivity v1_4 = 1;
            while (v0_7.hasNext()) {
                Integer v4_9;
                Integer v2_3 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy) v0_7.next());
                Integer v4_8 = this.event;
                if (v4_8 != null) {
                    v4_9 = v4_8.getKey();
                } else {
                    v4_9 = 0;
                }
                it.paranoidsquirrels.idleguildmaster.storage.data.items.ItemWrapper v5_7;
                Integer v4_12 = ((it.paranoidsquirrels.idleguildmaster.storage.data.items.ItemWrapper) it.paranoidsquirrels.idleguildmaster.Utils.rollFromWeightedMap(v2_3.listDrops(v4_9)));
                if (((this.petExploring == null) || ((v4_12 != null) && (v4_12.getItem().isNotSellable()))) || (it.paranoidsquirrels.idleguildmaster.Utils.random() >= (this.petExploring.getDrops() / 4636737291354636288))) {
                    v5_7 = 0;
                } else {
                    it.paranoidsquirrels.idleguildmaster.storage.data.items.ItemWrapper v5_9;
                    it.paranoidsquirrels.idleguildmaster.storage.data.items.ItemWrapper v5_8 = this.event;
                    if (v5_8 != null) {
                        v5_9 = v5_8.getKey();
                    } else {
                        v5_9 = 0;
                    }
                    v5_7 = ((it.paranoidsquirrels.idleguildmaster.storage.data.items.ItemWrapper) it.paranoidsquirrels.idleguildmaster.Utils.rollFromWeightedMap(v2_3.listDrops(v5_9)));
                }
                if (v4_12 != null) {
                    it.paranoidsquirrels.idleguildmaster.MainActivity v1_12 = v4_12.getItem();
                    it.paranoidsquirrels.idleguildmaster.Utils.collectItem(v1_12, this.drops);
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 8, new Object[] {Integer.valueOf(v2_3.getIdName()), Integer.valueOf(v1_12.getStack()), Integer.valueOf(v1_12.getIdName())}));
                    if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getTutorialStep() == 2) {
                        it.paranoidsquirrels.idleguildmaster.MainActivity.data.setTutorialStep(3);
                        if ((it.paranoidsquirrels.idleguildmaster.Utils.isMainLooper()) && (it.paranoidsquirrels.idleguildmaster.MainActivity.dungeonsFragment != null)) {
                            ((it.paranoidsquirrels.idleguildmaster.MainActivity) it.paranoidsquirrels.idleguildmaster.MainActivity.dungeonsFragment.getActivity()).refreshTutorial();
                        }
                        this.event = 0;
                    }
                    v1_4 = 0;
                }
                if (v5_7 != null) {
                    it.paranoidsquirrels.idleguildmaster.MainActivity v1_0 = v5_7.getItem();
                    it.paranoidsquirrels.idleguildmaster.Utils.collectItem(v1_0, this.drops);
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 8, new Object[] {Integer.valueOf(v2_3.getIdName()), Integer.valueOf(v1_0.getStack()), Integer.valueOf(v1_0.getIdName())}));
                    v1_4 = 0;
                }
                if (it.paranoidsquirrels.idleguildmaster.Utils.random() < 4557750909289998844) {
                    it.paranoidsquirrels.idleguildmaster.MainActivity v1_6 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance("Geode");
                    it.paranoidsquirrels.idleguildmaster.Utils.collectItem(v1_6, this.drops);
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 8, new Object[] {Integer.valueOf(v2_3.getIdName()), Integer.valueOf(v1_6.getStack()), Integer.valueOf(v1_6.getIdName())}));
                    v1_4 = 0;
                }
            }
            if (v1_4 == null) {
                this.refreshLoot();
            } else {
                it.paranoidsquirrels.idleguildmaster.MainActivity v1_7 = new Object[0];
                it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 9, v1_7);
            }
        } else {
            it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 100, new Object[] {Integer.valueOf(it.paranoidsquirrels.idleguildmaster.R$string.log_full_drops)}));
        }
        this.corpses.clear();
        return;
    }
```

## `Area.fullChest`

```java
private boolean fullChest()
    {
        int v0_3 = this.drops.iterator();
        int v1 = 0;
        int v2 = 0;
        while (v0_3.hasNext()) {
            v2 += ((it.paranoidsquirrels.idleguildmaster.storage.data.items.Item) v0_3.next()).getStack();
        }
        int v0_2;
        if (!it.paranoidsquirrels.idleguildmaster.MainActivity.data.isMerchantPackPurchased()) {
            v0_2 = 2000;
        } else {
            v0_2 = 3000;
        }
        if (v2 >= v0_2) {
            v1 = 1;
        }
        return v1;
    }
```

## `Utils.collectDrops`

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

## `Utils.collectItem`

```java
public static void collectItem(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item p2, java.util.List p3)
    {
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.getSeenItems().add(p2.getTrueClass());
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getItems().equals(p3)) {
            if ("DivineZygote".equals(p2.getTrueClass())) {
                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setReviewTrigger(1);
            }
            java.util.Set v0_6 = it.paranoidsquirrels.idleguildmaster.storage.data.items.Recipes.into(p2);
            if (v0_6 != null) {
                it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnownRecipes().add(v0_6);
            }
            it.paranoidsquirrels.idleguildmaster.MainActivity.data.getKnownRecipes().addAll(it.paranoidsquirrels.idleguildmaster.storage.data.items.Recipes.from(p2));
        }
        if (!p3.contains(p2)) {
            p3.add(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item.getInstance(p2.getTrueClass(), Math.min(99999, p2.getStack())));
        } else {
            it.paranoidsquirrels.idleguildmaster.storage.data.items.Item v3_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.items.Item) p3.get(p3.indexOf(p2)));
            v3_2.setStack(Math.min(99999, (v3_2.getStack() + p2.getStack())));
        }
        return;
    }
```

## `Utils.removeItemFromStorage`

```java
public static void removeItemFromStorage(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item p2)
    {
        it.paranoidsquirrels.idleguildmaster.storage.data.items.Item v0_4 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getItems().indexOf(p2);
        if (v0_4 != -1) {
            it.paranoidsquirrels.idleguildmaster.storage.data.items.Item v0_2 = ((it.paranoidsquirrels.idleguildmaster.storage.data.items.Item) it.paranoidsquirrels.idleguildmaster.MainActivity.data.getItems().get(v0_4));
            v0_2.setStack(Math.max(0, (v0_2.getStack() - p2.getStack())));
            if (v0_2.getStack() == 0) {
                it.paranoidsquirrels.idleguildmaster.MainActivity.data.getItems().remove(v0_2);
            }
            return;
        } else {
            return;
        }
    }
```
