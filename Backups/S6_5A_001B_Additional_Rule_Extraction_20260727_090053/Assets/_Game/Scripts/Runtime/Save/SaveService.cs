using System;
using System.IO;
using UnityEngine;

namespace GuildMaster.Runtime.Save
{
    public class SaveService : ISaveService
    {
        private const int CurrentSaveVersion = 1;
        private readonly string _saveFilePath;
        private readonly string _backupFilePath;

        public SaveData CurrentData { get; private set; }

        public SaveService()
        {
            _saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
            _backupFilePath = Path.Combine(Application.persistentDataPath, "save_backup.json");
            CurrentData = new SaveData();
        }

        public bool HasSaveFile()
        {
            return File.Exists(_saveFilePath);
        }

        public bool Load(out Exception error)
        {
            error = null;
            if (!HasSaveFile())
            {
                // Fallback to empty save
                CurrentData = new SaveData();
                return true;
            }

            try
            {
                string json = File.ReadAllText(_saveFilePath);
                var loadedData = JsonUtility.FromJson<SaveData>(json);

                if (loadedData == null)
                {
                    throw new InvalidDataException("Parsed save data is null.");
                }

                if (loadedData.Metadata != null && loadedData.Metadata.SaveVersion < CurrentSaveVersion)
                {
                    MigrateSave(loadedData);
                }

                CurrentData = loadedData;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Failed to load save file: {ex.Message}");
                error = ex;
                
                // Fallback to backup
                if (File.Exists(_backupFilePath))
                {
                    Debug.LogWarning("[SaveService] Attempting to load backup save...");
                    try
                    {
                        string jsonBackup = File.ReadAllText(_backupFilePath);
                        CurrentData = JsonUtility.FromJson<SaveData>(jsonBackup);
                        return true;
                    }
                    catch (Exception backupEx)
                    {
                        Debug.LogError($"[SaveService] Failed to load backup save: {backupEx.Message}");
                        CurrentData = new SaveData(); // Fallback to empty
                        return false;
                    }
                }
                
                CurrentData = new SaveData(); // Fallback to empty
                return false;
            }
        }

        public bool Save(out Exception error)
        {
            error = null;
            try
            {
                if (CurrentData.Metadata == null)
                {
                    CurrentData.Metadata = new SaveMetadata();
                }

                CurrentData.Metadata.SaveVersion = CurrentSaveVersion;
                CurrentData.Metadata.SaveTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                CurrentData.Metadata.GameVersion = Application.version;

                string json = JsonUtility.ToJson(CurrentData, true);

                // Backup old save before overwriting
                if (File.Exists(_saveFilePath))
                {
                    File.Copy(_saveFilePath, _backupFilePath, true);
                }

                File.WriteAllText(_saveFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Failed to save game: {ex.Message}");
                error = ex;
                return false;
            }
        }

        public void DeleteSave()
        {
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
            }
            if (File.Exists(_backupFilePath))
            {
                File.Delete(_backupFilePath);
            }
            CurrentData = new SaveData();
        }

        private void MigrateSave(SaveData oldData)
        {
            // Placeholder for future migrations
            Debug.Log($"[SaveService] Migrating save from version {oldData.Metadata.SaveVersion} to {CurrentSaveVersion}");
            oldData.Metadata.SaveVersion = CurrentSaveVersion;
        }
    }
}
