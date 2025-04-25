using System;
using System.Collections.Generic;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.TestProjectGenerator.Factories.BindingsGenerator
{
    public class FSharpBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = @"
namespace Bindings
open Reqnroll

[<Binding>]
type {0}() =
    {1}";

        public override ProjectFile GenerateBindingClassFile(string content)
        {
            string classNameGuidString = $"{Guid.NewGuid():N}".Substring(24);
            string randomClassName = $"BindingsClass_{classNameGuidString}";
            return new ProjectFile($"{randomClassName}.fs", "Compile", content);
        }

        public override ProjectFile GenerateStepDefinition(string method)
        {
            string classNameGuidString = $"{Guid.NewGuid():N}".Substring(24);
            string randomClassName = $"BindingsClass_{classNameGuidString}";
            return new ProjectFile($"{randomClassName}.fs", "Compile", string.Format(BindingsClassTemplate, randomClassName, method));
        }
        public override ProjectFile GenerateLoggerClass(string pathToLogFile)
        {
            string fileContent = $@"
namespace LocalApp

open System
open System.IO
open System.Runtime.CompilerServices

type internal Log =
    static member LogFileLocation = @""{pathToLogFile}"";

    static member LogStep ([<CallerMemberName>] ?stepName : string) =
        File.AppendAllText(Log.LogFileLocation, sprintf @""-> step: %s%s"" stepName.Value Environment.NewLine)

    static member LogHook([<CallerMemberName>] ?stepName : string) =
        File.AppendAllText(Log.LogFileLocation, sprintf @""-> hook: %s%s"" stepName.Value Environment.NewLine)
";
            return new ProjectFile("Log.fs", "Compile", fileContent);
        }

        protected override string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            string parameter = "";

            if (argumentName.IsNotNullOrWhiteSpace())
            {
                switch (parameterType)
                {
                    case ParameterType.Normal:
                        parameter = $"{argumentName} : Object";
                        break;
                    case ParameterType.Table:
                        parameter = $"{argumentName} : DataTable";
                        break;
                    case ParameterType.DocString:
                        parameter = $"{argumentName} : string";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
                }
            }

            return $@"
[<{attributeName}(@""{regex}"")>] let {methodName}({parameter}) : unit =
    {methodImplementation}
    LocalApp.Log.LogStep @""{methodName}""
";
        }

        protected override string GetLoggingStepDefinitionCode(string methodName, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            string parameter = "";

            if (argumentName.IsNotNullOrWhiteSpace())
            {
                switch (parameterType)
                {
                    case ParameterType.Normal:
                        parameter = $"{argumentName} : Object";
                        break;
                    case ParameterType.Table:
                        parameter = $"{argumentName} : DataTable";
                        break;
                    case ParameterType.DocString:
                        parameter = $"{argumentName} : string";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
                }
            }

            return $@"
[<{attributeName}(@""{regex}"")>] let {methodName}({parameter}) : unit =
    LocalApp.Log.LogStep @""{methodName}""
";
        }

        protected override string GetHookBindingClass(
            string hookType,
            string name,
            string code = "",
            bool? asyncHook = null,
            int? order = null,
            IList<string> hookTypeAttributeTags = null,
            IList<string> methodScopeAttributeTags = null,
            IList<string> classScopeAttributeTags = null)
        {
            throw new NotImplementedException();
        }
    }
}
