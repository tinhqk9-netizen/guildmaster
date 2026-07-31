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
        public SaveLoadResult LastLoadStatus { get; private set; }

        public event Action OnSaveStarted;
        public event Action<bool> OnSaveCompleted;

        public SaveService()
        {
            _saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
            _backupFilePath = Path.Combine(Application.persistentDataPath, "save_backup.json");
            CurrentData = SaveData.CreateDefault();
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
                CurrentData = SaveData.CreateDefault();
                CurrentData.NormalizeAfterLoad();
                LastLoadStatus = SaveLoadResult.FreshNewGame;
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

                // Fields introduced after this file was written come back as null/default;
                // normalising first means migration and callers never see a null list.
                loadedData.NormalizeAfterLoad();

                if (loadedData.Metadata != null && loadedData.Metadata.SaveVersion < CurrentSaveVersion)
                {
                    MigrateSave(loadedData);
                }

                CurrentData = loadedData;
                LastLoadStatus = SaveLoadResult.PrimaryLoaded;
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
                        CurrentData = JsonUtility.FromJson<SaveData>(jsonBackup) ?? SaveData.CreateDefault();
                        CurrentData.NormalizeAfterLoad();
                        LastLoadStatus = SaveLoadResult.BackupLoaded;
                        return true;
                    }
                    catch (Exception backupEx)
                    {
                        Debug.LogError($"[SaveService] Failed to load backup save: {backupEx.Message}");
                        CurrentData = SaveData.CreateDefault(); // Fallback to empty
                        CurrentData.NormalizeAfterLoad();
                        LastLoadStatus = SaveLoadResult.FreshAfterCorruption;
                        return false;
                    }
                }
                
                CurrentData = SaveData.CreateDefault(); // Fallback to empty
                CurrentData.NormalizeAfterLoad();
                LastLoadStatus = SaveLoadResult.FreshAfterCorruption;
                return false;
            }
        }

        public bool Save(out Exception error)
        {
            error = null;
            bool success = false;
            OnSaveStarted?.Invoke();
            
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
                success = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Failed to save game: {ex.Message}");
                error = ex;
                return false;
            }
            finally
            {
                OnSaveCompleted?.Invoke(success);
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
            CurrentData = SaveData.CreateDefault();
        }

        private void MigrateSave(SaveData oldData)
        {
            // Placeholder for future migrations
            Debug.Log($"[SaveService] Migrating save from version {oldData.Metadata.SaveVersion} to {CurrentSaveVersion}");
            oldData.Metadata.SaveVersion = CurrentSaveVersion;
        }
    }
}
