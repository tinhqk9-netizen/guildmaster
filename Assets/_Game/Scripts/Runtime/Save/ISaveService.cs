using System;

namespace GuildMaster.Runtime.Save
{
    public interface ISaveService
    {
        SaveData CurrentData { get; }
        SaveLoadResult LastLoadStatus { get; }
        
        event Action OnSaveStarted;
        event Action<bool> OnSaveCompleted;

        bool HasSaveFile();
        bool Load(out Exception error);
        bool Save(out Exception error);
        void DeleteSave();
    }
}
