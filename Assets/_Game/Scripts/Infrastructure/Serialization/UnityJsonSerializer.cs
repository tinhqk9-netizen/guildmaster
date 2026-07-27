using UnityEngine;

namespace GuildMaster.Infrastructure.Serialization
{
    public class UnityJsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        public string Serialize<T>(T value)
        {
            return JsonUtility.ToJson(value, true);
        }
    }
}
