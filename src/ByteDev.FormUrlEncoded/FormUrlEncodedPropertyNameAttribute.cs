using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ByteDev.FormUrlEncoded
{
    // Forms:
    // * FormUrlEncodedPropertyName() -- ignore, use property name
    // * FormUrlEncodedPropertyName(null) -- ignore, use property name
    // * FormUrlEncodedPropertyName("AltName") -- Alternate name to use while serializing and deserializing
    // * FormUrlEncodedPropertyName("AltName, AltName2") -- Alternate name to use while serializing and deserializing
    // * FormUrlEncodedPropertyName("AltOutName", "AltInName1,AltInName2") -- Alternate names to use while serializing and deserializing
    // * FormUrlEncodedPropertyName("AltOutName", "AltInName1,AltInName2", false) -- Alternate names to use while serializing and deserializing, will not include serializer name in deserializers

    /// <summary>
    /// Represents an attribute that specifies the property serializerName that is present in the form URL encoded data
    /// when serializing and deserializing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FormUrlEncodedPropertyNameAttribute : FormUrlEncodedAttribute
    {
        #region Properties
        /// <summary>
        /// The serializerName of the property (ie, URL form field).
        /// </summary>
        public string SerializerName { get; }

        /// <summary>
        /// The PropertyName of the property (ie, URL form field).
        /// </summary>
        [Obsolete("Use SerializerName instead")]
        public string Name { get; }

        /// <summary>
        /// Alternate names to use when deserializing, ie, other names that may appear in the URL-encoded string
        /// that will deserialize to this field.
        /// </summary>
        public List<string> DeserializerNames { get;  } = new List<string>();

        /// <summary>
        /// When true, uses only <see cref="DeserializerNames"/> when deserialzing. Any URL field named <see cref="SerializerName"/> will not be assigned to this property.
        /// </summary>
        public bool ExcludeSerializerName { get; } = false;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUrlEncodedPropertyNameAttribute" /> class
        /// which will use an alternate name for serialization and deserialization.
        /// </summary>
        /// <param name="name">The URL form field serializerName of the property to use when serializing and deserializing.</param>
        public FormUrlEncodedPropertyNameAttribute(string name)
        {
            // if this is a CSV with multiple entries, then first is serializer and all are deserializers
            if (name != null)
            {
                string[] names = name.Replace(" ", string.Empty).Split(',');
                if (names.Length == 1)
                {
                    SerializerName = name;
                    DeserializerNames = new List<string>() { name };
                }
                else if (names.Length > 1)
                {
                    SerializerName = names[0];
                    DeserializerNames = names.ToList();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUrlEncodedPropertyNameAttribute"/> class
        /// with alternate names for serialization and deserialization. The serialization key will be included in 
        /// deserializer keys.
        /// </summary>
        /// <param name="serializerName">The serializerName of the property, used as the field's key when creating URL-encoded output.</param>
        /// <param name="deserialzerNames">Altername names that when found as a form field key in URL-encoded input will deserialize to this property. The serializerName is automatically included.</param>
        public FormUrlEncodedPropertyNameAttribute(string serializerName, 
                                                   string deserialzerNames)
            : this(serializerName, deserialzerNames, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUrlEncodedPropertyNameAttribute" /> class
        /// with alternate names for serialization and deserialization. 
        /// The serialization key will optionally be included in deserializer keys.
        /// </summary>
        /// <param name="serializerName">The serializerName of the property, used as the field's key when creating URL-encoded output.</param>
        /// <param name="deserialzerNames">Altername names that when found as a form field key in URL-encoded input will deserialize to this property.</param>
        /// <param name="includeSerializerName">When true, adds <paramref name="serializerName"/> to <paramref name="deserialzerNames"/>."/></param>
        public FormUrlEncodedPropertyNameAttribute(string serializerName, 
                                                   string deserialzerNames, 
                                                   bool includeSerializerName)
        {
            if (serializerName.Split(',').Length > 1)
                throw new ArgumentException("Argument 'serializerName' contains more than one name");

            SerializerName = serializerName;
            DeserializerNames = deserialzerNames.Replace(" ", string.Empty).Split(',').ToList();

            if (includeSerializerName)
                DeserializerNames.Insert(0, SerializerName);
        }

        #endregion
    }
}