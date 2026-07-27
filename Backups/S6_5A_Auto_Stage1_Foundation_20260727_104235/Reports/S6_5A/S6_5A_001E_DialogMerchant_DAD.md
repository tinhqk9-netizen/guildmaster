# S6.5A-001E — DialogMerchant + lambdas (Merchant BUY/SELL rule)

**DEX:** classes3.dex · **Tool:** androguard 4.1.4 DAD

---

## `DialogMerchant.openBuyDialog`

```java
private void openBuyDialog(it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer p3, boolean p4)
    {
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogBuyFromMerchant == null) {
            it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogBuyFromMerchant v0_2 = new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogBuyFromMerchant();
            v0_2.offer = p3;
            v0_2.callback = new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogMerchant$$ExternalSyntheticLambda0(this, p3, p4);
            v0_2.show(this.getParentFragmentManager(), "select_equipment");
            return;
        } else {
            return;
        }
    }
```

## `DialogMerchant.attachListeners`

```java
protected void attachListeners()
    {
        this.binding.regularItemGrid.setOnItemClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogMerchant$$ExternalSyntheticLambda1(this));
        this.binding.specialItemGrid.setOnItemClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogMerchant$$ExternalSyntheticLambda2(this));
        this.binding.regularItemGrid.setOnItemLongClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogMerchant$$ExternalSyntheticLambda3(this));
        this.binding.specialItemGrid.setOnItemLongClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogMerchant$$ExternalSyntheticLambda4(this));
        this.binding.close.setOnClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogMerchant$$ExternalSyntheticLambda5(this));
        return;
    }
```

## `DialogMerchant.getBinding`

```java
protected androidx.viewbinding.ViewBinding getBinding()
    {
        return this.binding;
    }
```

## `DialogMerchant.getTitle`

```java
protected String getTitle()
    {
        return this.getString(it.paranoidsquirrels.idleguildmaster.R$string.merchant_dialog_title);
    }
```

## `DialogMerchant.inflate`

```java
protected androidx.viewbinding.ViewBinding inflate(android.view.LayoutInflater p1, android.view.ViewGroup p2, boolean p3)
    {
        it.paranoidsquirrels.idleguildmaster.databinding.DialogMerchantBinding v1_1 = it.paranoidsquirrels.idleguildmaster.databinding.DialogMerchantBinding.inflate(p1, p2, p3);
        this.binding = v1_1;
        return v1_1;
    }
```

## `DialogMerchant.initialize`

```java
protected void initialize(android.os.Bundle p5)
    {
        android.widget.TextView v0_18;
        int v1_4 = 4;
        int v2 = 0;
        if (!it.paranoidsquirrels.idleguildmaster.MainActivity.data.isNewMerchantRegularItems()) {
            v0_18 = 4;
        } else {
            v0_18 = 0;
        }
        this.binding.newRegular.setVisibility(v0_18);
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.data.isNewMerchantSpecialItems()) {
            v1_4 = 0;
        }
        this.binding.newSpecial.setVisibility(v1_4);
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.setNewMerchantRegularItems(0);
        it.paranoidsquirrels.idleguildmaster.MainActivity.data.setNewMerchantSpecialItems(0);
        ((it.paranoidsquirrels.idleguildmaster.MainActivity) this.getActivity()).refreshIcons();
        boolean v5_9 = new java.util.ArrayList();
        android.widget.TextView v0_5 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantRegularStockItems().iterator();
        while (v0_5.hasNext()) {
            v5_9.add(((it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer) v0_5.next()).getItem());
        }
        this.binding.regularItemGrid.setAdapter(it.paranoidsquirrels.idleguildmaster.UIUtils.getItemsGridAdapter(this.getContext(), v5_9));
        boolean v5_12 = new java.util.ArrayList();
        android.widget.TextView v0_11 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantSpecialReserve().iterator();
        while (v0_11.hasNext()) {
            v5_12.add(((it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer) v0_11.next()).getItem());
        }
        int v3_0;
        this.binding.specialItemGrid.setAdapter(it.paranoidsquirrels.idleguildmaster.UIUtils.getItemsGridAdapter(this.getContext(), v5_12));
        boolean v5_16 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantRegularStockItems().isEmpty();
        if (!v5_16) {
            v3_0 = 0;
        } else {
            v3_0 = 8;
        }
        boolean v5_17;
        this.binding.regularItemGrid.setVisibility(v3_0);
        if (!v5_16) {
            v5_17 = 8;
        } else {
            v5_17 = 0;
        }
        int v3_1;
        this.binding.noRegularItems.setVisibility(v5_17);
        boolean v5_20 = it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantSpecialReserve().isEmpty();
        if (!v5_20) {
            v3_1 = 0;
        } else {
            v3_1 = 8;
        }
        this.binding.specialItemGrid.setVisibility(v3_1);
        if (!v5_20) {
            v2 = 8;
        }
        this.binding.noSpecialItems.setVisibility(v2);
        return;
    }
```

## `DialogMerchant.lambda$attachListeners$0$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant`

```java
synthetic void lambda$attachListeners$0$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(android.widget.AdapterView p1, android.view.View p2, int p3, long p4)
    {
        this.openBuyDialog(((it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer) it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantRegularStockItems().get(p3)), 0);
        return;
    }
```

## `DialogMerchant.lambda$attachListeners$1$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant`

```java
synthetic void lambda$attachListeners$1$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(android.widget.AdapterView p1, android.view.View p2, int p3, long p4)
    {
        this.openBuyDialog(((it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer) it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantSpecialReserve().get(p3)), 1);
        return;
    }
```

## `DialogMerchant.lambda$attachListeners$2$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant`

```java
synthetic boolean lambda$attachListeners$2$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(android.widget.AdapterView p1, android.view.View p2, int p3, long p4)
    {
        it.paranoidsquirrels.idleguildmaster.UIUtils.vibrate(this.getContext());
        it.paranoidsquirrels.idleguildmaster.UIUtils.openItemDetail(((it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer) it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantRegularStockItems().get(p3)).getItem());
        return 1;
    }
```

## `DialogMerchant.lambda$attachListeners$3$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant`

```java
synthetic boolean lambda$attachListeners$3$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(android.widget.AdapterView p1, android.view.View p2, int p3, long p4)
    {
        it.paranoidsquirrels.idleguildmaster.UIUtils.vibrate(this.getContext());
        it.paranoidsquirrels.idleguildmaster.UIUtils.openItemDetail(((it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer) it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantSpecialReserve().get(p3)).getItem());
        return 1;
    }
```

## `DialogMerchant.lambda$attachListeners$4$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant`

```java
synthetic void lambda$attachListeners$4$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(android.view.View p1)
    {
        this.dismiss();
        return;
    }
```

## `DialogMerchant.lambda$openBuyDialog$5$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant`

```java
synthetic boolean lambda$openBuyDialog$5$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(it.paranoidsquirrels.idleguildmaster.storage.data.items.MerchantOffer p10, boolean p11)
    {
        if (!(p10.getItem() instanceof it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Upgrade)) {
            String v0_14 = new it.paranoidsquirrels.idleguildmaster.storage.data.items.Item[1];
            v0_14[0] = p10.getItem();
            if (it.paranoidsquirrels.idleguildmaster.Utils.remainingInventorySpaceAfterCollecting(0, v0_14) < 0) {
                it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogBuyFromMerchant.writeError(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.error_not_enough_space));
                return 0;
            }
        }
        if (!p10.isGems()) {
            long v5_1 = (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMoney() - p10.getPrice());
            if (v5_1 >= 0) {
                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setMoney(v5_1);
                ((it.paranoidsquirrels.idleguildmaster.MainActivity) this.getActivity()).refreshMoney();
            } else {
                it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogBuyFromMerchant.writeError(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.error_not_enough_money));
                return 0;
            }
        } else {
            long v5_3 = (it.paranoidsquirrels.idleguildmaster.MainActivity.data.getGems() - p10.getPrice());
            if (v5_3 >= 0) {
                it.paranoidsquirrels.idleguildmaster.MainActivity.data.setGems(v5_3);
                ((it.paranoidsquirrels.idleguildmaster.MainActivity) this.getActivity()).refreshGems();
            } else {
                it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogBuyFromMerchant.writeError(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.error_not_enough_gems));
                return 0;
            }
        }
        if (p11 == null) {
            it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantRegularStockItems().remove(p10);
        } else {
            it.paranoidsquirrels.idleguildmaster.MainActivity.data.getMerchantSpecialReserve().remove(p10);
        }
        if (p10.getItem().getUniqueOrigin() != null) {
            it.paranoidsquirrels.idleguildmaster.MainActivity.data.getUniqueItemsLost().remove(p10.getItem().getUniqueOrigin());
        }
        if (!(p10.getItem() instanceof it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Upgrade)) {
            it.paranoidsquirrels.idleguildmaster.Utils.collectItem(p10.getItem(), it.paranoidsquirrels.idleguildmaster.MainActivity.data.getItems());
        } else {
            ((it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Upgrade) p10.getItem()).use();
        }
        it.paranoidsquirrels.idleguildmaster.MainActivity.headquartersFragment.refresh();
        this.initialize(0);
        return 1;
    }
```

## `DialogMerchant.newItems`

```java
public void newItems()
    {
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogBuyFromMerchant != null) {
            it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogBuyFromMerchant.dismiss();
        }
        this.initialize(0);
        return;
    }
```

## `DialogMerchant.onResume`

```java
public void onResume()
    {
        super.onResume();
        it.paranoidsquirrels.idleguildmaster.Utils.refreshCooldowns(it.paranoidsquirrels.idleguildmaster.TrueTimeUtils.millis());
        return;
    }
```

## `DialogMerchant.onStart`

```java
public void onStart()
    {
        super.onStart();
        it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogMerchant = this;
        return;
    }
```

## `DialogMerchant.onStop`

```java
public void onStop()
    {
        it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogMerchant = 0;
        super.onStop();
        return;
    }
```

## `DialogMerchant.refreshCooldowns`

```java
public void refreshCooldowns(int p5, int p6, int p7)
    {
        this.binding.regularItemsCountdown.setText(String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.time_hours_minutes), new Object[] {Integer.valueOf(p6), Integer.valueOf(p7)})));
        this.binding.specialItemsCountdown.setText(String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.time_days_hours_minutes), new Object[] {Integer.valueOf(p5), Integer.valueOf(p6), Integer.valueOf(p7)})));
        return;
    }
```

## `DialogMerchant.setBinding`

```java
protected void setBinding(androidx.viewbinding.ViewBinding p1)
    {
        this.binding = ((it.paranoidsquirrels.idleguildmaster.databinding.DialogMerchantBinding) p1);
        return;
    }
```

## `DialogMerchant.show` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `DialogMerchant.getParentFragmentManager` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `DialogMerchant.getString` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `DialogMerchant.getActivity` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `DialogMerchant.getContext` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `DialogMerchant.dismiss` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `DialogMerchant$$ExternalSyntheticLambda0.getAsBoolean`

```java
public final boolean getAsBoolean()
    {
        return this.f$0.lambda$openBuyDialog$5$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(this.f$1, this.f$2);
    }
```

## `DialogMerchant$$ExternalSyntheticLambda1.onItemClick`

```java
public final void onItemClick(android.widget.AdapterView p7, android.view.View p8, int p9, long p10)
    {
        this.f$0.lambda$attachListeners$0$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(p7, p8, p9, p10);
        return;
    }
```

## `DialogMerchant$$ExternalSyntheticLambda2.onItemClick`

```java
public final void onItemClick(android.widget.AdapterView p7, android.view.View p8, int p9, long p10)
    {
        this.f$0.lambda$attachListeners$1$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(p7, p8, p9, p10);
        return;
    }
```

## `DialogMerchant$$ExternalSyntheticLambda3.onItemLongClick`

```java
public final boolean onItemLongClick(android.widget.AdapterView p7, android.view.View p8, int p9, long p10)
    {
        return this.f$0.lambda$attachListeners$2$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(p7, p8, p9, p10);
    }
```

## `DialogMerchant$$ExternalSyntheticLambda4.onItemLongClick`

```java
public final boolean onItemLongClick(android.widget.AdapterView p7, android.view.View p8, int p9, long p10)
    {
        return this.f$0.lambda$attachListeners$3$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(p7, p8, p9, p10);
    }
```

## `DialogMerchant$$ExternalSyntheticLambda5.onClick`

```java
public final void onClick(android.view.View p2)
    {
        this.f$0.lambda$attachListeners$4$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogMerchant(p2);
        return;
    }
```
