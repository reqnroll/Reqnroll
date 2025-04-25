using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator.Extensions;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.TestProjectGenerator.Factories.BindingsGenerator
{
    public class VbBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = 
            """
            Imports Reqnroll

            <Binding> _
            Public Class {0}
                {1}
            End Class
            """;

        public override ProjectFile GenerateLoggerClass(string pathToLogFile)
        {
            string fileContent = 
                $$"""
                Imports System
                Imports System.IO
                Imports System.Runtime.CompilerServices
                Imports System.Threading.Tasks

                Friend Module Log
                    Private Const RetryCount As Integer = 5
                    Private Const LogFileLocation As String = "{{pathToLogFile}}"
                    Private ReadOnly Rnd As Random = New Random()
                    Private ReadOnly LockObj As Object = New Object()
                   
                    Private Sub Retry(number As Integer, action As Action)
                        Try
                            action
                        Catch ex As Exception
                            Dim i = number - 1
                            If (i = 0)
                                Throw
                            End If
                            System.Threading.Thread.Sleep(50 + Rnd.Next(50))
                            Retry(i, action)
                        End Try
                    End Sub    

                    Friend Sub LogStep(<CallerMemberName()> Optional stepName As String = Nothing)
                        Retry(RetryCount, sub() WriteToFile($"-> step: {stepName}{Environment.NewLine}"))
                    End Sub

                    Friend Sub LogHook(<CallerMemberName()> Optional stepName As String = Nothing)
                        Retry(RetryCount, sub() WriteToFile($"-> hook: {stepName}{Environment.NewLine}"))
                    End Sub

                    Friend Sub LogCustom(category As String, value As String, <CallerMemberName()> Optional memberName As String = Nothing)
                        Retry(RetryCount, sub() WriteToFile($"-> {category}: {value}:{memberName}{Environment.NewLine}"))
                    End Sub
                   
                    Private Sub WriteToFile(line As String)
                        SyncLock LockObj
                            File.AppendAllText(LogFileLocation, line)
                        End SyncLock
                    End Sub
                End Module
                """;
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
                        parameter = $"{argumentName} As DataTable";
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
                        parameter = $"{argumentName} As DataTable";
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
            bool? asyncHook = null,
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
            return $"""
                Imports System
                Imports System.Collections
                Imports System.IO
                Imports System.Linq
                Imports System.Xml
                Imports System.Xml.Linq
                Imports Reqnroll

                <[Binding]> _
                {classScopeAttributes}
                Public Class {Guid.NewGuid()}
                    <[{hookType}({hookTypeAttributeTagsString})]>_
                    {methodScopeAttributes}
                    Public {staticKeyword} Sub {name}()
                        {code}
                        Console.WriteLine("-> hook: {name}")
                    End Sub
                End Class
                """;
        }

    }
}
