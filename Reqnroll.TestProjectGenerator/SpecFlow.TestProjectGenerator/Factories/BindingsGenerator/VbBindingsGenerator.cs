using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Driver;
using TechTalk.SpecFlow.TestProjectGenerator.Extensions;
using TechTalk.SpecFlow.TestProjectGenerator.Helpers;

namespace TechTalk.SpecFlow.TestProjectGenerator.Factories.BindingsGenerator
{
    public class VbBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = @"
Imports TechTalk.SpecFlow

<Binding> _
Public Class {0}
    {1}
End Class";

        public override ProjectFile GenerateLoggerClass(string pathToLogFile)
        {
            string fileContent = $@"
Imports System
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Threading.Tasks

Friend Module Log
    Private Const LogFileLocation As String = ""{pathToLogFile}""

    Friend Sub LogStep(<CallerMemberName()> Optional stepName As String = Nothing)
        File.AppendAllText(LogFileLocation, $""-> step: {{stepName}}{{Environment.NewLine}}"")
    End Sub

    Friend Sub LogHook(<CallerMemberName()> Optional stepName As String = Nothing)
        File.AppendAllText(LogFileLocation, $""-> hook: {{stepName}}{{Environment.NewLine}}"")
    End Sub
    
    Friend Async Function LogHookIncludingLockingAsync(
    <CallerMemberName> ByVal Optional stepName As String = Nothing) As Task
        File.AppendAllText(LogFileLocation, $""->waiting for hook lock: {{stepName}}{{Environment.NewLine}}"")
        Await WaitForLockAsync()
        File.AppendAllText(LogFileLocation, $""-> hook: {{stepName}}{{Environment.NewLine}}"")
    End Function

    Private Async Function WaitForLockAsync() As Task
        Dim lockFile = LogFileLocation & "".lock""

        While True

            Dim succeeded = True
            Try
                Using File.Open(lockFile, FileMode.CreateNew)
                End Using
                Exit While
            Catch __unusedIOException1__ As IOException
                succeeded = False
            End Try
            If Not succeeded Then
                Await Task.Delay(1000)
            End If
        End While

        File.Delete(lockFile)
    End Function
End Module
";
            return new ProjectFile("Log.vb", "Compile", fileContent);
        }

        public override ProjectFile GenerateBindingClassFile(string content)
        {
            string classNameGuidString = $"{Guid.NewGuid():N}".Substring(24);
            string randomClassName = $"BindingsClass_{classNameGuidString}";
            return new ProjectFile($"BindingsClass_{randomClassName}.vb", "Compile", content);
        }

        public override ProjectFile GenerateStepDefinition(string method)
        {
            string classNameGuidString = $"{Guid.NewGuid():N}".Substring(24);
            string randomClassName = $"BindingsClass_{classNameGuidString}";
            return new ProjectFile($"{randomClassName}.vb", "Compile", string.Format(BindingsClassTemplate, randomClassName, method));
        }

        protected override string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            string parameter = "";

            if (argumentName.IsNotNullOrWhiteSpace())
            {
                switch (parameterType)
                {
                    case ParameterType.Normal:
                        parameter = $"{argumentName} As Object";
                        break;
                    case ParameterType.Table:
                        parameter = $"{argumentName} As Table";
                        break;
                    case ParameterType.DocString:
                        parameter = $"{argumentName} As String";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
                }
            }

            return $@"<[{attributeName}](""{regex}"")> Public Sub {methodName}({parameter}) 
                                
                                    Log.LogStep()
                                    {methodImplementation}
                                End Sub";
        }

        protected override string GetLoggingStepDefinitionCode(string methodName, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            string parameter = "";

            if (argumentName.IsNotNullOrWhiteSpace())
            {
                switch (parameterType)
                {
                    case ParameterType.Normal:
                        parameter = $"{argumentName} As Object";
                        break;
                    case ParameterType.Table:
                        parameter = $"{argumentName} As Table";
                        break;
                    case ParameterType.DocString:
                        parameter = $"{argumentName} As String";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
                }
            }

            string attributeRegex = regex.IsNullOrWhiteSpace() ? string.Empty : $@"""{regex}""";

            return $@"<[{attributeName}]({attributeRegex})> Public Sub {methodName}({parameter}) 
                                
                                    Log.LogStep()
                                End Sub";
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
            string ToScopeTags(IList<string> scopeTags) => scopeTags.Any() ? $"{scopeTags.Select(t => $@"<[Scope](Tag=""{t}"")>").JoinToString("")}_" : null;

            bool isStatic = IsStaticEvent(hookType);
            
            string hookTypeTags = hookTypeAttributeTags?.Select(t => $@"""{t}""").JoinToString(", ");

            var hookAttributeConstructorProperties = new[]
            {
                hookTypeAttributeTags is null || !hookTypeAttributeTags.Any() ? null : $"tags:= New String() {{{hookTypeTags}}}",
                order is null ? null : $"Order:= {order}"
            }.Where(p => p.IsNotNullOrWhiteSpace());

            string hookTypeAttributeTagsString = string.Join(", ", hookAttributeConstructorProperties);
            string classScopeAttributes = ToScopeTags(classScopeAttributeTags);
            string methodScopeAttributes = ToScopeTags(methodScopeAttributeTags);

            string staticKeyword = isStatic ? "Static" : string.Empty;
            return $@"
Imports System
Imports System.Collections
Imports System.IO
Imports System.Linq
Imports System.Xml
Imports System.Xml.Linq
Imports TechTalk.SpecFlow

<[Binding]> _
{classScopeAttributes}
Public Class {Guid.NewGuid()}
    <[{hookType}({hookTypeAttributeTagsString})]>_
    {methodScopeAttributes}
    Public {staticKeyword} Sub {name}()
        {code}
        Console.WriteLine(""-> hook: {name}"")
    End Sub
End Class
";
        }

        protected override string GetAsyncHookIncludingLockingBindingClass(string hookType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null,
            IList<string> classScopeAttributeTags = null)
        {
            string ToScopeTags(IList<string> scopeTags) => scopeTags.Any() ? $"{scopeTags.Select(t => $@"<[Scope](Tag=""{t}"")>").JoinToString("")}_" : null;

            bool isStatic = IsStaticEvent(hookType);

            string hookTypeTags = hookTypeAttributeTags?.Select(t => $@"""{t}""").JoinToString(", ");

            var hookAttributeConstructorProperties = new[]
            {
                hookTypeAttributeTags is null || !hookTypeAttributeTags.Any() ? null : $"tags:= New String() {{{hookTypeTags}}}",
                order is null ? null : $"Order:= {order}"
            }.Where(p => p.IsNotNullOrWhiteSpace());

            string hookTypeAttributeTagsString = string.Join(", ", hookAttributeConstructorProperties);
            string classScopeAttributes = ToScopeTags(classScopeAttributeTags);
            string methodScopeAttributes = ToScopeTags(methodScopeAttributeTags);

            string staticKeyword = isStatic ? "Static" : string.Empty;
            return $@"
Imports System
Imports System.Collections
Imports System.IO
Imports System.Linq
Imports System.Xml
Imports System.Xml.Linq
Imports TechTalk.SpecFlow
Imports System.Threading
Imports System.Threading.Tasks

<[Binding]> _
{classScopeAttributes}
Public Class {Guid.NewGuid()}
    <[{hookType}({hookTypeAttributeTagsString})]>_
    {methodScopeAttributes}
    Public {staticKeyword} Async Function {name}() as Task
        {code}
        Await Log.LogHookIncludingLockingAsync()
    End Function
End Class
";
        }
    }
}
