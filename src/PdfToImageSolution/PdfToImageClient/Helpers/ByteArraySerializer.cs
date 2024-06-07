using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PdfToImageClient.Helpers
{
    public class ByteArraySerializer
    {
        public static byte[] Serialize(List<byte[]> listOfArrays)
        {
            // Используем MemoryStream для создания архива в памяти
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // Проходим по списку массивов байтов
                    for (int i = 0; i < listOfArrays.Count; i++)
                    {
                        // Формируем имя файла внутри архива с лидирующими нулями
                        string fileName = $"{i:D4}.dat";

                        // Создаём новую запись в архиве
                        var zipEntry = archive.CreateEntry(fileName, CompressionLevel.NoCompression);

                        // Записываем данные массива байтов в созданный файл в архиве
                        using (var entryStream = zipEntry.Open())
                        {
                            entryStream.Write(listOfArrays[i], 0, listOfArrays[i].Length);
                        }
                    }
                }

                // Возвращаем массив байтов, который содержит все данные ZIP-архива
                return memoryStream.ToArray();
            }
        }



        // Сериализует список массивов байтов в один массив байтов
        public static byte[] Serialize1(List<byte[]> listOfArrays)
        {
            // Используем MemoryStream для создания архива в памяти
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // Проходим по списку массивов байтов
                    for (int i = 0; i < listOfArrays.Count; i++)
                    {
                        // Формируем имя файла внутри архива с лидирующими нулями
                        string fileName = $"{i:D4}.dat";

                        // Создаём новую запись в архиве
                        var zipEntry = archive.CreateEntry(fileName, CompressionLevel.NoCompression);

                        // Записываем данные массива байтов в созданный файл в архиве
                        using (var entryStream = zipEntry.Open())
                        {
                            entryStream.Write(listOfArrays[i], 0, listOfArrays[i].Length);
                        }
                    }
                }

                // Возвращаем массив байтов, который содержит все данные ZIP-архива
                return memoryStream.ToArray();
            }
        }

        // Десериализует массив байтов в список массивов байтов
        public static List<byte[]> Deserialize(byte[] byteArray)
        {
            var byteArrayList = new List<byte[]>();

            using (var memoryStream = new MemoryStream(byteArray))
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    // Перебор всех файлов в архиве
                    foreach (var entry in archive.Entries.OrderBy(x => x.Name))
                    {
                        AddByteArrayItem(byteArrayList, entry);
                    }
                }
            }

            return byteArrayList;
        }

        private static void AddByteArrayItem(List<byte[]> byteArrayList, ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            {
                // Чтение содержимого файла в массив байт
                var fileData = new byte[entry.Length];
                entryStream.Read(fileData, 0, fileData.Length);
                byteArrayList.Add(fileData);
            }
        }

    }
}