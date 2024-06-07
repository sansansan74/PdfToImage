using System.IO.Compression;

namespace CallPdfToImageService.Helpers
{
    /// <summary>
    /// Class for serializing and deserializing list of byte arrays to/from byte array
    /// Uses System.IO.Compression.ZipArchive with CompressionLevel.NoCompression
    /// Jpeg pages can not be compressed
    /// </summary>
    public class ByteArraySerializer
    {
        /// <summary>
        /// Serializes list of byte arrays to byte array using ZIP archive
        /// </summary>
        /// <param name="listOfArrays"></param>
        /// <returns>content of array</returns>
        public static byte[] Serialize(List<byte[]> listOfArrays)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    for (int i = 0; i < listOfArrays.Count; i++)
                    {
                        string fileName = $"{i:D4}.dat";    // Create file name with leading zeros like 0001.dat, 0002.dat, etc.

                        // Create new entry/file in the archive
                        var zipEntry = archive.CreateEntry(fileName, CompressionLevel.NoCompression);

                        // Save data from byte array to the created file in the archive
                        using (var entryStream = zipEntry.Open())
                        {
                            entryStream.Write(listOfArrays[i], 0, listOfArrays[i].Length);
                        }
                    }
                }

                // return byte array with all data from the ZIP archive
                return memoryStream.ToArray();
            }
        }



        /// <summary>
        /// Deserializes byte array to list of byte arrays
        /// </summary>
        /// <param name="byteArray">byte array</param>
        /// <returns>list of byte arrays</returns>
        public static List<byte[]> Deserialize(byte[] byteArray)
        {
            var byteArrayList = new List<byte[]>();

            using var memoryStream = new MemoryStream(byteArray);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries.OrderBy(x => x.Name))
            {
                byteArrayList.Add(DecompressZipEntry(entry));
            }

            return byteArrayList;
        }


        private static byte[] DecompressZipEntry(ZipArchiveEntry entry)
        {
            using var entryStream = entry.Open();
            var fileData = new byte[entry.Length];
            entryStream.Read(fileData, 0, fileData.Length);
            
            return fileData;
        }

    }
}