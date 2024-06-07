//using System;
//using System.Collections.Generic;
//using System.Net.Http;
//using System.Net.Http.Headers;

//namespace CallPdfToImageService
//{
//    public class PdfConverterClient
//    {
//        private readonly string _baseUrl;
//        private readonly HttpClient _httpClient;

//        public PdfConverterClient(string baseUrl)
//        {
//            _baseUrl = baseUrl;
//            _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
//        }

//        public IEnumerable<byte[]> ConvertPdf(byte[] pdfContents)
//        {
//            var fileId = UploadPdf(pdfContents);
//            var totalPages = GetTotalPages(fileId);

//            for (int i = 1; i <= totalPages; i++)
//            {
//                var page = DownloadPage(fileId, i);
//                yield return page;
//            }
//        }

//        private string UploadPdf(byte[] pdfContents)
//        {
//            using (var content = new MultipartFormDataContent())
//            {
//                var pdfContent = new ByteArrayContent(pdfContents);
//                pdfContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
//                content.Add(pdfContent, "file", "document.pdf");

//                var response = _httpClient.PostAsync("/convert_pdf", content).Result;
//                response.EnsureSuccessStatusCode();

//                var responseContent = response.Content.ReadAsStringAsync().Result;
//                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);

//                if (result.status != "success")
//                {
//                    throw new Exception("Failed to upload and convert PDF: " + result.message);
//                }

//                return result.file_id;
//            }
//        }

//        private int GetTotalPages(string fileId)
//        {
//            var request = new HttpRequestMessage(HttpMethod.Get, $"/convert_pdf/{fileId}/1");
//            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

//            var response = _httpClient.SendAsync(request).Result;
//            response.EnsureSuccessStatusCode();

//            var responseContent = response.Content.ReadAsStringAsync().Result;
//            dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);

//            return result.total_pages;
//        }

//        private byte[] DownloadPage(string fileId, int pageNumber)
//        {
//            var response = _httpClient.GetAsync($"/convert_pdf/{fileId}/{pageNumber}").Result;
//            response.EnsureSuccessStatusCode();

//            return response.Content.ReadAsByteArrayAsync().Result;
//        }
//    }
//}
