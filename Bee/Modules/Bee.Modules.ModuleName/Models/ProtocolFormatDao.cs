using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polaris.Protocol.enums;
using Prism.Mvvm;

namespace Bee.Modules.Script.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class ProtocolFormatDao : BindableBase
    {
        public ProtocolEncodeFormat EncodeFormat { get; set; }
        public ProtocolEndian ProtocolEndian { get; set; }

        public string SendDescription { get; set; }
        public string ResponseDescription { get; set; }
        public string BehaviorKeyword { get; set; }
        public string SendFrameRule { get; set; }
        public string ResponseFrameRule { get; set; }
        private string _keyValueData;

        public ProtocolFormatDao Clone()
        {
            var clone = MemberwiseClone();
            return (ProtocolFormatDao)clone;
        }

        [JsonIgnore]
        [KeyValuePairIgnore]
        public string KeyValueData
        {
            get
            {
                if (_keyValueData != null)
                    return _keyValueData;
                else
                {
                    //_keyValueData=;
                    //SetProperty(ref _keyValueData, ToKeyValueString());
                    _keyValueData = ToKeyValueString();
                    return _keyValueData;
                }
            }
            set => SetProperty(ref _keyValueData, value);
        }



        private string ToKeyValueString()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<KeyValuePairIgnoreAttribute>() == null);

            var keyValuePairs = properties.Select(p =>
            {
                var value = p.GetValue(this);
                var formattedValue = value != null ? $"{value}" : " ";
                return $"{p.Name}={formattedValue}";
            });

            return string.Join("\n", keyValuePairs);
        }

        public void ModifyFromKeyValueString()
        {

            var keyValuePairs = KeyValueData.Split('\n')
                .Select(pair => pair.Split('='))
                .ToDictionary(split => split[0], split => split.Skip(1).Any() ? string.Join("=", split.Skip(1)) : null);

            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<KeyValuePairIgnoreAttribute>() == null);

            foreach (var property in properties)
            {
                if (keyValuePairs.TryGetValue(property.Name, out var value))
                {
                    // Handle enum types
                    if (property.PropertyType.IsEnum)
                    {
                        var enumValue = Enum.Parse(property.PropertyType, value);
                        property.SetValue(this, enumValue);
                    }
                    else
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(this, convertedValue);
                    }


                }
            }
        }

    }
}
