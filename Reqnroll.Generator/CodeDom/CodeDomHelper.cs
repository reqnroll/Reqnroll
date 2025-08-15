using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Reqnroll.Generator.Generation;

namespace Reqnroll.Generator.CodeDom
{
    public class CodeDomHelper
    {
        public CodeDomProviderLanguage TargetLanguage { get; private set; }

        public CodeDomHelper(CodeDomProvider codeComProvider)
        {
            switch (codeComProvider.FileExtension.ToLower(CultureInfo.InvariantCulture))
            {
                case "cs":
                    TargetLanguage = CodeDomProviderLanguage.CSharp;
                    break;
                case "vb":
                    TargetLanguage = CodeDomProviderLanguage.VB;
                    break;
                default:
                    TargetLanguage = CodeDomProviderLanguage.Other;
                    break;
            }
        }

        public CodeDomHelper(CodeDomProviderLanguage targetLanguage)
        {
            TargetLanguage = targetLanguage;
        }

        public CodeTypeReference CreateNestedTypeReference(CodeTypeDeclaration baseTypeDeclaration, string nestedTypeName)
        {
            return new CodeTypeReference(baseTypeDeclaration.Name + "." + nestedTypeName);
        }

        private CodeStatement CreateCommentStatement(string comment)
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.CSharp:
                    return new CodeSnippetStatement("//" + comment);
                case CodeDomProviderLanguage.VB:
                    return new CodeSnippetStatement("'" + comment);
            }

            throw TargetLanguageNotSupportedException();
        }

        private NotImplementedException TargetLanguageNotSupportedException()
        {
            return new NotImplementedException($"{TargetLanguage} is not supported");
        }

        public void BindTypeToSourceFile(CodeTypeDeclaration typeDeclaration, string fileName)
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.VB:
                    typeDeclaration.Members.Add(new CodeSnippetTypeMember(string.Format("#ExternalSource(\"{0}\",1)", fileName)));
                    typeDeclaration.Members.Add(new CodeSnippetTypeMember("#End ExternalSource"));
                    break;

                case CodeDomProviderLanguage.CSharp:
                    typeDeclaration.Members.Add(new CodeSnippetTypeMember(string.Format("#line 1 \"{0}\"", fileName)));
                    typeDeclaration.Members.Add(new CodeSnippetTypeMember("#line hidden"));
                    break;
            }
        }

        public CodeStatement GetStartRegionStatement(string regionText)
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.CSharp:
                    return new CodeSnippetStatement("#region " + regionText);
                case CodeDomProviderLanguage.VB:
                    return new CodeSnippetStatement("#Region \"" + regionText + "\"");
            }
            return new CodeCommentStatement("#region " + regionText);
        }

        public CodeStatement GetEndRegionStatement()
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.CSharp:
                    return new CodeSnippetStatement("#endregion");
                case CodeDomProviderLanguage.VB:
                    return new CodeSnippetStatement("#End Region");
            }
            return new CodeCommentStatement("#endregion");
        }

        public CodeStatement GetDisableWarningsPragma()
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.CSharp:
                    return new CodeSnippetStatement("#pragma warning disable");
                case CodeDomProviderLanguage.VB:
                    return new CodeSnippetStatement("#Disable Warning BC42356"); //in VB warning codes must be listed explicitly
            }
            return new CodeCommentStatement("#pragma warning disable");
        }

        public CodeStatement GetEnableWarningsPragma()
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.CSharp:
                    return new CodeSnippetStatement("#pragma warning restore");
                case CodeDomProviderLanguage.VB:
                    return new CodeSnippetStatement("#Enable Warning BC42356"); //in VB warning codes must be listed explicitly
            }
            return new CodeCommentStatement("#pragma warning restore");
        }

        private Version GetCurrentReqnrollVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public CodeTypeDeclaration CreateGeneratedTypeDeclaration(string className)
        {
            var result = new CodeTypeDeclaration(className);
            result.CustomAttributes.Add(
                new CodeAttributeDeclaration(
                    new CodeTypeReference(typeof(GeneratedCodeAttribute), CodeTypeReferenceOptions.GlobalReference),
                    new CodeAttributeArgument(new CodePrimitiveExpression("Reqnroll")),
                    new CodeAttributeArgument(new CodePrimitiveExpression(GetCurrentReqnrollVersion().ToString()))));
            result.CustomAttributes.Add(
                new CodeAttributeDeclaration(
                    new CodeTypeReference(typeof(CompilerGeneratedAttribute), CodeTypeReferenceOptions.GlobalReference)));

            return result;
        }

        public string GetErrorStatementString(string msg)
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.CSharp:
                    return "#error " + msg;
                default:
                    return msg;
            }
        }

        // FIX #633 by forcing all attributes to use the global:: (GLOBAL. for VB) prefix
        public CodeAttributeDeclaration AddAttribute(CodeTypeMember codeTypeMember, string attrType)
        {
            var attributeTypeReference = new CodeTypeReference(attrType, CodeTypeReferenceOptions.GlobalReference);
            var codeAttributeDeclaration = new CodeAttributeDeclaration(attributeTypeReference);
            codeTypeMember.CustomAttributes.Add(codeAttributeDeclaration);
            return codeAttributeDeclaration;
        }

        public CodeAttributeDeclaration AddAttribute(CodeTypeMember codeTypeMember, string attrType, params object[] attrValues)
        {
            var attributeTypeReference = new CodeTypeReference(attrType, CodeTypeReferenceOptions.GlobalReference);

            var codeAttributeDeclaration = new CodeAttributeDeclaration(attributeTypeReference,
                attrValues.Select(attrValue => new CodeAttributeArgument(new CodePrimitiveExpression(attrValue))).ToArray());
            codeTypeMember.CustomAttributes.Add(codeAttributeDeclaration);
            return codeAttributeDeclaration;
        }

        public CodeAttributeDeclaration AddAttribute(CodeTypeMember codeTypeMember, string attrType, params CodeAttributeArgument[] attrArguments)
        {
            var attributeTypeReference = new CodeTypeReference(attrType, CodeTypeReferenceOptions.GlobalReference);

            var codeAttributeDeclaration = new CodeAttributeDeclaration(attributeTypeReference, attrArguments);
            codeTypeMember.CustomAttributes.Add(codeAttributeDeclaration);
            return codeAttributeDeclaration;
        }

        public void AddAttributeForEachValue<TValue>(CodeTypeMember codeTypeMember, string attrType, IEnumerable<TValue> attrValues)
        {
            foreach (var attrValue in attrValues)
                AddAttribute(codeTypeMember, attrType, attrValue);
        }

        public CodeDomProvider CreateCodeDomProvider()
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.CSharp:
                    return new CSharpCodeProvider();
                case CodeDomProviderLanguage.VB:
                    return new VBCodeProvider();
                default:
                    throw new NotSupportedException();
            }
        }

        public CodeMemberMethod CreateMethod(CodeTypeDeclaration type)
        {
            var method = new CodeMemberMethod();
            type.Members.Add(method);
            return method;
        }

        public CodeStatement CreateDisableSourceLinePragmaStatement()
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.VB:
                    return new CodeSnippetStatement("#End ExternalSource");
                case CodeDomProviderLanguage.CSharp:
                    return new CodeSnippetStatement("#line hidden");
            }

            throw TargetLanguageNotSupportedException();
        }

        public IEnumerable<CodeStatement> CreateSourceLinePragmaStatement(string filename, int lineNo, int colNo)
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.VB:
                    yield return new CodeSnippetStatement($"#ExternalSource(\"{filename}\",{lineNo})");
                    yield return CreateCommentStatement($"#indentnext {colNo - 1}");
                    break;
                case CodeDomProviderLanguage.CSharp:
                    yield return new CodeSnippetStatement($"#line {lineNo}");
                    yield return CreateCommentStatement($"#indentnext {colNo - 1}");
                    break;
            }
        }

        public void MarkCodeMemberMethodAsAsync(CodeMemberMethod method)
        {
            var globalPrefix = TargetLanguage == CodeDomProviderLanguage.CSharp ? "global::" : string.Empty;

            if (IsVoid(method.ReturnType))
            {
                method.ReturnType = new CodeTypeReference($"async {globalPrefix}{typeof(Task).FullName}");
                return;
            }

            var returnTypeArgumentReferences = method.ReturnType.TypeArguments.OfType<CodeTypeReference>().ToArray();

            // Skip adding 'async' prefix for ValueTask in VB
            bool isValueTaskInVb = TargetLanguage == CodeDomProviderLanguage.VB && 
                                  method.ReturnType.BaseType.Contains("ValueTask");

            if (isValueTaskInVb)
            {
                return;
            }

            var asyncReturnType = new CodeTypeReference($"async {method.ReturnType.BaseType}", returnTypeArgumentReferences);
            method.ReturnType = asyncReturnType;
        }

        public bool IsVoid(CodeTypeReference codeTypeReference)
        {
            return typeof(void).FullName!.Equals(codeTypeReference.BaseType);
        }

        public CodeMethodInvokeExpression MarkCodeMethodInvokeExpressionAsAwait(CodeMethodInvokeExpression expression)
        {
            if (expression.Method.TargetObject is CodeVariableReferenceExpression variableExpression)
            {
                expression.Method.TargetObject = new CodeVariableReferenceExpression($"await {variableExpression.VariableName}");
            }
            else if (expression.Method.TargetObject is CodeFieldReferenceExpression fieldExpression 
                     && fieldExpression.TargetObject is CodeThisReferenceExpression thisFieldExpression)
            {
                expression.Method.TargetObject = new CodeFieldReferenceExpression(GetAwaitedMethodThisTargetObject(thisFieldExpression), fieldExpression.FieldName);
            }
            else if (expression.Method.TargetObject is CodeTypeReferenceExpression typeExpression)
            {
                string baseType = typeExpression.Type.BaseType;
                if (typeExpression.Type.Options.HasFlag(CodeTypeReferenceOptions.GlobalReference))
                {
                    typeExpression.Type.Options &= ~CodeTypeReferenceOptions.GlobalReference;
                    if (TargetLanguage == CodeDomProviderLanguage.CSharp)
                        baseType = $"global::{baseType}"; // For VB.NET, the BaseType already contains the "Global." prefix
                }
                expression.Method.TargetObject = new CodeTypeReferenceExpression(new CodeTypeReference($"await {baseType}", typeExpression.Type.Options));
            }
            else if (expression.Method.TargetObject is CodeThisReferenceExpression thisExpression)
            {
                expression.Method.TargetObject = GetAwaitedMethodThisTargetObject(expression.Method.TargetObject);
            }
            return expression;
        }

        private CodeExpression GetAwaitedMethodThisTargetObject(CodeExpression thisExpression)
        {
            return TargetLanguage switch
            {
                CodeDomProviderLanguage.VB => new CodeVariableReferenceExpression("await Me"),
                CodeDomProviderLanguage.CSharp => new CodeVariableReferenceExpression("await this"),
                _ => thisExpression
            };
        }

        public string GetGlobalizedName(string itemName)
        {
            string prefix = (TargetLanguage == CodeDomProviderLanguage.CSharp) ? "global::" : "Global.";

            return prefix + itemName;
        }

        public CodeExpression CreateOptionalArgumentExpression(string parameterName, CodeVariableReferenceExpression valueExpression)
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.CSharp:
                    return new CodeSnippetExpression($"{parameterName}: {valueExpression.VariableName}");
                case CodeDomProviderLanguage.VB:
                    return new CodeSnippetExpression($"{parameterName} := {valueExpression.VariableName}");
            }
            return valueExpression;
        }

        public IList<CodeStatement> GetFeatureFilewideUsingStatements()
        {
            var usingStatements = new List<CodeStatement>();
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.CSharp:
                    usingStatements.Add(new CodeSnippetStatement($"using {GeneratorConstants.REQNROLL_NAMESPACE};"));
                    break;
                case CodeDomProviderLanguage.VB:
                    usingStatements.Add(new CodeSnippetStatement($"Imports {GeneratorConstants.REQNROLL_NAMESPACE}"));
                    break;
            }
            return usingStatements;
        }
    }
}


