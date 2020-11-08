using System;
using System.IO;
using TestForVeeamSoftware.ResultOutputManager;

namespace TestForVeeamSoftware.CheckInputParameters
{
    internal sealed class SimpleCheckInputParameters : ICheckInputParameters
    {
        private readonly IResultOutputManager _resultOutput;
        private readonly string[] _args;
        public string[] Args { get { return _args; } }
        internal SimpleCheckInputParameters(IResultOutputManager resultOutput, string[] args)
        {
            _args = args;
            _resultOutput = resultOutput;
        }

        public bool CheckParameters()
        {
            try
            {
                bool result = true;
                if (_args.Length != 3)
                {
                    _resultOutput.AddError(new Exception("Не корректное количество входных параметров."));
                    return false;
                }

                if (!Enum.IsDefined(typeof(Operation), _args[0]))
                {
                    _resultOutput.AddError(new Exception("Не верно указана команда для обработки."));
                    result = false;
                }

                if (!File.Exists(_args[1]))
                {
                    _resultOutput.AddError(new Exception("Файл для обработки не найден."));
                    result = false;
                }
                if (File.Exists(_args[2]))
                {
                    _resultOutput.AddError(new Exception("Указанный как результирующий файл уже существует."));
                    result = false;
                }

                return result;
            }
            catch (Exception ex)
            {
                _resultOutput.AddError(ex);
                return false;
            }
        }
    }
}
