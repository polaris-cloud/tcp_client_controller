using System;
using Newtonsoft.Json;
using System.Net;


namespace Polaris.Storage.Json
{
    public class IPAddressConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            return IPAddress.Parse((string?)reader.Value??throw  new InvalidOperationException("Parse value is null"));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddress);
        }
    }
}
