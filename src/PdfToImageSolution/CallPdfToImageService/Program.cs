using PdfToImageClient.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;



namespace CallPdfToImageService
{
    internal class Program
    {

        // url of the service
        static string serviceUrl = "http://localhost:32768/pdf";
        //    "http://docker-host2:5257/pdf"

        static void Main(string[] args)
        {
            //TestYield();

            TestMultiThreading();
        }

        private static void TestMultiThreading()
        {
            int threadCount = readThreadCount();

            while (threadCount > 0)
            {
                ProcessInThreads(threadCount);

                threadCount = readThreadCount();
            }
        }

        static void TestYield()
        {
            Console.WriteLine("TestYield Start");

            var fpdFileContents = System.IO.File.ReadAllBytes("1.pdf");

            var (pages, fileId) = ConvertFileContents(fpdFileContents);
            CreateFolder(fileId);

            for (int i = 0; i < pages.Count(); i++)
            {
                // write page to disk
                var fileName = GetFullPageName(fileId, i);
                System.IO.File.WriteAllBytes(fileName, pages[i]);
            }

            Console.WriteLine("TestYield End");
            Console.ReadLine();
        }

        private static (List<byte[]> pages, string fileId) ConvertFileContents(byte[] fpdFileContents)
        {
            using (var clientService = new PdfToImagesClientService())
            {
                var pages = clientService.ConvertToYield(fpdFileContents).ToList();
                return (pages, clientService.ResponseDto.FileId);
            }
        }

        // input: threadCount - number of threads to run
        // start threadCount tasks to convert pdf to images
        private static void ProcessInThreads(int threadCount)
        {
            var fpdFileContents = System.IO.File.ReadAllBytes("Data\\1.pdf");

            var tasks = new List<Task>();

            // start threadCount tasks
            for (int i = 1; i <= threadCount; i++)
            {
                Task task = Task.Run(() => ConvertOnePdfAndSaveImages(fpdFileContents));
                tasks.Add(task);
            }

            // wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("All tasks completed.");
        }


        // Convert one pdf and save images to disk
        private static void ConvertOnePdfAndSaveImages(byte[] fpdFileContents)
        {
            try
            {
                using (var pdfToImagesClientService = new PdfToImagesClientService(serviceUrl))
                {
                    // get first portion of pages
                    var jpegPagesList = pdfToImagesClientService.ConvertFirst(fpdFileContents);

                    CreateFolder(pdfToImagesClientService.ResponseDto.FileId);

                    int pagesProcessed = 0;
                    SavePagesToDisk(jpegPagesList, pdfToImagesClientService.ResponseDto.FileId, ref pagesProcessed);

                    int amountPages = pdfToImagesClientService.ResponseDto.AmountPages;
                    string fileId = pdfToImagesClientService.ResponseDto.FileId;

                    int iterateCount = 0;
                    // get next portions of pages
                    while (pagesProcessed < amountPages)
                    {
                        jpegPagesList = pdfToImagesClientService.ConvertNext(pagesProcessed + 1, fileId);
                        SavePagesToDisk(jpegPagesList, fileId, ref pagesProcessed);
                        iterateCount++;
                    }

                    if (iterateCount > 0)
                    {
                        pdfToImagesClientService.ClearServerCache();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


       

        private static void CreateFolder(string fileId)
        {
            string folder = GetFolderNameByFileId(fileId);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }


        // save pages to disk
        private static void SavePagesToDisk(List<byte[]> jpegPagesList, string fileId, ref int pagesProcessed)
        {
            foreach (byte[] page in jpegPagesList)
            {
                pagesProcessed++;
                System.IO.File.WriteAllBytes(GetFullPageName(fileId, pagesProcessed), page);
            }
        }

        private static string GetFolderNameByFileId(string fileId) => Path.Combine("out", fileId);

        private static string GetFullPageName(string fileId, int pageNumber) => 
            Path.Combine(GetFolderNameByFileId(fileId), GetPageName(pageNumber));
  

        private static string GetPageName(int pageNumber) => $"{pageNumber:D4}.jpeg";


        // This function reads the number of threads from the user input and returns it.
        static int readThreadCount()
        {
            while (true)
            {
                Console.WriteLine("Input threads count >0:");
                string res = Console.ReadLine();

                if (int.TryParse(res, out int threadCount) == false)
                    continue;

                return threadCount;
            }
        }
    }
}
