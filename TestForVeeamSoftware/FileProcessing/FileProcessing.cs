using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TestForVeeamSoftware.DataObjects;

namespace TestForVeeamSoftware.FileProcessing
{
    internal abstract class FileProcessing : IFileProcessing
    {
        protected IInputParameters _inputParameters;
        protected Thread[] _threads;
        protected List<bool> _threadReady;
        protected int _threadsNumber = Environment.ProcessorCount * 30;
        protected Dictionary<int, byte[]> _blocks;
        protected Mutex _mtxBlocks = new Mutex();
        protected bool _calculationInProgress;
        protected Thread _writer;
        protected bool _stopProcess;

        internal FileProcessing(IInputParameters inputParameters)
        {
            _inputParameters = inputParameters;
            _threads = new Thread[_threadsNumber];
            _threadReady = new List<bool>();
            for (int i = 0; i < _threadsNumber; i++)
            {
                _threadReady.Add(true);
            }
            _calculationInProgress = false;
            _blocks = new Dictionary<int, byte[]>();
            _stopProcess = false;
        }
        
        public abstract void Processing();
        public virtual void StopProcess() => _stopProcess = true;

        protected abstract void BlockProcessing(IThreadBlock threadBlock);
        protected virtual void WriteToFile()
        {
            int blockCounter = 1;
            while (_calculationInProgress || (_blocks.Count > 0))
            {
                if (_stopProcess)
                {
                    break;
                }
                _mtxBlocks.WaitOne();
                if (_blocks.ContainsKey(blockCounter))
                {
                    using (FileStream _outputFileStream = new FileStream(_inputParameters.ResultFileName, FileMode.Append))
                    {
                        _outputFileStream.Write(_blocks[blockCounter], 0, _blocks[blockCounter].Length);
                    }
                    _blocks.Remove(blockCounter);
                    _mtxBlocks.ReleaseMutex();
                    blockCounter++;
                }
                else
                {
                    _mtxBlocks.ReleaseMutex();
                    Thread.Sleep(10);
                }
            }
        }
    }
}
