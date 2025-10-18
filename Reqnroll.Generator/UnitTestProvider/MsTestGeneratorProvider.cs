using System;
using System.CodeDom;
using System.Collections.Generic;
using Reqnroll.Generator.CodeDom;
using Reqnroll.BoDi;
using System.Diagnostics.CodeAnalysis;

namespace Reqnroll.Generator.UnitTestProvider
{
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    public class MsTestGeneratorProvider : IUnitTestGeneratorProvider
    {
        protected internal const string TESTFIXTURE_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute";
        protected internal const string TEST_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
        protected internal const string PROPERTY_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute";
        protected internal const string TESTFIXTURESETUP_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute";
        protected internal const string TESTFIXTURETEARDOWN_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute";
        protected internal const string CLASSCLEANUPBEHAVIOR_ENUM = "Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupBehavior";
        protected internal const string CLASSCLEANUPBEHAVIOR_ENDOFCLASS = "EndOfClass";
        protected internal const string TESTSETUP_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute";
        protected internal const string TESTTEARDOWN_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute";
        protected internal const string IGNORE_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute";
        protected internal const string DESCRIPTION_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute";
        protected internal const string FEATURE_TITILE_PROPERTY_NAME = "FeatureTitle";
        protected internal const string TESTCONTEXT_TYPE = "Microsoft.VisualStudio.TestTools.UnitTesting.TestContext";
        protected internal const string TESTCONTEXT_FIELD_NAME = "_testContext";
        protected internal const string TESTCONTEXT_PROPERTY_NAME = "TestContext";

        protected CodeDomHelper CodeDomHelper { get; set; }

        public virtual UnitTestGeneratorTraits GetTraits()
        {
            return UnitTestGeneratorTraits.None;
        }

        public bool GenerateParallelCodeForFeature { get; set; }

        public MsTestGeneratorProvider(CodeDomHelper codeDomHelper)
        {
            CodeDomHelper = codeDomHelper;
        }

        protected void SetProperty(CodeTypeMember codeTypeMember, string name, string value)
        {
            CodeDomHelper.AddAttribute(codeTypeMember, PROPERTY_ATTR, name, value);
        }

        public virtual void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, TESTFIXTURE_ATTR);

            // Add a TestContext field
            generationContext.TestClass.Members.Add(new CodeMemberField(TESTCONTEXT_TYPE, TESTCONTEXT_FIELD_NAME));

            // Add a TestContext property
            var testContextProperty = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public,
                Name = TESTCONTEXT_PROPERTY_NAME,
                HasGet = true,
                HasSet = true,
                Type = new CodeTypeReference(TESTCONTEXT_TYPE)
            };
            testContextProperty.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), TESTCONTEXT_FIELD_NAME)));
            testContextProperty.SetStatements.Add(new CodeAssignStatement(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), TESTCONTEXT_FIELD_NAME), new CodePropertySetValueReferenceExpression()));

            generationContext.TestClass.Members.Add(testContextProperty);
        }

        public virtual void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
        {
            //MsTest does not support categories... :(
        }

        public void SetTestClassIgnore(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, IGNORE_ATTR);
        }

        public virtual void FinalizeTestClass(TestClassGenerationContext generationContext)
        {
            // testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<TestContext>(_testContext);
            generationContext.ScenarioInitializeMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodePropertyReferenceExpression(
                            new CodePropertyReferenceExpression(
                                new CodeFieldReferenceExpression(null, generationContext.TestRunnerField.Name),
                                nameof(ScenarioContext)),
                            nameof(ScenarioContext.ScenarioContainer)),
                        nameof(IObjectContainer.RegisterInstanceAs),
                        new CodeTypeReference(TESTCONTEXT_TYPE)),
                    new CodeVariableReferenceExpression(TESTCONTEXT_FIELD_NAME)));
        }

        public virtual void SetTestClassNonParallelizable(TestClassGenerationContext generationContext)
        {
            //Not Supported            
        }

        public virtual void SetTestMethodNonParallelizable(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            // Not Supported
        }

        public virtual void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            generationContext.TestClassInitializeMethod.Attributes |= MemberAttributes.Static;

            generationContext.TestClassInitializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                TESTCONTEXT_TYPE, "testContext"));

            CodeDomHelper.AddAttribute(generationContext.TestClassInitializeMethod, TESTFIXTURESETUP_ATTR);
        }

        public virtual void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            generationContext.TestClassCleanupMethod.Attributes |= MemberAttributes.Static;
            // [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute(Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupBehavior.EndOfClass)]
            var attribute = CodeDomHelper.AddAttribute(generationContext.TestClassCleanupMethod, TESTFIXTURETEARDOWN_ATTR);
            attribute.Arguments.Add(new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(CLASSCLEANUPBEHAVIOR_ENUM), CLASSCLEANUPBEHAVIOR_ENDOFCLASS)));
        }


        public virtual void SetTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestInitializeMethod, TESTSETUP_ATTR);
        }

        public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestCleanupMethod, TESTTEARDOWN_ATTR);
        }

        public virtual void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string friendlyTestName)
        {
            if (generationContext.DisableFriendlyTestNames)
            {
                CodeDomHelper.AddAttribute(testMethod, TEST_ATTR);
            }
            else
            {
                CodeDomHelper.AddAttribute(testMethod, TEST_ATTR, friendlyTestName);
            }

            CodeDomHelper.AddAttribute(testMethod, DESCRIPTION_ATTR, friendlyTestName);

            //as in mstest, you cannot mark classes with the description attribute, we
            //just apply it for each test method as a property
            SetProperty(testMethod, FEATURE_TITILE_PROPERTY_NAME, generationContext.Feature.Name);
        }

        public virtual void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
        {
            //MsTest does not support caregories... :(
        }

        public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            CodeDomHelper.AddAttribute(testMethod, IGNORE_ATTR);
        }


        public virtual void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {
            //MsTest does not support row tests... :(
            throw new NotSupportedException();
        }

        public virtual void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
        {
            //MsTest does not support row tests... :(
            throw new NotSupportedException();
        }

        public virtual void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            if (!string.IsNullOrEmpty(exampleSetName))
            {
                SetProperty(testMethod, "ExampleSetName", exampleSetName);
            }

            if (!string.IsNullOrEmpty(variantName))
            {
                SetProperty(testMethod, "VariantName", variantName);
            }

            foreach (var pair in arguments)
            {
                SetProperty(testMethod, "Parameter:" + pair.Key, pair.Value);
            }
        }

        public void MarkCodeMethodInvokeExpressionAsAwait(CodeMethodInvokeExpression expression)
        {
            CodeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);
        }
    }
}
