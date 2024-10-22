using System;
using System.Collections.Generic;
using System.Threading;

namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public class CommandBuilder
    {
        protected readonly IOutputWriter _outputWriter;

        public CommandBuilder(IOutputWriter outputWriter, string executablePath, string argumentsFormat, string workingDirectory)
        {
            _outputWriter = outputWriter;
            WorkingDirectory = workingDirectory;
            ExecutablePath = executablePath;
            ArgumentsFormat = argumentsFormat;
        }

        public string ArgumentsFormat { get; }
        public string ExecutablePath { get; }
        public string WorkingDirectory { get; }

        public CommandResult ExecuteWithRetry(int times, TimeSpan interval, Func<Exception, Exception> exceptionFunction)
        {
            var exceptions = new List<Exception>();

            while (times-- >= 0)
            {
                try
                {
                    return Execute(exceptionFunction);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
                Thread.Sleep(interval);
            }

            throw exceptionFunction(new AggregateException(exceptions));
        }

        public CommandResult Execute()
        {
            return Execute(innerException => new Exception($"Error while executing {ExecutablePath} {ArgumentsFormat}", innerException));
        }

        public virtual CommandResult Execute(Func<Exception, Exception> exceptionFunction) 
        {
            var solutionCreateProcessHelper = new ProcessHelper();

            var processResult = solutionCreateProcessHelper.RunProcess(_outputWriter, WorkingDirectory, ExecutablePath, ArgumentsFormat);
            if (processResult.ExitCode != 0)
            {
                var innerException = new Exception(processResult.CombinedOutput);

                throw exceptionFunction(innerException);
            }

            return new CommandResult(processResult.ExitCode, processResult.CombinedOutput);
        }
    }
}
