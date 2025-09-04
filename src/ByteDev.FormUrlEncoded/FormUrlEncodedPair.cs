using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ByteDev.FormUrlEncoded
{
    internal class FormUrlEncodedPair
    {
        public string PropertyName { get; }

        public string Value { get; }

        public bool HasValue => Value != string.Empty;

        public bool IsValid => (PropertyName != null) && (PropertyName != string.Empty) && (Value != string.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUrlEncodedPair"/> class, 
        /// decoding a URL-encoded string pair (like 'key=value') into a FormUrlEncodedPair object,
        /// matching the key name to a property found in <paramref name="propertyMap"/>.
        /// </summary>
        /// <param name="pair">URL-encoded string pair (like 'key=value').</param>
        /// <param name="options"></param>
        /// <param name="propertyMap">SortedDictionary of URL names mapped to property names, returned from <see cref="FormUrlEncodedSerializer.GetDeserializerPropertyKeyMapping"/>.</param>
        public FormUrlEncodedPair(
            string pair,
            DeserializeOptions options,
            SortedDictionary<string, string> propertyMap)
        {
            var pairArray = pair.Split('=');
            // pairArray[0] is the urlKey, pairArray[1] is the value (still urlEncoded).

            // See if the urlKey matches a mapping in the properties -- if it's not there, it's not meant to map
            if (propertyMap.TryGetValue(pairArray[0], out string keyName))
                PropertyName = keyName;

            // if there's a value (ie, a second item in pairArray), then decode and assign
            if (pairArray.Length == 2)
                Value = UrlEncoder.Decode(pairArray[1], options);
            else
                Value = string.Empty;
        }

        [Obsolete("Deprecated in favor of new property attribute mapping method.")]
        public FormUrlEncodedPair(string pair, DeserializeOptions options, List<PropertyInfo> propertiesWithAttr)
        {
            var pairArray = pair.Split('=');
            // pairArray[0] is the urlName, pairArray[1] is the value (still urlEncoded).

            // See if the (url)name matches a mapping in the properties
            PropertyInfo attrProperty = propertiesWithAttr.GetByUrlKeyName(pairArray[0], options.StringComparer);

            if (attrProperty == null) // if there's no matching property, then match UrlName to PropertyName
                PropertyName = UrlEncoder.Decode(pairArray[0], options);
            else // otherwise use the name from the attribute
                PropertyName = attrProperty.Name;

            // if there's a value (ie, a second item in pairArray), then decode and assign
            if (pairArray.Length == 2)
                Value = UrlEncoder.Decode(pairArray[1], options);
            else
                Value = string.Empty;
        }
    }
}