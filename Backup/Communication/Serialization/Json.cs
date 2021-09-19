using Newtonsoft.Json;
using System.Text;

namespace Communication.Serialization
{
    public class Json : ISerialization
    {
        public T Deserialize<T>(byte[] content)
        {
            string json = Encoding.UTF8.GetString(content);
            return JsonConvert.DeserializeObject<T>(json,
                 new JsonSerializerSettings()
                 {
                     TypeNameHandling = TypeNameHandling.Objects,
                     ContractResolver = new PrivateResolver()
                 });
        }

        public byte[] Serialize<T>(T obj)
        {
            string json = JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                });
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
