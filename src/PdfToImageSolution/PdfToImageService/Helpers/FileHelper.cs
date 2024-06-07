namespace PdfToImageService.Helpers
{
    internal static class FileHelper
    {

        internal static async Task<byte[]> GetFileContents(IFormFile formFile)
        {
            using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }


        internal static void CreateDirectoryIfNotExists(string folderName)
        {
            if (Directory.Exists(folderName) == false)
                Directory.CreateDirectory(folderName);
        }

        internal static void RemoveDirectoryIfNotExists(string folderName)
        {
            if (Directory.Exists(folderName))
                Directory.Delete(folderName, true);
        }

    }
}