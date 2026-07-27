# S6.5A-001E — DialogCollectDrops (loot transfer)

**Class:** `Lit/paranoidsquirrels/idleguildmaster/ui/dialogs/DialogCollectDrops;`  ·  **DEX:** classes3.dex  ·  **Tool:** androguard 4.1.4 DAD

---

## `lambda$initialize$0(Lit/paranoidsquirrels/idleguildmaster/storage/data/items/Item;)I`

```java
static synthetic int lambda$initialize$0(it.paranoidsquirrels.idleguildmaster.storage.data.items.Item p0)
    {
        return (- p0.getRarity());
    }
```

## `attachListeners()V`

```java
protected void attachListeners()
    {
        this.binding.itemGrid.setOnItemClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogCollectDrops$$ExternalSyntheticLambda0(this));
        this.binding.itemGrid.setOnItemLongClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogCollectDrops$$ExternalSyntheticLambda1(this));
        this.binding.report.setOnClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogCollectDrops$$ExternalSyntheticLambda2(this));
        this.binding.close.setOnClickListener(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogCollectDrops$$ExternalSyntheticLambda3(this));
        return;
    }
```

## `getTitle()Ljava/lang/String;`

```java
protected String getTitle()
    {
        return String.format(this.getString(it.paranoidsquirrels.idleguildmaster.R$string.drops_collected_title), new Object[] {this.sourceArea}));
    }
```

## `inflate(Landroid/view/LayoutInflater; Landroid/view/ViewGroup; Z)Landroidx/viewbinding/ViewBinding;`

```java
protected androidx.viewbinding.ViewBinding inflate(android.view.LayoutInflater p1, android.view.ViewGroup p2, boolean p3)
    {
        it.paranoidsquirrels.idleguildmaster.databinding.DialogCollectDropsBinding v1_1 = it.paranoidsquirrels.idleguildmaster.databinding.DialogCollectDropsBinding.inflate(p1, p2, p3);
        this.binding = v1_1;
        return v1_1;
    }
```

## `initialize(Landroid/os/Bundle;)V`

```java
protected void initialize(android.os.Bundle p3)
    {
        this.drops.sort(java.util.Comparator.comparingInt(new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogCollectDrops$$ExternalSyntheticLambda4()));
        this.binding.itemGrid.setAdapter(it.paranoidsquirrels.idleguildmaster.UIUtils.getItemsGridAdapter(this.getContext(), this.drops));
        return;
    }
```

## `lambda$attachListeners$1$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogCollectDrops(Landroid/widget/AdapterView; Landroid/view/View; I J)V`

```java
synthetic void lambda$attachListeners$1$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogCollectDrops(android.widget.AdapterView p1, android.view.View p2, int p3, long p4)
    {
        it.paranoidsquirrels.idleguildmaster.UIUtils.openItemDetail(((it.paranoidsquirrels.idleguildmaster.storage.data.items.Item) this.drops.get(p3)));
        return;
    }
```

## `lambda$attachListeners$2$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogCollectDrops(Landroid/widget/AdapterView; Landroid/view/View; I J)Z`

```java
synthetic boolean lambda$attachListeners$2$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogCollectDrops(android.widget.AdapterView p1, android.view.View p2, int p3, long p4)
    {
        it.paranoidsquirrels.idleguildmaster.UIUtils.vibrate(this.getContext());
        int v1_3 = new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogSell();
        v1_3.setItem(((it.paranoidsquirrels.idleguildmaster.storage.data.items.Item) this.drops.get(p3)));
        v1_3.show(this.getParentFragmentManager(), "sell");
        return 1;
    }
```

## `lambda$attachListeners$3$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogCollectDrops(Landroid/view/View;)V`

```java
synthetic void lambda$attachListeners$3$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogCollectDrops(android.view.View p3)
    {
        if (it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogReport == null) {
            it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogReport v3_2 = new it.paranoidsquirrels.idleguildmaster.ui.dialogs.DialogReport();
            v3_2.sourceArea = this.sourceArea;
            v3_2.recap = this.recap;
            v3_2.show(this.getParentFragmentManager(), "dialog_report");
            return;
        } else {
            return;
        }
    }
```

## `lambda$attachListeners$4$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogCollectDrops(Landroid/view/View;)V`

```java
synthetic void lambda$attachListeners$4$it-paranoidsquirrels-idleguildmaster-ui-dialogs-DialogCollectDrops(android.view.View p1)
    {
        this.dismiss();
        return;
    }
```

## `onStart()V`

```java
public void onStart()
    {
        super.onStart();
        it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogCollectDrops = this;
        return;
    }
```

## `onStop()V`

```java
public void onStop()
    {
        it.paranoidsquirrels.idleguildmaster.MainActivity.shownDialogCollectDrops = 0;
        super.onStop();
        return;
    }
```

## `setBinding(Landroidx/viewbinding/ViewBinding;)V`

```java
protected void setBinding(androidx.viewbinding.ViewBinding p1)
    {
        this.binding = ((it.paranoidsquirrels.idleguildmaster.databinding.DialogCollectDropsBinding) p1);
        return;
    }
```

## `setCancelable` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `show` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getString` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getContext` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `getParentFragmentManager` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'

## `dismiss` — DAD FAILED: 'ExternalMethod' object has no attribute 'get_source'
