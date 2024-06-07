namespace PdfToImageService.Helpers.Tests
{
    [TestClass()]
    public class ClassSerializerTests
    {
        public class DtoClass
        {
            public int IntField { get; set; }
            public string StringField { get; set; }
            public bool BoolField { get; set; }
        }

        [TestMethod()]
        public void SerializeTest()
        {
            // Algorithm:
            // 1. Create a new instance of the DtoClass.
            // 2. Fill the fields of the DtoClass with some values.
            // 3. Serialize the DtoClass instance to byte array.
            // 4. Deserialize the byte array to a new instance of the DtoClass.
            // 5. Check that the fields of the original and deserialized instances are equal.

            // Arrange
            var serializer = new ClassSerializer();
            var originalObject = new DtoClass { IntField = 123, StringField = "Test", BoolField = true };

            // Act
            byte[] serialized = ClassSerializer.Serialize(originalObject);
            DtoClass deserializedObject = ClassSerializer.Deserialize<DtoClass>(serialized);

            // Assert
            Assert.AreEqual(originalObject.IntField, deserializedObject.IntField);
            Assert.AreEqual(originalObject.StringField, deserializedObject.StringField);
            Assert.AreEqual(originalObject.BoolField, deserializedObject.BoolField);
        }
    }
}