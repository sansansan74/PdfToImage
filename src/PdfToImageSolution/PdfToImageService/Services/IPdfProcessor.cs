

using PdfToImageService.Dto;

namespace PdfToImageService.Services
{
    public interface IPdfProcessor
    {
        void CreateErrorResponse(Exception ex);
        void CreateResponse(List<byte[]> resultsList);
        Task<DataPortionDescriptor> GetCachedJpegPages();
        List<byte[]> CreateResultsList(DataPortionDescriptor convertResult);
        void Init(string fileId, int diapazonStartPage);
        Task<byte[]?> ReadPageFromCache(int pageNumber);
        Task WritePageToCache(int currentPageNumber, byte[] jpegContent);
        Task<DataPortionDescriptor> ConvertPdfFileToJpegPagesList(byte[] contents);
        byte[]? SerializedData { get; internal set; }
        string FileId { get; set; }
        public int ResponseLen { get; }
        void ClearOldCaches();
        void ClearCache();

    }
}