using System;
using Newtonsoft.Json;
using System.Net;


namespace Bee.Core.Converters
{
    public class IPAddressConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            
            if (reader == null)
                return existingValue;
            return IPAddress.Parse((string)reader.Value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddress);
        }
    }
}
