using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ByteDev.FormUrlEncoded
{
    internal class FormUrlEncodedPair
    {
        public string Name { get; }

        public string Value { get; }

        public bool HasValue => Value != string.Empty;

        public bool IsValid => (Name != null) && (Name != string.Empty) && (Value != string.Empty);

        public FormUrlEncodedPair(
            string pair, 
            DeserializeOptions options, 
            SortedDictionary<string, string> propertyMap)
        {
            var pairArray = pair.Split('=');
            // pairArray[0] is the urlKey, pairArray[1] is the value (still urlEncoded).

            // See if the urlKey matches a mapping in the properties
            if (propertyMap.TryGetValue(pairArray[0], out string keyName))
                Name = keyName;

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
                Name = UrlEncoder.Decode(pairArray[0], options);
            else // otherwise use the name from the attribute
                Name = attrProperty.Name;

            // if there's a value (ie, a second item in pairArray), then decode and assign
            if (pairArray.Length == 2)
                Value = UrlEncoder.Decode(pairArray[1], options);
            else
                Value = string.Empty;
        }
    }
}