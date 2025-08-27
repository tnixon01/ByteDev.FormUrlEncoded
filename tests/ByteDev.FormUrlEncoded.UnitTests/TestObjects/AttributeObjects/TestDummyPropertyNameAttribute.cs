using System.Collections.Generic;

namespace ByteDev.FormUrlEncoded.UnitTests.TestObjects.AttributeObjects
{
    internal class TestDummyPropertyNameAttribute
    {
        /// <summary>
        /// There is no <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property.
        /// </summary>
        public string NoAttributeProperty1 { get; set; }

        /// <summary>
        /// There is no <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property.
        /// </summary>
        public string NoAttributeProperty2 { get; set; }

        /// <summary>
        /// The <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property is "DifferentName1".
        /// </summary>
        [FormUrlEncodedPropertyName("DifferentName1")]
        public string DifferentNameAttributeProperty1 { get; set; }

        /// <summary>
        /// The <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property is "DifferentName2".
        /// </summary>
        [FormUrlEncodedPropertyName("DifferentName2")]
        public string DifferentNameAttributeProperty2 { get; set; }

        /// <summary>
        /// The <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property is "List".
        /// </summary>
        [FormUrlEncodedPropertyName("List")]
        public List<string> Items { get; set; }

        /// <summary>
        /// The <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property is "".
        /// </summary>
        [FormUrlEncodedPropertyName("")]
        public string EmptyAttributeProperty { get; set; }

        /// <summary>
        /// The <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property is null.
        /// </summary>
        [FormUrlEncodedPropertyName(null)]
        public string NullAttributeProperty { get; set; }

        /// <summary>
        /// The <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property is includes
        /// Deserializer aliases "IncAlias1,IncAlias2" and the SerialzerName "MultipleAliasesInclusiveOut".
        /// </summary>
        [FormUrlEncodedPropertyName("MultipleAliasesInclusiveOut", "IncAlias1,IncAlias2")]
        public string MultipleAliasesInclusiveProperty { get; set; }

        /// <summary>
        /// The <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property is includes
        /// Deserializer aliases "ExcAlias1,ExcAlias2" and excludes the serializer name "MultipleAliasesExclusiveOut".
        /// </summary>
        [FormUrlEncodedPropertyName("MultipleAliasesExclusiveOut", "ExcAlias1,ExcAlias2", false)]
        public string MultipleAliasesExclusiveProperty { get; set; }
    }
}