using Newtonsoft.Json;

namespace MyWorldIsComics.Mappers
{
    class JsonDeserialize
    {
        public static T DeserializeJsonString<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
