using PdfToImageService.Helpers;

namespace PdfToImageService.Services
{
    /// <summary>
    /// Class for managing cache of jpeg pages
    /// All local caches are stored in the folder CacheFolderName
    /// Every pdf file has its own folder with FileId name in CacheFolderName
    /// Page cache template: CacheFolderName\{FileId}\{pageNumber}.jpeg
    /// For example:    
    ///                 CacheFolderName\FileId1\0003.jpeg
    ///                 CacheFolderName\FileId1\0004.jpeg
    ///                 CacheFolderName\FileId2\0002.jpeg
    ///                 CacheFolderName\FileId2\0001.jpeg
    ///                 
    /// Important:
    /// Cache never contains first pages of pdf files, because they are stored in memory and send to client immediately
    /// </summary>
    public class PageCacheManager
    {
        public string? FileId { get; set; }

        bool _isFolderCreated = false;

        public PageCacheManager()
        {
        }

        private const int DeleteOldCacheSecondsTimeout = 10 * 60;  // 10 minutes

        string _rootCacheFolder;

        /// <summary>
        /// Root-folder for caches for all pdf documents
        /// </summary>
        private string RootCacheFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_rootCacheFolder))
                {
                    _rootCacheFolder = Path.Combine(Path.GetTempPath(), "Pdf2JpegCache");

                    if (!Directory.Exists(_rootCacheFolder))
                        Directory.CreateDirectory(_rootCacheFolder);
                }

                return _rootCacheFolder;
            }
        }

        /// <summary>
        /// Save jpeg page to cache
        /// </summary>
        /// <param name="currentPageNumber">page number</param>
        /// <param name="jpegContent">page content</param>
        /// <returns></returns>
        public async Task WritePageToCache(int currentPageNumber, byte[] jpegContent)
        {
            if (!_isFolderCreated)
            {
                CreateCacheFolder();
                _isFolderCreated = true;
            }

            await System.IO.File.WriteAllBytesAsync(GetPageFileName(currentPageNumber), jpegContent);
        }

        /// <summary>
        /// Read jpeg page from cache
        /// </summary>
        /// <param name="pageNumber">page number</param>
        /// <returns>page content or null, if no such page in cache</returns>
        public async Task<byte[]?> ReadPageFromCache(int pageNumber)
        {
            var pageFileName = GetPageFileName(pageNumber);
            if (!File.Exists(pageFileName))
                return null;

            var jpegContent = await System.IO.File.ReadAllBytesAsync(pageFileName);
            return jpegContent;
        }

        /// <summary>
        /// Remove folder with cached jpeg pages for the file with FileId = this.FileId (current file)
        /// </summary>
        public void ClearCache()
        {
            try
            {
                FileHelper.RemoveDirectoryIfNotExists(GetCacheFolderName());
            }
            catch
            {
            }
        }


        /// <summary>
        /// remove old cache folders, what was created more than DeleteOldCacheSecondsTimeout seconds ago
        /// folders contains cached jpeg pages
        /// </summary>
        public void ClearOldCaches()
        {
            try
            {
                var rootCacheDirectoryInfo = new DirectoryInfo(RootCacheFolder);
                if (!rootCacheDirectoryInfo.Exists)
                    return;

                foreach (DirectoryInfo folder in rootCacheDirectoryInfo.GetDirectories())
                {
                    if (DateTime.Now - folder.CreationTime > TimeSpan.FromSeconds(DeleteOldCacheSecondsTimeout))
                        folder.Delete(true);
                }
            }
            catch
            {
            }
        }
        private void CreateCacheFolder() => FileHelper.CreateDirectoryIfNotExists(GetCacheFolderName());

        private string GetCacheFolderName() => Path.Combine(RootCacheFolder, FileId);

        private string GetPageFileName(int pageNumber) =>
            Path.Combine(RootCacheFolder, FileId, $"{pageNumber:D4}.jpeg");

        internal void Init(string? fileId)
        {
            FileId = string.IsNullOrEmpty(fileId) ? Guid.NewGuid().ToString() : fileId;
        }
    }
}