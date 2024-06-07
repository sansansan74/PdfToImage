using PdfToImageClient.Dto;
using PdfToImageClient.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PdfToImageClient.Classes
{
    /// <summary>
    /// Represents a client service for converting PDF files to images using a REST API.
    /// Client write on .NET 4.8
    /// This is spetially, because big complex program is written on .NET 4.8
    /// And there is no GRPC in .NET 4.8!!!
    /// We need fast binary data transfer, so we write our own binary serializer
    /// </summary>
    public class PdfToImagesClientService : IDisposable
    {
        string RequestUri { get; set; }
        ResponseDto _responseDto;
        HttpClient _client;

        // flag to indicate that the cache was used on the server and should be cleared
        bool _useCache = false;
        public ResponseDto ResponseDto { get => _responseDto; }
        public PdfToImagesClientService(string requestUri = null, int timeOutSeconds = 60*10)
        {
            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeOutSeconds)
            };

            if (!string.IsNullOrEmpty(requestUri))
                RequestUri = requestUri;
        }


        public List<byte[]> ConvertFirst(byte[] pdfFileContents)
        {
            byte[] content = ReadContentFirst(pdfFileContents);
            return DecodeContentToPagesList(content, true);
        }

        public List<byte[]> ConvertNext(int startPageNumber, string fileId)
        {
            _useCache = true;
            byte[] content = ReadContentNext(startPageNumber, fileId);
            return DecodeContentToPagesList(content, false);
        }

        private List<byte[]> DecodeContentToPagesList(byte[] content, bool isFirst)
        {
            if (content is null)
                throw new Exception("Ошибка вызова конвертера с пустыми данными");

            List<byte[]> jpegPagesList = ByteArraySerializer.Deserialize(content);
            if (jpegPagesList.Count == 0)
                throw new Exception("Ошибка вызова конвертера");

            // descriptor is the first element in the list
            ReadDescriptor(isFirst, jpegPagesList);
            jpegPagesList.RemoveAt(0);

            if (_responseDto.ErrorMessage != "OK")
                throw new Exception(_responseDto.ErrorMessage);

            return jpegPagesList;
        }

        private void ReadDescriptor(bool isFirst, List<byte[]> jpegPagesList)
        {
            // we get amount of pages only from the first portion of pages
            var prev = _responseDto;
            _responseDto = ClassSerializer.Deserialize<ResponseDto>(jpegPagesList[0]);
            if (isFirst)
                return;

            // get amount of pages from the previous descriptor
            _responseDto.AmountPages = prev.AmountPages;
        }

        byte[] ReadContentFirst(byte[] pdfFileContents)
        {
            using (var fileStream = new MemoryStream(pdfFileContents))
            {
                // Create content for the file
                StreamContent fileContent = CreateFileContent(fileStream);
                // Create MultipartFormDataContent and add the file content
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(fileContent);

                    // Post the multipart/form-data content to the REST service
                    HttpResponseMessage response = _client.PostAsync(RequestUri, formData).Result;

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Failed to convert PDF: {response.StatusCode}");

                    var result = response.Content.ReadAsByteArrayAsync().Result;
                    return result;
                }
            }
        }

        byte[] ReadContentNext(int startPageNumber, string fileId)
        {
            string requestUri = $"{RequestUri}/DownloadPdfTail/{fileId}/{startPageNumber}";
            var response = _client.GetAsync(requestUri).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsByteArrayAsync().Result;

            throw new Exception($"Failed to get data: {response.StatusCode}");
        }

        private static StreamContent CreateFileContent(MemoryStream fileStream)
        {
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",  // Name of the form data
                FileName = "\"my.pdf\""  // Name of the file
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return fileContent;
        }

        public IEnumerable<byte[]> ConvertToYield(byte[] fpdFileContents)
        {
            // get first portion of pages
            var jpegPagesList = ConvertFirst(fpdFileContents);

            int pagesProcessed = 0;
            foreach (var page in jpegPagesList)
            {
                pagesProcessed++;
                yield return page;
            }

            int amountPages = ResponseDto.AmountPages;
            string fileId = ResponseDto.FileId;

            // get next portions of pages
            while (pagesProcessed < amountPages)
            {
                jpegPagesList = ConvertNext(pagesProcessed + 1, fileId);

                foreach (var page in jpegPagesList)
                {
                    pagesProcessed++;
                    yield return page;
                }
            }

            ClearServerCache();
        }

        public void ClearServerCache()
        {
            if (!_useCache)
                return;

            _useCache = false;

            string requestUri = $"{RequestUri}/ClearCache/{_responseDto.FileId}";
            var response = _client.DeleteAsync(requestUri).Result;

            if (response.IsSuccessStatusCode)
                return;
        }

        public void Dispose()
        {
            ((IDisposable)_client).Dispose();
            _client = null;
        }
    }
}
