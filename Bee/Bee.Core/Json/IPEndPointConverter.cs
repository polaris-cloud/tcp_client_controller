using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Bee.Core.Json
{
    public class IPEndPointConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ep = (IPEndPoint)value;
            var jo = new JObject();
            jo.Add("Address", JToken.FromObject(ep.Address, serializer));
            jo.Add("Port", ep.Port);
            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            //if (reader.Value == null)
            //    return null;
            var jo = JObject.Load(reader);
            var address = jo["Address"].ToObject<IPAddress>(serializer);
            var port = (int)jo["Port"];
            return new IPEndPoint(address, port);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPEndPoint);
        }
    }
}
