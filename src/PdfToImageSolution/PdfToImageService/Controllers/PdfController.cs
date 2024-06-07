using Microsoft.AspNetCore.Mvc;
using PdfToImageService.Services;

namespace PdfToImageService.Controllers
{
    /// <summary>
    /// REST API for converting PDF files to images
    /// Algorithm of work with the service:
    /// 1. Upload a PDF file to the service using the POST. 
    ///     The service converts the PDF file to a list of images and returns the first N images, 
    ///             with amount size <= MAX_RESPONSE.
    ///     If the PDF file is too large (> MAX_RESPONSE), the method saves the rest of the images to disk.
    ///     
    /// 2. Download the remaining images using the GET.
    /// 3. Clear the cache using the DELETE.
    /// </summary>

    [ApiController]
    [Route("[controller]")]
    public partial class PdfController : ControllerBase
    {
        readonly ILogger<PdfController> _logger;
        readonly IPdfProcessor _pdfProcessor;

        public PdfController(ILogger<PdfController> logger, IPdfProcessor pdfProcessor)
        {
            _logger = logger;
            _pdfProcessor = pdfProcessor;
        }


        [HttpGet(Name = "DownloadPdfTail")]
        [HttpGet("DownloadPdfTail/{fileId}/{diapazonStartPage}")]
        public async Task<IActionResult> DownloadPdfTail(string fileId, int diapazonStartPage)
        {
            _pdfProcessor.Init(fileId, diapazonStartPage);
            //var pdfProcessor = new PdfProcessor(fileId, diapazonStartPage);

            try
            {
                _logger.LogInformation($"Start DownloadPdfTail FileId={fileId}");

                var convertResult = await _pdfProcessor.GetCachedJpegPages();
                List<byte[]> resultsList = _pdfProcessor.CreateResultsList(convertResult);
                _pdfProcessor.CreateResponse(resultsList);

                _logger.LogInformation($"Finish process DownloadPdfTail. FileId={fileId}, amountPages={convertResult.AmountPages}, Pages 1 portion={resultsList.Count-1}, Output len={_pdfProcessor.ResponseLen}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _pdfProcessor.CreateErrorResponse(ex);
            }

            return File(_pdfProcessor.SerializedData, "application/octet-stream", "PdfImages.bin");
        }


        [HttpPost(Name = "UploadPdf")]
        public async Task<IActionResult> UploadPdf()
        {
            _pdfProcessor.Init(null, 1);

            // we clear caches not every time, but only once at 60 calls
            // because probability of call at the start of minute is 1/60
            if (DateTime.Now.Second == 0)
                _pdfProcessor.ClearOldCaches();

            try
            {
                _logger.LogInformation($"Start UploadPdf FileId={_pdfProcessor.FileId}");

                var contents = await GetUploadedFileContents();

                var convertResult = await _pdfProcessor.ConvertPdfFileToJpegPagesList(contents);
                List<byte[]> resultsList = _pdfProcessor.CreateResultsList(convertResult);
                _pdfProcessor.CreateResponse(resultsList);

                _logger.LogInformation($"Finish process UploadPdf. FileId={_pdfProcessor.FileId}, Pages in portion={resultsList.Count-1}. Input len={contents?.Length}, output len={_pdfProcessor.ResponseLen}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _pdfProcessor.CreateErrorResponse(ex);
            }

            return File(_pdfProcessor.SerializedData, "application/octet-stream", "PdfImages.bin");
        }

        [HttpDelete("ClearCache/{fileId}")]
        public IActionResult ClearCache(string fileId)
        {
            _pdfProcessor.Init(fileId, -1);

            try
            {
                _logger.LogInformation($"Start ClearCache FileId={_pdfProcessor.FileId}");

                _pdfProcessor.ClearCache();

                _logger.LogInformation($"Finish ClearCache FileId={_pdfProcessor.FileId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }

            return Ok();
        }


        async Task<byte[]> GetUploadedFileContents()
        {
            if (Request.Form.Files.Count != 1)
                throw new Exception($"Only 1 file must be uploaded. Files count = {Request.Form.Files.Count}");

            IFormFile File = Request.Form.Files[0];
            if (File.Length <= 0)
                throw new Exception("File length = 0");

            using var pdfStream = new MemoryStream();
            await File.CopyToAsync(pdfStream);
            var contents = pdfStream.ToArray();

            return contents;
        }

    }
}