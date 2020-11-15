using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using TestForVeeamSoftware.DataObjects;

namespace TestForVeeamSoftware.FileProcessing
{
    internal sealed class CompressProcessing : FileProcessing
    {
        private int _chunkSize;
        internal CompressProcessing(IInputParameters inputParameters, int chunkSize = 1048576) : base(inputParameters) 
        {
            _chunkSize = chunkSize;
        }

        public override void Processing()
        {
            _calculationInProgress = true;
            _writer = new Thread(() => WriteToFile());
            _writer.Start();
            using (FileStream _inputFileStream = new FileStream(_inputParameters.SourceFileName, FileMode.Open))
            {
                int dataRead;
                byte[] buffer = new byte[_chunkSize];
                int currentThread = 0;
                int currentBlock = 0;
                while ((dataRead = _inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (_stopProcess)
                    {
                        break;
                    }
                    currentBlock++;
                    while (true)
                    {
                        if (_stopProcess)
                        {
                            break;
                        }
                        if (_threadReady[currentThread])
                        {
                            _threadReady[currentThread] = false;
                            if ((_threads[currentThread] != null) && _threads[currentThread].ThreadState.Equals(ThreadState.Running))
                            {
                                _threads[currentThread].Join();
                            }
                            
                            byte[] copyBuffer = new byte[buffer.Length];
                            buffer.CopyTo(copyBuffer, 0);
                            IThreadBlock cbuf = new ThreadBlock(currentBlock, currentThread, copyBuffer, dataRead);
                            _threads[currentThread] = new Thread(() => BlockProcessing(cbuf));
                            _threads[currentThread].Start();
                            currentThread = (currentThread + 1) % _threadsNumber;
                            break;
                        }
                        else
                        {
                            currentThread = (currentThread + 1) % _threadsNumber;
                        }
                    }
                }
            }
            for (int i = 0; i < _threadsNumber; i++)
            {
                if (_threads[i].ThreadState.Equals(ThreadState.Running))
                {
                    _threads[i].Join();
                }
            }
            _calculationInProgress = false;
            _writer.Join();
        }

        protected override void BlockProcessing(IThreadBlock threadBlock)
        {
            using (MemoryStream _memoryStream = new MemoryStream())
            {
                using (GZipStream cs = new GZipStream(_memoryStream, CompressionMode.Compress))
                {
                    cs.Write(threadBlock.buffer, 0, threadBlock.actualLength);
                }

                byte[] compressedData = _memoryStream.ToArray();
                byte[] compressedDataWithLength = new byte[compressedData.Length + 8];
                byte[] lengthCompressed = BitConverter.GetBytes(compressedData.Length);
                byte[] lengthOriginal = BitConverter.GetBytes(threadBlock.actualLength);
                lengthCompressed.CopyTo(compressedDataWithLength, 0);
                lengthOriginal.CopyTo(compressedDataWithLength, 4);
                compressedData.CopyTo(compressedDataWithLength, 8);
                _mtxBlocks.WaitOne();
                _blocks.Add(threadBlock.blockNumber, compressedDataWithLength);
                _mtxBlocks.ReleaseMutex();
            }
            _threadReady[threadBlock.threadNumber] = true;
        }
    }
}
