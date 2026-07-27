using System.Collections.Generic;

namespace GuildMaster.Database
{
    public class DatabaseBuildReport
    {
        public string providerName;
        public bool manifestLoaded;
        public int expectedFiles;
        public int loadedFiles;
        public int skippedFiles;
        
        public Dictionary<string, int> loadedRecordsByCategory = new Dictionary<string, int>();
        public List<string> duplicateIds = new List<string>();
        public List<string> recordCountMismatches = new List<string>();
        public List<string> unsupportedCategories = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        
        public bool hasFatalErrors => errors.Count > 0 || !manifestLoaded;
    }
}
