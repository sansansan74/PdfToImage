namespace PdfToImageService.Dto
{
    [Serializable]
    public class ResponseDto
    {
        public string FileId { get; set; }
        public int AmountPages { get; set; }
        public int PortionStartPage { get; set; }
        public int PortionFinishPage { get; set; }
        public string ErrorMessage { get; set;}
    }
}
