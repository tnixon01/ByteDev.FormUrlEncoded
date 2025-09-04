using ByteDev.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;

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

            SortedDictionary<string, string> propertyMap = GetSerializerPropertyKeyMapping(obj.GetType());

            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in properties)
            {
                // looping through the hydrated object's properties
                // use that as cacheKey from cached dictionary, anything that is in it is to be used
                if (propertyMap.TryGetValue(propertyInfo.Name, out string mapName))
                {
                    object propertyValue = propertyInfo.GetValue(obj);

                    if (propertyValue == null)
                    {
                        if (options.IgnoreIfDefault || options.IgnoreIfNull)
                        {
                            continue;
                        }

                        sb.AppendKeyValue(mapName, string.Empty, options);
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
                            sb.AppendKeySequenceValue(mapName, valueConverter.ConvertToString(propertyValue), options);
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

                // original code below here ---------
                #region Original Code
                // if (propertyInfo.HasIgnoreAttribute())
                //     continue;

                // object propertyValue = propertyInfo.GetValue(obj);

                // if (propertyValue == null)
                // {
                //     if (options.IgnoreIfDefault || options.IgnoreIfNull)
                //         continue;

                // sb.AppendKeyValue(propertyInfo.GetAttributeOrPropertyName(), string.Empty, options);
                // }
                // else
                // {
                //     if (options.IgnoreIfDefault)
                //     {
                //         var valuesDefault = propertyValue.GetType().GetDefault();

                // if (propertyValue.Equals(valuesDefault))
                //             continue;
                //     }

                // if (options.EnumHandling == EnumHandling.Number && propertyInfo.PropertyType.IsEnum)
                //     {
                //         propertyValue = GetEnumNumberFromName(propertyValue);
                //     }

                // if (propertyInfo.HasValueConverterAttribute())
                //     {
                //         var valueConverter = Attribute.GetCustomAttributes(propertyInfo, typeof(FormUrlEncodedValueConverterAttribute)).FirstOrDefault() as FormUrlEncodedValueConverterAttribute;
                //         sb.AppendKeySequenceValue(propertyInfo.GetAttributeOrPropertyName(), valueConverter.ConvertToString(propertyValue), options);
                //         continue;
                //     }

                // if (propertyInfo.IsTypeList())
                //     {
                //         var sequence = propertyValue as IEnumerable;

                // sb.AppendKeySequenceValue(propertyInfo.GetAttributeOrPropertyName(), sequence.ToCsv(), options);
                //     }
                //     else
                //     {
                //         sb.AppendKeyValue(propertyInfo.GetAttributeOrPropertyName(), propertyValue.ToString(), options);
                //     }
                // }
                #endregion
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

            SortedDictionary<string, string> propertyMap = GetDeserializerPropertyKeyMapping(typeof(T));

            foreach (string strPair in strPairs)
            {
                var pair = new FormUrlEncodedPair(strPair, options, propertyMap);

                if (!pair.IsValid)
                    continue;

                var propertyInfo = typeof(T).GetProperty(pair.PropertyName);

                if (propertyInfo == null) // || propertyInfo.HasIgnoreAttribute()) -- won't be in map if it has ignore attribute
                    continue;

                if (propertyInfo.HasValueConverterAttribute())
                {
                    var valueConverter = Attribute.GetCustomAttributes(propertyInfo, typeof(FormUrlEncodedValueConverterAttribute)).FirstOrDefault() as FormUrlEncodedValueConverterAttribute;
                    obj.SetPropertyValue(pair.PropertyName, valueConverter.ConvertFromString(pair.Value));
                    continue;
                } 

                if (propertyInfo.IsTypeList())
                {
                    obj.SetPropertyValue(pair.PropertyName, pair.Value.ToList(','));
                    continue;
                }

                // otherwise it's just a vanilla property, so set it
                obj.SetPropertyValue(pair.PropertyName, pair.Value);
            }

            return obj;
        }

        private static readonly MemoryCache _cache = MemoryCache.Default;

        #region Deserializer Cache

        /// <summary>
        /// Retrieves PropertyKeyMapping for a class from the cache, rebuilding when necessary.
        /// </summary>
        private static SortedDictionary<string, string> GetDeserializerPropertyKeyMapping(Type type)
        {
            string cacheKey = "FormUrlEncodedDeserializerMaps." + type.FullName;
            object fromCache = _cache.Get(cacheKey);
            if (fromCache != null) // (_cache.Contains(cacheKey))
            {
                Debug.Print($"D -- {type.Name} pulled from cache");
                return (SortedDictionary<string, string>)fromCache;
            }
            else
            {
                SortedDictionary<string, string> data = BuildDeserializerPropertyKeyMapping(type);
                _cache.Add(cacheKey, data, DateTimeOffset.Now.AddHours(1));
                Debug.Print($"D {type.Name} added to cache -----");
                return data;
            }
        }

        /// <summary>
        /// Builds property key map in the deserialization direction (UrlKeys-to-Properties). 
        /// URL keys are generally treated as case-insensitive, so the resulting dictionary ignores case.
        /// </summary>
        /// <param name="type">Type from which to construct a key-to-property map</param>
        private static SortedDictionary<string, string> BuildDeserializerPropertyKeyMapping(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("parameter 'type' in BuildDeserializerPropertyKeyMapping.");

            var outDict = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            List<PropertyInfo> allProperties = type.GetProperties().ToList();
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

        #endregion

        #region Serializer Cache

        private static SortedDictionary<string, string> GetSerializerPropertyKeyMapping(Type type)
        {
            string cacheKey = "FormUrlEncodedSerializerMaps." + type.FullName;
            object fromCache = _cache.Get(cacheKey);
            if (fromCache != null) // (_cache.Contains(cacheKey))
            {
                Debug.Print($"S ++ {type.Name} pulled from cache");
                return (SortedDictionary<string, string>)fromCache;
            }
            else
            {
                var data = BuildSerializerPropertyKeyMapping(type);
                _cache.Add(cacheKey, data, DateTimeOffset.Now.AddHours(1));
                Debug.Print($"S {type.Name} added to cache +++++");
                return data;
            }
        }

        /// <summary>
        /// Builds property key map in the serialization direction (Properties-to-UrlKeys). 
        /// Property names are treated as case-insensitive.
        /// </summary>
        /// <param name="type">Type from which to construct a property-to-key map</param>
        private static SortedDictionary<string, string> BuildSerializerPropertyKeyMapping(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("parameter 'type' in BuildSerializerPropertyKeyMapping.");

            SortedDictionary<string, string> outDict = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            List<PropertyInfo> allProperties = type.GetProperties().ToList();
            foreach (PropertyInfo prop in allProperties)
            {
                if (!prop.HasIgnoreAttribute())
                {
                    if (prop.HasValidPropertyNameAttribute())
                    {
                        var attr = prop.GetAttribute<FormUrlEncodedPropertyNameAttribute>();
                        outDict[prop.Name] = attr.SerializerName;
                    }
                    else
                    {
                        outDict[prop.Name] = prop.Name;
                    }
                }
            }
            return outDict;
        }

        #endregion

        private static object GetEnumNumberFromName(object propertyValue)
        {
            // e.g. input name: "Yellow", return number: 2
            return Convert.ChangeType(propertyValue, ((Enum)propertyValue).GetTypeCode());
        }
    }
}