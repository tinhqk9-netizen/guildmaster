using System;

namespace GuildMaster.Runtime.Save
{
    public interface ISaveService
    {
        SaveData CurrentData { get; }
        
        bool HasSaveFile();
        bool Load(out Exception error);
        bool Save(out Exception error);
        void DeleteSave();
    }
}
