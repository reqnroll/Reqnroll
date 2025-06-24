#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reqnroll.TestProjectGenerator
{
    public class ProcessHelper
    { 
        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

        public delegate Task<ProcessResult> ProcessResultSelector(Process process);

        public ProcessResult RunProcess(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, IReadOnlyDictionary<string, string> environmentVariables,
            params object[] arguments)
        {
            var task = RunProcessInternal(outputWriter, workingDirectory, executablePath, argumentsFormat, environmentVariables,
                                          async (process) => Execute(process), arguments);

            return task.Result;
        }

        public async Task<ProcessResult> RunProcessAsync(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, IReadOnlyDictionary<string, string> environmentVariables,
            params object[] arguments)
        {
            return await RunProcessInternal(outputWriter, workingDirectory, executablePath, argumentsFormat, environmentVariables, ExecuteAsync, arguments);
        }

        private async Task<ProcessResult> RunProcessInternal(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, IReadOnlyDictionary<string, string> environmentVariables,
            ProcessResultSelector processResultSelector, params object[] arguments)
        {
            string parameters = string.Format(argumentsFormat, arguments);

            outputWriter.WriteLine("Starting external program: \"{0}\" {1} in {2}", executablePath, parameters, workingDirectory);
            var psi = CreateProcessStartInfo(workingDirectory, executablePath, parameters, environmentVariables);

            using (var process = new Process { StartInfo = psi })
            {
                var result = Execute(process);
                var message = new StringBuilder();
                if (!string.IsNullOrEmpty(result.StdOutput)) message.AppendLine("StdOut:").Append(result.StdOutput).AppendLine();
                if (!string.IsNullOrEmpty(result.StdError)) message.AppendLine("StdError:").Append(result.StdError).AppendLine();
                outputWriter.WriteLine(message.ToString());
                message = new StringBuilder($"Process \"{psi.FileName}\" {psi.Arguments} executed in {result.ExecutionTime} with ExitCode:{result.ExitCode}.");

                if (result.ExecutionTime > Timeout)
                {
                    message.Append($"Took longer than {Timeout}.");
                    throw new TimeoutException(message.ToString());
                }

                outputWriter.WriteLine(message.ToString());
                return result;
            }
        }

        private ProcessResult Execute(Process process)
        {
            int timeOutInMilliseconds = Convert.ToInt32(Timeout.TotalMilliseconds);
            var stdError = new StringBuilder();
            var stdOutput = new StringBuilder();
            var outputWaiter = new CountdownEvent(2);
            process.ErrorDataReceived += (_, e) => AppendReceivedData(stdError, e.Data);
            process.OutputDataReceived += (_, e) => AppendReceivedData(stdOutput, e.Data);
            var sw = Stopwatch.StartNew();

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            bool processResult = process.WaitForExit(timeOutInMilliseconds);

            if (!processResult)
            {
                process.Kill();
            }

            var waitForOutputs = Timeout-sw.Elapsed;
            if (waitForOutputs <= TimeSpan.Zero || waitForOutputs > TimeSpan.FromMinutes(1)) waitForOutputs = TimeSpan.FromMinutes(1);
            outputWaiter.Wait(waitForOutputs);

            sw.Stop();

            return new ProcessResult(process.ExitCode, stdOutput.ToString(), stdError.ToString(), $"{stdOutput}{stdError}", sw.Elapsed);

            void AppendReceivedData(StringBuilder builder, string? data)
            {
                if (data is not null) //null is a sign to the end of the output
                {
                    builder.AppendLine(data);
                }
                else
                {
                    outputWaiter.Signal();
                }
            }
        }

        private async Task<ProcessResult> ExecuteAsync(Process process)
        {
            int timeOutInMilliseconds = Convert.ToInt32(Timeout.TotalMilliseconds);
            var stdError = new StringBuilder();
            var stdOutput = new StringBuilder();
            var outputWaiter = new CountdownEvent(2);
            process.ErrorDataReceived += (_, e) => AppendReceivedData(stdError, e.Data);
            process.OutputDataReceived += (_, e) => AppendReceivedData(stdOutput, e.Data);
            var sw = Stopwatch.StartNew();

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var taskCompletionSource = new TaskCompletionSource<ProcessResult>();
            var cancellationTokenSource = new CancellationTokenSource(timeOutInMilliseconds);

            cancellationTokenSource.Token.Register(() =>
            {
                process.Kill();
                var waitForOutputs = Timeout - sw.Elapsed;
                if (waitForOutputs <= TimeSpan.Zero || waitForOutputs > TimeSpan.FromMinutes(1)) waitForOutputs = TimeSpan.FromMinutes(1);
                outputWaiter.Wait(waitForOutputs);

                sw.Stop();

                taskCompletionSource.TrySetResult(new ProcessResult(process.ExitCode, stdOutput.ToString(), stdError.ToString(), $"{stdOutput}{stdError}", sw.Elapsed));

            }, useSynchronizationContext: false);

            process.Exited += (sender, args) =>
            {
                var waitForOutputs = Timeout - sw.Elapsed;
                if (waitForOutputs <= TimeSpan.Zero || waitForOutputs > TimeSpan.FromMinutes(1)) waitForOutputs = TimeSpan.FromMinutes(1);
                outputWaiter.Wait(waitForOutputs);

                sw.Stop();

                taskCompletionSource.TrySetResult(new ProcessResult(process.ExitCode, stdOutput.ToString(), stdError.ToString(), $"{stdOutput}{stdError}", sw.Elapsed));
            };

            return await taskCompletionSource.Task;

            void AppendReceivedData(StringBuilder builder, string? data)
            {
                if (data is not null) //null is a sign to the end of the output
                {
                    builder.AppendLine(data);
                }
                else
                {
                    outputWaiter.Signal();
                }
            }
        }

        public ProcessResult RunProcess(IOutputWriter outputWriter, string workingDirectory, string executablePath, string argumentsFormat, params object[] arguments)
        {
            return RunProcess(outputWriter, workingDirectory, executablePath, argumentsFormat, new Dictionary<string, string>(), arguments);
        }

        private ProcessStartInfo CreateProcessStartInfo(string workingDirectory, string executablePath, string parameters, IReadOnlyDictionary<string, string> environmentVariables)
        {
            var processStartInfo = new ProcessStartInfo(executablePath, parameters)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory
            };

            foreach (var env in environmentVariables)
            {
                processStartInfo.Environment.Add(env.Key, env.Value);
            }

            return processStartInfo;
        }
    }
}
