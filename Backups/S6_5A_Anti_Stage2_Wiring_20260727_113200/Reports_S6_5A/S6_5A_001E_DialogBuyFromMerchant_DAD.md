# S6.5A-001E — DialogBuyFromMerchant (Merchant BUY rule)

**Class:** `Lit/paranoidsquirrels/idleguildmaster/ui/dialogs/DialogBuyFromMerchant;`  ·  **DEX:** classes3.dex  ·  **Tool:** androguard 4.1.4 DAD

---

## `attachListeners()V`

```java
protected void attachListeners()
    {
        this.binding.cancel.setOnClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogBuyFromMerchant$$ExternalSyntheticLambda0(this));
        this.binding.priceContainer.setOnClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogBuyFromMerchant$$ExternalSyntheticLambda1(this));
        this.binding.shop.setOnClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogBuyFromMerchant$$ExternalSyntheticLambda2(this));
        return;
    }
```

## `getTitle()Ljava/lang/String;`

```java
protected String getTitle()
    {
        return this.getString(it.paranoidsquirrels.idleguildmaster.R$string.merchant_dialog_confirm_buy_title);
    }
```

## `inflate(Landroid/view/LayoutInflater; Landroid/view/ViewGroup; Z)Landroidx/viewbinding/ViewBinding;`

```java
protected androidx.viewbinding.ViewBinding inflate(android.view.LayoutInflater p1, android.view.ViewGroup p2, boolean p3)
    {
        it.paranoidsquirrels.idleguildmaster.databinding.DialogBuyFromMerchantBinding v1_1 = it.paranoidsquirrels.idleguildmaster.databinding.DialogBuyFromMerchantBinding.inflate(p1, p2, p3);
        this.binding = v1_1;
        return v1_1;
    }
```

## `initialize(Landroid/os/Bundle;)V`

```java
protected void initialize(android.os.Bundle p5)
    {
        int v0_3;
        android.widget.TextView v5_1 = this.binding.confirmText;
        if (this.offer.getItem().getStack() <= 1) {
            v0_3 = String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.merchant_dialog_confirm_buy_single_body), new Object[] {this.getString(this.offer.getItem().getIdName())}));
        } else {
            v0_3 = String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.merchant_dialog_confirm_buy_multiple_body), new Object[] {Integer.valueOf(this.offer.getItem().getStack()), this.getString(this.offer.getItem().getIdName())}));
        }
        int v0_9;
        v5_1.setText(v0_3);
        android.content.res.Resources$Theme v2_12 = 8;
        if (!this.offer.isGems()) {
            v0_9 = 0;
        } else {
            v0_9 = 8;
        }
        int v0_13;
        this.binding.containerMoney.setVisibility(v0_9);
        if (!this.offer.isGems()) {
            v0_13 = 8;
        } else {
            v0_13 = 0;
        }
        this.binding.containerGems.setVisibility(v0_13);
        if ((this.offer.isGems()) && (it.paranoidsquirrels.idleguildmaster.MainActivity.IAPWrapper.initialized)) {
            v2_12 = 0;
        }
        this.binding.shop.setVisibility(v2_12);
        if (!this.offer.isGems()) {
            it.paranoidsquirrels.idleguildmaster.UIUtils.populateMoneyContainer(this.binding.amountMoney, this.offer.getPrice(), 1);
        } else {
            this.binding.amountGems.setText(String.valueOf(this.offer.getPrice()));
        }
        this.binding.error.setTextColor(this.getResources().getColor(it.paranoidsquirrels.idleguildmaster.UIUtils.getFailureColor(), this.getContext().getTheme()));
        return;
    }
```

## `lambda$attachListeners$0$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogBuyFromMerchant(Landroid/view/View;)V`

```java
synthetic void lambda$attachListeners$0$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogBuyFromMerchant(android.view.View p1)
    {
        this.dismiss();
        return;
    }
```

## `lambda$attachListeners$1$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogBuyFromMerchant(Landroid/view/View;)V`

```java
synthetic void lambda$attachListeners$1$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogBuyFromMerchant(android.view.View p1)
    {
        if (this.callback.getAsBoolean()) {
            this.dismiss();
        }
        return;
    }
```

## `lambda$attachListeners$2$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogBuyFromMerchant(Landroid/view/View;)V`

```java
synthetic void lambda$attachListeners$2$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogBuyFromMerchant(android.view.View p3)
    {
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogShop == null) {
            new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogShop().show(this.getParentFragmentManager(), "shop");
            return;
        } else {
            return;
        }
    }
```

## `onStart()V`

```java
public void onStart()
    {
        super.onStart();
        it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogBuyFromMerchant = this;
        return;
    }
```

## `onStop()V`

```java
public void onStop()
    {
        it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogBuyFromMerchant = 0;
        super.onStop();
        return;
    }
```

## `setBinding(Landroidx/viewbinding/ViewBinding;)V`

```java
protected void setBinding(androidx.viewbinding.ViewBinding p1)
    {
        this.binding = ((it.paranoidsquirrels.idleguildmaster.databinding.DialogBuyFromMerchantBinding) p1);
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

## `writeError(Ljava/lang/String;)V`

```java
public void writeError(String p2)
    {
        this.binding.error.setText(p2);
        return;
    }
```

## `getString` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getResources` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getContext` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `dismiss` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getParentFragmentManager` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getDialog` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `show` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'
