namespace PdfToImageService.Dto
{
    public class DataPortionDescriptor
    {
        internal List<byte[]> JpegPagesList { get; set; }
        internal string? FileId { get; set; }
        internal int AmountPages { get; set; }
    }
}