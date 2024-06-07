//using System.Collections.Generic;
//using System.IO;

//namespace CallPdfToImageService
//{


//    // Пример использования
//    public class HelperTestService
//    {
//        public static void TestService(string[] args)
//        {
//            var pdfConverterClient = new PdfConverterClient("https://yourservice.com");

//            byte[] pdfContents = File.ReadAllBytes("path/to/your.pdf");

//            IEnumerable<byte[]> jpegPages = pdfConverterClient.ConvertPdf(pdfContents);

//            int pageIndex = 1;
//            foreach (var jpegPage in jpegPages)
//            {
//                File.WriteAllBytes($"page_{pageIndex}.jpeg", jpegPage);
//                pageIndex++;
//            }
//        }
//    }
//}
