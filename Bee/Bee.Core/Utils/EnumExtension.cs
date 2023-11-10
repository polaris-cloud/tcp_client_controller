using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Bee.Core.Utils
{
    public static class EnumExtension
    {
        public static string GetDisplay<T>(this T value)
        {
            var type = value.GetType();
            FieldInfo field = type.GetField(System.Enum.GetName(type, value));
            DisplayAttribute descAttr =
                Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) as DisplayAttribute;
            if (descAttr == null)
            {
                return value.ToString();
            }

            return TranslationUtil.GetTranslate(descAttr.Name, descAttr.ResourceType);
        }
    }
}
