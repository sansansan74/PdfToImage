using CallPdfToImageService.Helpers;
using PDFtoImage;
using PdfToImageService.Dto;
using PdfToImageService.Helpers;
using SkiaSharp;
using System.Text;

namespace PdfToImageService.Services
{
    /// <summary>
    /// Class for converting pdf file to jpeg pages list.
    /// </summary>
    public class PdfProcessor : PageCacheManager, IPdfProcessor
    {
        /// <summary>
        /// First page number of the diapazon
        /// </summary>
        int _diapazonStartPage;

        /// <summary>
        /// Serialized response data
        /// </summary>
        public byte[]? SerializedData { get; set; }

        public int ResponseLen => SerializedData?.Length ?? 0;



        readonly IConfiguration _configuration;

        public PdfProcessor(IConfiguration configuration)
        {
            _diapazonStartPage = -1;
            _configuration = configuration;
        }


        public void Init(string? fileId, int diapazonStartPage) 
        {
            base.Init(fileId);
            _diapazonStartPage = diapazonStartPage;
        }

        private const int JpegQualityPersent = 90;

        // Convert pdf file to jpeg pages list
        // If the file is too large, the rest of pages are saved to disk
        // returns DataPortionDescriptor:
        //      AmountPages,
        //      FileId,
        //      JpegPagesList - list of first jpeg pages
        public async Task<DataPortionDescriptor> ConvertPdfFileToJpegPagesList(byte[] contents)
        {
            var jpegPagesList = new List<byte[]>();

            // count amount bytes in the list
            int currentPageNumber = 0;
            int amountBytes = 0;

            IAsyncEnumerable<SKBitmap> bitmaps = PDFtoImage.Conversion.ToImagesAsync(contents, null, new RenderOptions(150));

            await foreach (SKBitmap batmap in bitmaps)
            {
                // get jpeg content
                var jpegContent = batmap.Encode(SKEncodedImageFormat.Jpeg, JpegQualityPersent).ToArray();

                currentPageNumber++;

                if (amountBytes < MaxAmountBytes)
                {
                    // save respose pages to memory list
                    amountBytes += jpegContent.Length;
                    jpegPagesList.Add(jpegContent);
                    continue;
                }

                await WritePageToCache(currentPageNumber, jpegContent);
            }

            return new DataPortionDescriptor
            {
                AmountPages = currentPageNumber,
                FileId = FileId,
                JpegPagesList = jpegPagesList
            };
        }


        /// <summary>
        /// Create list of byte arrays with response data
        /// First element is serialized descriptor of data: ResponseDto
        /// </summary>
        /// <param name="convertResult"></param>
        /// <returns></returns>
        public List<byte[]> CreateResultsList(DataPortionDescriptor convertResult)
        {
            var responseDto = new ResponseDto
            {
                AmountPages = convertResult.AmountPages,
                FileId = convertResult.FileId,
                PortionStartPage = _diapazonStartPage,
                PortionFinishPage = convertResult.JpegPagesList.Count-1,
                ErrorMessage = "OK"
            };

            return CreateResultsList(convertResult.JpegPagesList, responseDto);
        }


        /// <summary>
        /// Create list of byte arrays with response data. First element is serialized descriptor of data: ResponseDto
        /// </summary>
        /// <param name="JpegPagesList">List of jpeg</param>
        /// <param name="responseDto">Return data descriptor</param>
        /// <returns>List of byte arrays</returns>
        internal List<byte[]> CreateResultsList(List<byte[]>? JpegPagesList, ResponseDto responseDto)
        {
            var returnList = new List<byte[]> { ClassSerializer.Serialize(responseDto) };

            if (JpegPagesList is not null && JpegPagesList.Count > 0)
                returnList.AddRange(JpegPagesList);

            return returnList;
        }

        public void CreateResponse(List<byte[]> dataList) => SerializedData = ByteArraySerializer.Serialize(dataList);

        /// <summary>
        /// Create response with error message. Used in case of unhandled exception
        /// </summary>
        /// <param name="exception"></param>
        public void CreateErrorResponse(Exception exception)
        {
            try
            {
                var responseDto = new ResponseDto
                {
                    AmountPages = -1,
                    FileId = "Error",
                    PortionStartPage = -1,
                    PortionFinishPage = -1,
                    ErrorMessage = $"An error occurred: {exception.Message}"
                };

                var dataList = CreateResultsList(null, responseDto);
                CreateResponse(dataList);
            }
            catch (Exception ex)
            {
                SerializedData = Encoding.UTF8.GetBytes($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Use only If the file is too large, where the rest of pages are saved to disk
        /// </summary>
        /// <returns>
        /// DataPortionDescriptor:
        ///      AmountPages = -1, (take AmountPages from first call of ConvertPdfFileToJpegPagesList)
        ///      FileId,
        ///      JpegPagesList - next portion list jpeg pages
        /// </returns>
        public async Task<DataPortionDescriptor> GetCachedJpegPages()
        {
            var jpegPagesList = new List<byte[]>();

            // count amount bytes in the list
            int amountBytes = 0;

            for (int pageNumber = _diapazonStartPage; true; pageNumber++)
            {
                byte[]? jpegContent = await ReadPageFromCache(pageNumber);
                if (jpegContent is null)    // no more pages
                    break;

                amountBytes += jpegContent.Length;

                if (amountBytes > MaxAmountBytes && jpegPagesList.Any())    //  max size of response is reached
                    break;

                jpegPagesList.Add(jpegContent);
            }

            return new DataPortionDescriptor
            {
                AmountPages = -1,
                FileId = FileId,
                JpegPagesList = jpegPagesList
            };
        }

        //int MaxA

        int _maxAmountBytes = -1;

        int MaxAmountBytes
        {
            get
            {
                if (_maxAmountBytes <= 0)
                {
                    ReadFromConfigurationMaxAmountBytes();
                }

                return _maxAmountBytes;
            }
        }

        private void ReadFromConfigurationMaxAmountBytes()
        {
            try
            {
                _maxAmountBytes = constMaxAmountBytes;

                int maxAmountBytes = _configuration.GetValue<int>("ServiceJpegSettings:MaxAmountBytes");
                if (maxAmountBytes > 0)
                    _maxAmountBytes = maxAmountBytes;
            }
            catch (Exception)
            {
            }
        }

        const int constMaxAmountBytes= 2_000_000;
    }
}
