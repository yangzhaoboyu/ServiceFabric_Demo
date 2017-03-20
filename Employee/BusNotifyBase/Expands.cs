using Newtonsoft.Json;

namespace BusNotify
{
    public static class Expands
    {
        public static string ToJson(this object value, string nullValue = null)
        {
            if (value == null)
            {
                return nullValue;
            }
            try
            {
                return JsonConvert.SerializeObject(value);
            }
            catch
            {
                return null;
            }
        }
    }
}