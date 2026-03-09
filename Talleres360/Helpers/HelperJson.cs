using Newtonsoft.Json;

namespace Talleres360.Helpers
{
    public class HelperJson
    {
        public static string SerializeObject<T>(T data)
        {
            string json = JsonConvert.SerializeObject(data);
            return json;

        }

        public static T DeserializeObject<T>(string data)
        {

            T objeto = JsonConvert.DeserializeObject<T>(data);
            return objeto;

        }
    }
}
