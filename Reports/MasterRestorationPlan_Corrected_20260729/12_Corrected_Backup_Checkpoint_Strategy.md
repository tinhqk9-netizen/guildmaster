# CORRECTED BACKUP / CHECKPOINT STRATEGY

---

## Rule: Never `git reset --hard`

**NEVER USE:**
```bash
git reset --hard HEAD         # ❌ DESTROYS ALL UNCOMMITTED WORK
git checkout -- .             # ❌ DESTROYS ALL UNCOMMITTED CHANGES
```

**USE INSTEAD:** File-level backup + SHA256 manifest.

---

## Pre-Execution Backup

**Script to run BEFORE any phase:**

```bash
#!/bin/bash
# backup-before-phase.sh
# Usage: ./backup-before-phase.sh RESTORE_X_NAME

PHASE=$1
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="D:/Tinh/Rebuild_GuildMaster/_backups/${PHASE}/${DATE}"

mkdir -p "$BACKUP_DIR"

# 1. Git state record
git log --oneline -10 > "$BACKUP_DIR/git_log.txt"
git diff > "$BACKUP_DIR/git_uncommitted.diff" 2>/dev/null

# 2. Full Scripts directory (with compression)
echo "Backing up Scripts/..."
cp -r "Assets/_Game/Scripts" "$BACKUP_DIR/Scripts/"

# 3. UI directory (prefabs, scenes may have UI changes)
echo "Backing up UI/..."
cp -r "Assets/_Game/UI" "$BACKUP_DIR/UI/" 2>/dev/null || echo "No UI/ dir"

# 4. Prefabs (if any added/modified)
echo "Backing up Prefabs/..."
cp -r "Assets/_Game/Prefabs" "$BACKUP_DIR/Prefabs/" 2>/dev/null || echo "No Prefabs/ dir"

# 5. Resources (data files)
echo "Backing up Resources/Data..."
cp -r "Assets/_Game/Resources/Data" "$BACKUP_DIR/Resources_Data/" 2>/dev/null || echo "No Resources/Data"

# 6. Scene files
echo "Backing up Scenes..."
cp -r "Assets/_Game/Scenes" "$BACKUP_DIR/Scenes/" 2>/dev/null || echo "No Scenes/ dir"

# 7. ProjectSettings (may include Unity version changes)
echo "Backing up ProjectSettings..."
cp -r "ProjectSettings" "$BACKUP_DIR/ProjectSettings/" 2>/dev/null || echo "No ProjectSettings/"

# 8. Generate SHA256 manifest
echo "Generating SHA256 manifest..."
cd "$BACKUP_DIR"
find . -type f -exec sha256sum {} \; > "MANIFEST.sha256"
cd - > /dev/null

echo "=== BACKUP COMPLETE ==="
echo "Location: $BACKUP_DIR"
echo "Manifest: $BACKUP_DIR/MANIFEST.sha256"
echo "Size: $(du -sh "$BACKUP_DIR" | cut -f1)"
```

---

## Rollback Procedure

### Rollback to Last Backup:

```bash
# 1. Identify backup directory
ls -la "D:/Tinh/Rebuild_GuildMaster/_backups/${PHASE}/"

# 2. Restore individual files (NOT git reset)
cp -r "_backups/${PHASE}/${DATE}/Scripts" "Assets/_Game/Scripts"
cp -r "_backups/${PHASE}/${DATE}/UI" "Assets/_Game/UI"
# ... etc for each backed-up directory

# 3. Verify manifest
cd "_backups/${PHASE}/${DATE}"
sha256sum -c MANIFEST.sha256
cd -
```

### Rollback to Last Git Commit (for committed changes):

```bash
# Revert LAST commit (preserves history):
git revert HEAD --no-edit

# Or restore from stash (unstaged changes only):
git stash pop  # careful: this applies most recent stash

# NEVER use:
# ❌ git reset --hard HEAD
# ❌ git checkout -- .
```

---

## Per-Phase Checkpoint Convention

```
_backups/
  RESTORE_0_FOUNDATION/
    20260729_160000/        # pre-phase backup
    20260729_180000/        # post-phase checkpoint
  RESTORE_1_CORE_LOOP/
    20260730_090000/        # pre-phase backup
    ...
```

---

## Save File Backup

Before testing any phase in Unity editor:

1. Navigate to Unity's `persistentDataPath` (typically `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\`)
2. Copy `save.json` to `save.json.bak.<phase-name>`
3. Copy `save_backup.json` to `save_backup.json.bak.<phase-name>`

**Never test a phase without a save backup.**

---

## Emergency Rollback Script

```bash
#!/bin/bash
# emergency-rollback.sh
# Restore from most recent backup

LATEST_BACKUP=$(find "D:/Tinh/Rebuild_GuildMaster/_backups" -mindepth 2 -maxdepth 2 -type d | sort -r | head -1)

if [ -z "$LATEST_BACKUP" ]; then
    echo "ERROR: No backup found"
    exit 1
fi

echo "Restoring from: $LATEST_BACKUP"

# Verify manifest
cd "$LATEST_BACKUP"
if sha256sum -c MANIFEST.sha256; then
    echo "Manifest verified — restoring..."
else
    echo "WARNING: Manifest mismatch — files may be corrupted"
    echo "Proceed anyway? (y/N)"
    read -r RESPONSE
    [ "$RESPONSE" != "y" ] && exit 1
fi
cd - > /dev/null

# Restore directories
for DIR in Scripts UI Prefabs Resources_Data Scenes ProjectSettings; do
    SRC="$LATEST_BACKUP/$DIR"
    if [ -d "$SRC" ]; then
        # Map Resources_Data → Resources/Data
        if [ "$DIR" = "Resources_Data" ]; then
            DEST="Assets/_Game/Resources/Data"
        else
            DEST="Assets/_Game/$DIR"
        fi
        cp -r "$SRC"/* "$DEST/"
        echo "Restored: $DEST"
    fi
done

echo "=== ROLLBACK COMPLETE ==="
echo "Run Unity to verify restoration."
```
