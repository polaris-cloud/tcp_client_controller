using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Bee.Core.Utils
{
    public static class EnumExtension
    {
        public static string GetDisplay<T>(this T value) where T : Enum
        {
            var field = value.GetType().GetField(value.ToString());

            if (field == null)
            {
                throw new ArgumentException($"Enum field not found for value {value}");
            }

            if (Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) is DisplayAttribute descAttr)
            {
                return TranslationUtil.GetTranslate(descAttr.Name, descAttr.ResourceType);
            }

            return value.ToString();
        }
    }
}
