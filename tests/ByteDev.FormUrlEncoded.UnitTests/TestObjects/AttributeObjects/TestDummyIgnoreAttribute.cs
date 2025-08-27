namespace ByteDev.FormUrlEncoded.UnitTests.TestObjects.AttributeObjects
{
    internal class TestDummyIgnoreAttribute
    {
        /// <summary>
        /// This property is NOT ignored.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This property is ignored.
        /// </summary>
        [FormUrlEncodedIgnore]
        public string Email { get; set; }

        /// <summary>
        /// This property is ignored.
        /// </summary>
        [FormUrlEncodedIgnore]
        public int Age { get; set; }
    }
}