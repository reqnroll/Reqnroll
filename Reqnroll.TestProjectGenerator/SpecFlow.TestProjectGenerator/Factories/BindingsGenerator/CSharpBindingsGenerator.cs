using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;
using TechTalk.SpecFlow.TestProjectGenerator.Extensions;
using TechTalk.SpecFlow.TestProjectGenerator.Helpers;

namespace TechTalk.SpecFlow.TestProjectGenerator.Factories.BindingsGenerator
{
    public class CSharpBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = @"
using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

[Binding]
public class {0}
{{
    private readonly ScenarioContext _scenarioContext;

    public {0}(ScenarioContext scenarioContext)
    {{
        _scenarioContext = scenarioContext;
    }}

    {1}
}}";

        public override ProjectFile GenerateBindingClassFile(string content)
        {
            content = AddMissingNamespace(content, "using System;");
            content = AddMissingNamespace(content, "using System.IO;");
            content = AddMissingNamespace(content, "using System.Threading.Tasks;");
            content = AddMissingNamespace(content, "using TechTalk.SpecFlow;");

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
            string fileContent = $@"
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

internal static class Log
{{
    private const string LogFileLocation = @""{pathToLogFile}"";

    private static void Retry(int number, Action action)
    {{
        try
        {{
            action();
        }}
        catch (Exception)
        {{
            var i = number - 1;

            if (i == 0)
                throw;

            Thread.Sleep(500);
            Retry(i, action);
        }}
    }}

    internal static void LogStep([CallerMemberName] string stepName = null)
    {{
        Retry(5, () => WriteToFile($@""-> step: {{stepName}}{{Environment.NewLine}}""));
    }}

    internal static void LogHook([CallerMemberName] string stepName = null)
    {{
        Retry(5, () => WriteToFile($@""-> hook: {{stepName}}{{Environment.NewLine}}""));
    }}

    internal static async Task LogHookIncludingLockingAsync([CallerMemberName] string stepName = null)
    {{
        WriteToFile($@""-> waiting for hook lock: {{stepName}}{{Environment.NewLine}}"");
        await WaitForLockAsync();
        WriteToFile($@""-> hook: {{stepName}}{{Environment.NewLine}}"");
    }}

    static void WriteToFile(string line)
    {{
        using (FileStream fs = File.Open(LogFileLocation, FileMode.Append, FileAccess.Write, FileShare.None))
        {{
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(line);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
        }}
    }}

    static async Task WaitForLockAsync()
    {{
        var lockFile = LogFileLocation + "".lock"";

        while (true)
        {{
            try
            {{
                using (File.Open(lockFile, FileMode.CreateNew))
                {{
                }}

                break;
            }}
            catch (IOException)
            {{
                //wait and retry
                await Task.Delay(1000);
            }}
        }}

        File.Delete(lockFile);
    }}
}}";
            return new ProjectFile("Log.cs", "Compile", fileContent);
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
                        parameter = $"Table {argumentName}";
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
                        parameter = $"Table {argumentName}";
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


            return $@"
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using TechTalk.SpecFlow;

[Binding]
{scopeClassAttributes}
public class {$"HooksClass_{Guid.NewGuid():N}"}
{{
    [{hookType}({hookTypeAttributeTagsString})]
    {scopeMethodAttributes}
    public {staticKeyword} void {name}()
    {{
        {code}
        global::Log.LogHook(); 
    }}   
}}
";
        }

        protected override string GetAsyncHookIncludingLockingBindingClass(
            string hookType,
            string name,
            string code = "",
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


            return $@"
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using TechTalk.SpecFlow;

[Binding]
{scopeClassAttributes}
public class {$"HooksClass_{Guid.NewGuid():N}"}
{{
    [{hookType}({hookTypeAttributeTagsString})]
    {scopeMethodAttributes}
    public {staticKeyword} async Task {name}()
    {{
        {code}
        await global::Log.LogHookIncludingLockingAsync(); 
    }}   
}}
";
        }
    }
}
