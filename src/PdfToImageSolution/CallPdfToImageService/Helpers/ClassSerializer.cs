//using Newtonsoft.Json;
//using System.Text;
//using System;

//namespace PdfToImageService.Helpers
//{
//    public class ClassSerializer
//    {
//        public static byte[] Serialize<T>(T responseDto)
//        {
//            if (responseDto == null)
//                throw new ArgumentNullException(nameof(responseDto));

//            string jsonString = JsonConvert.SerializeObject(responseDto);
//            return Encoding.UTF8.GetBytes(jsonString);
//        }

//        public static T Deserialize<T>(byte[] data)
//        {
//            if (data == null)
//                throw new ArgumentNullException(nameof(data));

//            string jsonString = Encoding.UTF8.GetString(data);
//            return JsonConvert.DeserializeObject<T>(jsonString);
//        }
//    }
//}
