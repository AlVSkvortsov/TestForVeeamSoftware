using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using TestForVeeamSoftware.DataObjects;

namespace TestForVeeamSoftware.FileProcessing
{
    internal sealed class DecompressProcessing : FileProcessing
    {
        internal DecompressProcessing(IInputParameters inputParameters) : base(inputParameters) { }

        public override void Processing()
        {
            _calculationInProgress = true;
            _writer = new Thread(() => WriteToFile());
            _writer.Start();
            using (FileStream _inputFileStream = new FileStream(_inputParameters.SourceFileName, FileMode.Open))
            {
                int currentThread = 0;
                int currentBlock = 0;
                while (_inputFileStream.Position < _inputFileStream.Length)
                {
                    if (_stopProcess)
                    {
                        break;
                    }
                    currentBlock++;
                    byte[] lengthBytes = new byte[4];
                    _inputFileStream.Read(lengthBytes, 0, 4);
                    var lengthCompressed = BitConverter.ToInt32(lengthBytes, 0);
                    byte[] _compressedBuffer = new byte[lengthCompressed];
                    _inputFileStream.Read(lengthBytes, 0, 4);
                    var lengthOriginal = BitConverter.ToInt32(lengthBytes, 0);
                    _inputFileStream.Read(_compressedBuffer, 0, lengthCompressed);
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
                            
                            byte[] copyBuffer = new byte[_compressedBuffer.Length];
                            _compressedBuffer.CopyTo(copyBuffer, 0);
                            IThreadBlock cbuf = new ThreadBlock(currentBlock, currentThread, copyBuffer, lengthOriginal);
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
                if ((_threads[i] != null) && _threads[i].ThreadState.Equals(ThreadState.Running))
                {
                    _threads[i].Join();
                }
            }
            _calculationInProgress = false;
            _writer.Join();
        }

        protected override void BlockProcessing(IThreadBlock threadBlock)
        {
            using (GZipStream gzipStream = new GZipStream(new MemoryStream(threadBlock.buffer), CompressionMode.Decompress))
            {
                var result = new byte[threadBlock.actualLength];
                gzipStream.Read(result, 0, threadBlock.actualLength);
                _mtxBlocks.WaitOne();
                _blocks.Add(threadBlock.blockNumber, result);
                _mtxBlocks.ReleaseMutex();
            }
            _threadReady[threadBlock.threadNumber] = true;
        }
    }
}
