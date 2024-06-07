using System.Text;

namespace CallPdfToImageService.Helpers.Tests
{
    [TestClass()]
    public class ByteArraySerializerTests
    {
        [TestMethod()]
        public void SerializeTest()
        {
            // Algorithm:
            // Create a list of byte arrays
            // Serialize it to byte array
            // Deserialize it back to list of byte arrays
            // Compare the original list with the deserialized list
            // if lists are equal, the test is passed
            // if not, the test is failed

            var originalList = new List<byte[]>
            {
                Encoding.UTF8.GetBytes("Test1"),
                Encoding.UTF8.GetBytes("Test2"),
                Encoding.UTF8.GetBytes("Test3")
            };

            // Act
            var serializer = new ByteArraySerializer();
            var serialized = ByteArraySerializer.Serialize(originalList);
            var deserializedList = ByteArraySerializer.Deserialize(serialized);

            // Assert
            for (int i = 0; i < originalList.Count; i++)
            {
                CollectionAssert.AreEqual(originalList[i], deserializedList[i]);
            }
        }
    }
}