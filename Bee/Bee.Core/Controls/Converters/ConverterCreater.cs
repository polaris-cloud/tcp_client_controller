using System;
using System.Collections.Generic;

namespace Bee.Core.Controls.Converters;

internal class ConverterCreater
{
    private static readonly Dictionary<Type, object> ConverterMap = new Dictionary<Type, object>();

    public static T Get<T>() where T : new()
    {
        if (!ConverterCreater.ConverterMap.ContainsKey(typeof(T)))
            ConverterCreater.ConverterMap.Add(typeof(T), (object)new T());
        return (T)ConverterCreater.ConverterMap[typeof(T)];
    }
}