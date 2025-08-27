using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ByteDev.Reflection;

namespace ByteDev.FormUrlEncoded
{
    internal static class PropertyInfoExtensions
    {
        /// <summary>
        /// Retrieves the form field name to use when serializing output. 
        /// </summary>
        /// <param name="source"></param>
        /// <returns>
        /// If no <see cref="FormUrlEncodedPropertyNameAttribute"/> exists on the property, return the object property's name.
        /// If a <see cref="FormUrlEncodedPropertyNameAttribute"/> exists on the property, returns <see cref="FormUrlEncodedPropertyNameAttribute.SerializerName"/>.
        /// </returns>
        public static string GetAttributeOrPropertyName(this PropertyInfo source)
        {
            var attribute = source.GetCustomAttributes(typeof(FormUrlEncodedPropertyNameAttribute), false).SingleOrDefault();

            if (attribute is FormUrlEncodedPropertyNameAttribute furpnAttribute)
            {
                if (!string.IsNullOrEmpty(furpnAttribute.SerializerName))
                    return furpnAttribute.SerializerName;
            }
                
            return source.Name;
        }

        public static string GetAttributeName(this PropertyInfo source)
        {
            var attribute = (FormUrlEncodedPropertyNameAttribute)source
                .GetCustomAttributes(typeof(FormUrlEncodedPropertyNameAttribute), false)
                .Single();

            return attribute.SerializerName;
        }

        // Tim did this, 2025-08-08, Williwaw Campground, Portage, AK
        public static List<string> GetDeserializerKeyNames(this PropertyInfo source)
        {
            var attribute = (FormUrlEncodedPropertyNameAttribute)source
                .GetCustomAttributes(typeof(FormUrlEncodedPropertyNameAttribute), false)
                .Single();

            if (attribute == null || attribute.DeserializerNames == null || attribute.DeserializerNames.Count == 0)
            {
                return new List<string>();  
            }
            else
            {                
                return attribute.DeserializerNames;
            }
        }

        public static bool HasIgnoreAttribute(this PropertyInfo source)
        {
            return source.HasAttribute<FormUrlEncodedIgnoreAttribute>();
        }

        public static bool HasValidPropertyNameAttribute(this PropertyInfo source)
        {
            var attr = source.GetAttribute<FormUrlEncodedPropertyNameAttribute>();

            if (attr == null)
                return false;

            if (attr.DeserializerNames == null || attr.DeserializerNames.Count == 0)
                return false;

            if (attr.DeserializerNames.First() == null || attr.DeserializerNames.First() == string.Empty)
                return false;

            return true;
        }

        public static bool HasValueConverterAttribute(this PropertyInfo source)
        {
            return source.HasAttribute<FormUrlEncodedValueConverterAttribute>();
        }

        public static bool IsTypeList(this PropertyInfo source)
        {
            return typeof(IList).IsAssignableFrom(source.PropertyType);
        }
    }
}