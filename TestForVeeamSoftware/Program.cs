﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestForVeeamSoftware.CheckInputParameters;
using TestForVeeamSoftware.ResultOutputManager;

namespace TestForVeeamSoftware
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IResultOutputManager resultOutput = new ConsoleResultOutputManager();
                ICheckInputParameters checkInputParameters = new SimpleCheckInputParameters(resultOutput, args);

                if (checkInputParameters.CheckParameters())
                {
                    IInputParameters inputParameters = new InputParameters(args);
                    Compress(inputParameters.SourceFileName, inputParameters.ResultFileName);
                    Decompress(inputParameters.ResultFileName, inputParameters.SourceFileName + "Test");
                }

                resultOutput.RenderResult();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }

#if DEBUG
            Console.ReadKey();
#endif
        }

        public static void Compress(string sourceFile, string compressedFile)
        {
            // поток для чтения исходного файла
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                // поток для записи сжатого файла
                using (FileStream targetStream = File.Create(compressedFile))
                {
                    // поток архивации
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream); // копируем байты из одного потока в другой
                        Console.WriteLine("Сжатие файла {0} завершено. Исходный размер: {1}  сжатый размер: {2}.",
                            sourceFile, sourceStream.Length.ToString(), targetStream.Length.ToString());
                    }
                }
            }
        }

        public static void Decompress(string compressedFile, string targetFile)
        {
            // поток для чтения из сжатого файла
            using (FileStream sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
            {
                // поток для записи восстановленного файла
                using (FileStream targetStream = File.Create(targetFile))
                {
                    // поток разархивации
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                        Console.WriteLine("Восстановлен файл: {0}", targetFile);
                    }
                }
            }
        }
    }
}