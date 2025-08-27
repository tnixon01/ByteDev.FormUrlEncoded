using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Xml.Linq;
using ByteDev.Reflection;

namespace ByteDev.FormUrlEncoded
{
    /// <summary>
    /// Represents a serializer for form URL encoded (x-www-form-urlencoded) content.
    /// </summary>
    public static class FormUrlEncodedSerializer
    {
        /// <summary>
        /// Serialize an object to a form URL encoded string.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>Form URL encoded string.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="obj" /> is null.</exception>
        public static string Serialize(object obj)
        {
            return Serialize(obj, new SerializeOptions());
        }

        // todo: integrate Cached map into Serialize routines

        /// <summary>
        /// Serialize an object to a form URL encoded string.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="options">Serialize options.</param>
        /// <returns>Form URL encoded string.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="obj" /> is null.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="options" /> is null.</exception>
        public static string Serialize(object obj, SerializeOptions options)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var sb = new StringBuilder();

            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.HasIgnoreAttribute())
                    continue;

                object propertyValue = propertyInfo.GetValue(obj);

                if (propertyValue == null)
                {
                    if (options.IgnoreIfDefault || options.IgnoreIfNull)
                        continue;

                    sb.AppendKeyValue(propertyInfo.GetAttributeOrPropertyName(), string.Empty, options);
                }
                else
                {
                    if (options.IgnoreIfDefault)
                    {
                        var valuesDefault = propertyValue.GetType().GetDefault();

                        if (propertyValue.Equals(valuesDefault))
                            continue;
                    }

                    if (options.EnumHandling == EnumHandling.Number && propertyInfo.PropertyType.IsEnum)
                    {
                        propertyValue = GetEnumNumberFromName(propertyValue);
                    }

                    if (propertyInfo.HasValueConverterAttribute())
                    {
                        var valueConverter = Attribute.GetCustomAttributes(propertyInfo, typeof(FormUrlEncodedValueConverterAttribute)).FirstOrDefault() as FormUrlEncodedValueConverterAttribute;
                        sb.AppendKeySequenceValue(propertyInfo.GetAttributeOrPropertyName(), valueConverter.ConvertToString(propertyValue), options);
                        continue;
                    }

                    if (propertyInfo.IsTypeList())
                    {
                        var sequence = propertyValue as IEnumerable;

                        sb.AppendKeySequenceValue(propertyInfo.GetAttributeOrPropertyName(), sequence.ToCsv(), options);
                    }
                    else
                    {
                        sb.AppendKeyValue(propertyInfo.GetAttributeOrPropertyName(), propertyValue.ToString(), options);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Deserialize a form URL encoded string to an object.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to.</typeparam>
        /// <param name="formUrlEncodedData">Form URL encoded string to deserialize.</param>
        /// <returns>Object of type <typeparamref name="T" />.</returns>
        /// <exception cref="T:System.ArgumentException"><paramref name="formUrlEncodedData" /> is null or empty.</exception>
        /// <exception cref="T:System.FormatException">Matching property's type cannot be set to the value.</exception>
        public static T Deserialize<T>(string formUrlEncodedData) where T : new()
        {
            return Deserialize<T>(formUrlEncodedData, new DeserializeOptions());
        }

        /// <summary>
        /// Deserialize a form URL encoded string to an object.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to.</typeparam>
        /// <param name="formUrlEncodedData">Form URL encoded string to deserialize.</param>
        /// <param name="options">Deserialize options.</param>
        /// <returns>Object of type <typeparamref name="T" />.</returns>
        /// <exception cref="T:System.ArgumentException"><paramref name="formUrlEncodedData" /> is null or empty.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="options" /> is null.</exception>
        /// <exception cref="T:System.FormatException">Matching property's type cannot be set to the value.</exception>
        public static T Deserialize<T>(string formUrlEncodedData, DeserializeOptions options)
            where T : new()
        {
            if (string.IsNullOrEmpty(formUrlEncodedData))
                throw new ArgumentException("Form URL encoded data was null or empty.", nameof(formUrlEncodedData));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var strPairs = formUrlEncodedData.Split('&');

            var obj = new T();

            SortedDictionary<string, string> propertyMap = GetPropertyKeyMapping<T>();

            foreach (string strPair in strPairs)
            {
                // todo: Maybe there's a clearer way to split the input pair and resolve the target property name
                var pair = new FormUrlEncodedPair(strPair, options, propertyMap);

                if (!pair.IsValid)
                    continue;

                var propertyInfo = typeof(T).GetProperty(pair.Name);

                if (propertyInfo == null) // || propertyInfo.HasIgnoreAttribute()) -- won't be in map if it has ignore attribute
                    continue;

                if (propertyInfo.HasValueConverterAttribute())
                {                    
                    var valueConverter = Attribute.GetCustomAttributes(propertyInfo, typeof(FormUrlEncodedValueConverterAttribute)).FirstOrDefault() as FormUrlEncodedValueConverterAttribute;
                    obj.SetPropertyValue(pair.Name, valueConverter.ConvertFromString(pair.Value));
                    continue;
                } 

                if (propertyInfo.IsTypeList())
                {
                    obj.SetPropertyValue(pair.Name, pair.Value.ToList(','));
                    continue;
                }

                // otherwise it's just a vanilla property, so set it
                obj.SetPropertyValue(pair.Name, pair.Value);
            }

            return obj;
        }

        // =================== CACHEING ===================

        private static MemoryCache _cache = MemoryCache.Default;
        
        /// <summary>
        /// Retrieves PropertyKeyMapping for a class from the cache, rebuilding when necessary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static SortedDictionary<string, string> GetPropertyKeyMapping<T>()
            where T : new()
        {
            var key = typeof(T).FullName;
            if (_cache.Contains(key))
            {
                return (SortedDictionary<string, string>)_cache.Get(key);
            }
            else
            {
                var data = BuildPropertyKeyMapping<T>();
                _cache.Add(key, data, DateTimeOffset.Now.AddHours(1));
                return data;
            }
        }

        // todo: documentation -- url keys are case insensitive
        private static SortedDictionary<string, string> BuildPropertyKeyMapping<T>()
            where T : new()
        {
            var outDict = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            List<PropertyInfo> allProperties = typeof(T).GetProperties().ToList();
            foreach (PropertyInfo prop in allProperties) 
            {
                if (!prop.HasIgnoreAttribute())
                {                    
                    if (prop.HasValidPropertyNameAttribute())
                    {
                        var aliases = prop.GetDeserializerKeyNames();
                        foreach (string alias in aliases)
                        {
                            if (alias != null && alias != string.Empty)
                                outDict[alias] = prop.Name;
                        }
                    } 
                    else
                    {
                        outDict[prop.Name] = prop.Name;
                    }
                }
            }
            return outDict;
        }

        private static object GetEnumNumberFromName(object propertyValue)
        {
            // e.g. input name: "Yellow", return number: 2
            return Convert.ChangeType(propertyValue, ((Enum)propertyValue).GetTypeCode());
        }
    }
}