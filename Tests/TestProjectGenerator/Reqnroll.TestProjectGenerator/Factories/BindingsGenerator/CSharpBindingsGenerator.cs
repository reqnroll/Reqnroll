using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator.Extensions;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.TestProjectGenerator.Factories.BindingsGenerator
{
    public class CSharpBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = """

                                                     using System;
                                                     using System.IO;
                                                     using System.Xml;
                                                     using System.Linq;
                                                     using System.Threading.Tasks;
                                                     using Reqnroll;

                                                     [Binding]
                                                     public class {0}
                                                     {{
                                                         private readonly ScenarioContext _scenarioContext;
                                                     
                                                         public {0}(ScenarioContext scenarioContext)
                                                         {{
                                                             _scenarioContext = scenarioContext;
                                                         }}
                                                     
                                                         {1}
                                                     }}
                                                     """;

        public override ProjectFile GenerateBindingClassFile(string content)
        {
            content = AddMissingNamespace(content, "using System;");
            content = AddMissingNamespace(content, "using System.IO;");
            content = AddMissingNamespace(content, "using System.Threading.Tasks;");
            content = AddMissingNamespace(content, "using Reqnroll;");

            string classNameGuidString = $"{Guid.NewGuid():N}".Substring(24);
            string randomClassName = $"BindingsClass_{classNameGuidString}";
            return new ProjectFile($"{randomClassName}.cs", "Compile", content);
        }

        private string AddMissingNamespace(string content, string @namespace)
        {
            if (!content.Contains(@namespace))
            {
                content = @namespace + Environment.NewLine + content;
            }

            return content;
        }

        public override ProjectFile GenerateStepDefinition(string method)
        {
            string classNameGuidString = $"{Guid.NewGuid():N}".Substring(24);
            string randomClassName = $"BindingsClass_{classNameGuidString}";
            string fileContent = string.Format(BindingsClassTemplate, randomClassName, method);
            return new ProjectFile($"{randomClassName}.cs", "Compile", fileContent);
        }

        public override ProjectFile GenerateLoggerClass(string pathToLogFile)
        {
            string fileContent = GetLogFileContent(pathToLogFile);
            return new ProjectFile("Log.cs", "Compile", fileContent);
        }

        protected virtual string GetLogFileContent(string pathToLogFile)
        {
            string fileContent = $$"""
                using System;
                using System.IO;
                using System.Runtime.CompilerServices;
                using System.Threading;
                using System.Threading.Tasks;

                internal static class Log
                {
                    private const int RetryCount = 5;
                    private const string LogFileLocation = @"{{pathToLogFile}}";
                    private static readonly Random Rnd = new Random();
                    private static readonly object LockObj = new object();

                    private static void Retry(int number, Action action)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception)
                        {
                            var i = number - 1;

                            if (i == 0)
                                throw;

                            Thread.Sleep(50 + Rnd.Next(50));
                            Retry(i, action);
                        }
                    }

                    internal static void LogStep([CallerMemberName] string stepName = null)
                    {
                        Retry(RetryCount, () => WriteToFile($@"-> step: {stepName}{Environment.NewLine}"));
                    }

                    internal static void LogHook([CallerMemberName] string stepName = null)
                    {
                        Retry(RetryCount, () => WriteToFile($@"-> hook: {stepName}{Environment.NewLine}"));
                    }

                    internal static void LogCustom(string category, string value, [CallerMemberName] string memberName = null)
                    {
                        Retry(RetryCount, () => WriteToFile($@"-> {category}: {value}:{memberName}{Environment.NewLine}"));
                    }
                   
                    static void WriteToFile(string line)
                    {
                        lock(LockObj)
                            File.AppendAllText(LogFileLocation, line);
                    }
                }
                """;
            return fileContent;
        }

        protected override string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            string parameter = "";

            if (argumentName.IsNotNullOrWhiteSpace())
            {
                switch (parameterType)
                {
                    case ParameterType.Normal:
                        parameter = $"object {argumentName}";
                        break;
                    case ParameterType.Table:
                        parameter = $"DataTable {argumentName}";
                        break;
                    case ParameterType.DocString:
                        parameter = $"string {argumentName}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
                }
            }

            return $@"[{attributeName}(@""{regex}"")] public void {methodName}({parameter}) 
                                {{
                                    global::Log.LogStep();
                                    {methodImplementation}
                                }}";
        }

        protected override string GetLoggingStepDefinitionCode(string methodName, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            string parameter = "";

            if (argumentName.IsNotNullOrWhiteSpace())
            {
                switch (parameterType)
                {
                    case ParameterType.Normal:
                        parameter = $"object {argumentName}";
                        break;
                    case ParameterType.Table:
                        parameter = $"DataTable {argumentName}";
                        break;
                    case ParameterType.DocString:
                        parameter = $"string {argumentName}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
                }
            }

            string attributeRegex = regex.IsNullOrWhiteSpace() ? string.Empty : $@"@""{regex}""";

            return $@"[{attributeName}({attributeRegex})] public void {methodName}({parameter}) 
                                {{
                                    global::Log.LogStep();
                                }}";
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
            string ToScopeTags(IList<string> scopeTags) => scopeTags is null || !scopeTags.Any() ? null : $"[{string.Join(", ", scopeTags.Select(t => $@"Scope(Tag=""{t}"")"))}]";

            bool isStatic = IsStaticEvent(hookType);

            string hookTags = hookTypeAttributeTags?.Select(t => $@"""{t}""").JoinToString(", ");

            var hookAttributeConstructorProperties = new[]
            {
                hookTypeAttributeTags is null || !hookTypeAttributeTags.Any() ? null : $"tags: new string[] {{{hookTags}}}",
                order is null ? null : $"Order = {order}"
            }.Where(p => p.IsNotNullOrWhiteSpace());

            string hookTypeAttributeTagsString = string.Join(", ", hookAttributeConstructorProperties);

            string scopeClassAttributes = ToScopeTags(classScopeAttributeTags);
            string scopeMethodAttributes = ToScopeTags(methodScopeAttributeTags);
            string staticKeyword = isStatic ? "static" : string.Empty;
            
            var asyncHookValue = asyncHook ?? DefaultAsyncHook;
            var returnType = asyncHookValue ? "async Task" : "void";

            return $$"""

                     using System;
                     using System.Collections;
                     using System.IO;
                     using System.Linq;
                     using System.Xml;
                     using System.Xml.Linq;
                     using Reqnroll;

                     [Binding]
                     {{scopeClassAttributes}}
                     public class {{$"HooksClass_{Guid.NewGuid():N}"}}
                     {
                         [{{hookType}}({{hookTypeAttributeTagsString}})]
                         {{scopeMethodAttributes}}
                         public {{staticKeyword}} {{returnType}} {{name}}()
                         {
                             {{code}}
                             global::Log.LogHook();
                         }
                     }

                     """;
        }
    }
}
