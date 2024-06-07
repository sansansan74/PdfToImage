using Microsoft.AspNetCore.Mvc;

namespace PdfToImageService.Controllers
{
    /// <summary>
    /// REST API for checking the service
    /// </summary>

    [ApiController]
    [Route("[controller]")]
    public partial class PingController : ControllerBase
    {
        ILogger<PdfController> _logger;

        public PingController(ILogger<PdfController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetPdf")]
        public IActionResult GetPdf() => Content("<html>The service is work!</html>", "text/html");


    }
}