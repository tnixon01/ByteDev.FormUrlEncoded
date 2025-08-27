using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ByteDev.FormUrlEncoded
{
    internal static class PropertyInfoListExtensions
    {
        [Obsolete("Renamed/changed to GetByUrlKeyName")]
        public static PropertyInfo GetByAttributeName(this List<PropertyInfo> source, string name)
        {
            return source.SingleOrDefault(p => p.GetAttributeName() == name);
        }

        public static PropertyInfo GetByUrlKeyName(this List<PropertyInfo> source, string name)
        {
            return GetByUrlKeyName(source, name, StringComparer.Ordinal);
        }

        public static PropertyInfo GetByUrlKeyName(this List<PropertyInfo> source, string name, IEqualityComparer<string> stringComparer)
        {
            return source.SingleOrDefault(p => p.GetDeserializerKeyNames().Contains(name, stringComparer));
        }
    }
}