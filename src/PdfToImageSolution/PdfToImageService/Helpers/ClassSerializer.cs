using Newtonsoft.Json;
using System.Text;

namespace PdfToImageService.Helpers
{
    /// <summary>
    /// Serialize and deserialize class to byte array.
    /// Serialize to json string and then convert to byte array.
    /// Deserialize from byte array to json string and then convert to class.
    /// </summary>
    public class ClassSerializer
    {
        public static byte[] Serialize<T>(T classValue)
        {
            if (classValue == null)
                throw new ArgumentNullException(nameof(classValue));

            string jsonString = JsonConvert.SerializeObject(classValue);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public static T? Deserialize<T>(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            string jsonString = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
