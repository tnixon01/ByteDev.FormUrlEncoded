namespace ByteDev.FormUrlEncoded.UnitTests.TestObjects.AttributeObjects
{
    internal class TestDummyValueConverterAndNamedPropertyAttribute
    {
        /// <summary>
        /// The <see cref="FormUrlEncodedPropertyNameAttribute"/> applied to this property is "OfficeColor".
        /// </summary>
        [TestDummyColorValueConverter]
        [FormUrlEncodedPropertyName("OfficeColor")]
        public System.Drawing.Color OfficeWallColor { get; set; }
    }
}