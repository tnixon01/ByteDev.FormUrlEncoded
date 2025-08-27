using System;
using System.Collections.Generic;
using System.Linq;

namespace ByteDev.FormUrlEncoded
{
    /// <summary>
    /// Represents an attribute that specifies the property serializerName that is present in the form URL encoded data
    /// when serializing and deserializing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FormUrlEncodedPropertyNameAttribute : FormUrlEncodedAttribute
    {
        /// <summary>
        /// The serializerName of the property (ie, URL form field).
        /// </summary>
        public string SerializerName { get; }

        /// <summary>
        /// The Name of the property (ie, URL form field).
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

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUrlEncodedPropertyNameAttribute" /> class
        /// with the specified property serializerName.
        /// </summary>
        /// <param name="name">The URL form field serializerName of the property to use when serializing and deserializing.</param>
        public FormUrlEncodedPropertyNameAttribute(string name)
        {
            SerializerName = name;
            DeserializerNames = new List<string>() { name };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUrlEncodedPropertyNameAttribute"/> class
        /// with the specified property serializerName.
        /// </summary>
        /// <param name="serializerName">The serializerName of the property, used as the field's key when creating URL-encoded output.</param>
        /// <param name="deserialzerNames">Altername names that when found as a form field key in URL-encoded input will deserialize to this property. The serializerName is automatically included.</param>
        public FormUrlEncodedPropertyNameAttribute(string serializerName, string deserialzerNames)
        {
            SerializerName = serializerName;
            DeserializerNames = deserialzerNames.Split(',').ToList();
            DeserializerNames.Insert(0, SerializerName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUrlEncodedPropertyNameAttribute" /> class
        /// with the specified property serializerName.
        /// </summary>
        /// <param name="serializerName">The serializerName of the property, used as the field's key when creating URL-encoded output.</param>
        /// <param name="deserialzerNames">Altername names that when found as a form field key in URL-encoded input will deserialize to this property.</param>
        /// <param name="includeSerializerName">When true, adds <paramref name="serializerName"/> to <paramref name="deserialzerNames"/>."/></param>
        public FormUrlEncodedPropertyNameAttribute(string serializerName, string deserialzerNames, bool includeSerializerName)
        {
            SerializerName = serializerName;
            DeserializerNames = deserialzerNames.Split(',').ToList();
            if (includeSerializerName)
                DeserializerNames.Insert(0, SerializerName);
        }
    }
}