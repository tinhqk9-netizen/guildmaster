namespace GuildMaster.Runtime.Save
{
    public enum SaveLoadResult
    {
        PrimaryLoaded,        // Load bình thường
        BackupLoaded,         // Hỏng file chính, dùng backup
        FreshNewGame,         // Người chơi mới tải game
        FreshAfterCorruption, // Mất cả file chính & backup -> phải reset
        Failed                // Lỗi không thể load hay tạo mới
    }
}
