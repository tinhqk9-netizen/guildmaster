# S6.5A-001E — Action class (dungeon state machine)

**Class:** `Lit/paranoidsquirrels/idleguildmaster/storage/data/places/Action;`  ·  **DEX:** classes3.dex  ·  **Tool:** androguard 4.1.4 DAD

---

## `<init>(I)V`

```java
public Action(int p2)
    {
        this.turnsPassed = 0;
        this.type = p2;
        switch (p2) {
            case 0:
                this.turnsToComplete = 5;
                this.name = it.paranoidsquirrels.idleguildmaster.R$string.action_enter_dungeon;
                break;
            case 1:
                this.turnsToComplete = 5;
                this.name = it.paranoidsquirrels.idleguildmaster.R$string.action_enter_room;
                break;
            case 2:
                this.turnsToComplete = 2;
                this.name = it.paranoidsquirrels.idleguildmaster.R$string.action_fight;
                break;
            case 3:
                this.turnsToComplete = 5;
                this.name = it.paranoidsquirrels.idleguildmaster.R$string.action_loot;
                break;
            case 4:
                this.turnsToComplete = 5;
                this.name = it.paranoidsquirrels.idleguildmaster.R$string.action_search;
                break;
            case 5:
                this.turnsToComplete = 18;
                this.name = it.paranoidsquirrels.idleguildmaster.R$string.action_respawn;
                break;
            case 6:
                this.turnsToComplete = 12;
                this.name = it.paranoidsquirrels.idleguildmaster.R$string.action_flee;
                break;
            default:
        }
        return;
    }
```

## `finished()Z`

```java
public boolean finished()
    {
        int v0_1;
        if (this.turnsPassed < this.turnsToComplete) {
            v0_1 = 0;
        } else {
            v0_1 = 1;
        }
        return v0_1;
    }
```

## `getName()I`

```java
public int getName()
    {
        return this.name;
    }
```

## `getTurnsPassed()I`

```java
public int getTurnsPassed()
    {
        return this.turnsPassed;
    }
```

## `getTurnsToComplete()I`

```java
public int getTurnsToComplete()
    {
        return this.turnsToComplete;
    }
```

## `getType()I`

```java
public int getType()
    {
        return this.type;
    }
```

## `nextTurn()V`

```java
public void nextTurn()
    {
        this.turnsPassed = (this.turnsPassed + 1);
        return;
    }
```

## `setTurnsPassed(I)V`

```java
public void setTurnsPassed(int p1)
    {
        this.turnsPassed = p1;
        return;
    }
```
