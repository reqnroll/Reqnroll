using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Generator.CodeDom;
using Reqnroll.BoDi;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Reqnroll.Generator.UnitTestProvider
{
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    public class XUnit2TestGeneratorProvider : IUnitTestGeneratorProvider
    {
        private CodeTypeDeclaration _currentFixtureDataTypeDeclaration = null;
        private readonly CodeTypeReference _objectCodeTypeReference = new(typeof(object));
        protected internal const string THEORY_ATTRIBUTE = "Xunit.SkippableTheoryAttribute";
        protected internal const string INLINEDATA_ATTRIBUTE = "Xunit.InlineDataAttribute";
        protected internal const string ICLASSFIXTURE_INTERFACE = "Xunit.IClassFixture";
        protected internal const string COLLECTION_ATTRIBUTE = "Xunit.CollectionAttribute";
        protected internal const string OUTPUT_INTERFACE = "Xunit.Abstractions.ITestOutputHelper";
        protected internal const string OUTPUT_INTERFACE_PARAMETER_NAME = "testOutputHelper";
        protected internal const string OUTPUT_INTERFACE_FIELD_NAME = "_testOutputHelper";
        protected internal const string FIXTUREDATA_PARAMETER_NAME = "fixtureData";
        protected internal const string COLLECTION_DEF = "Xunit.Collection";
        protected internal const string COLLECTION_TAG = "xunit:collection";
        protected internal const string FEATURE_TITLE_PROPERTY_NAME = "FeatureTitle";
        protected internal const string DESCRIPTION_PROPERTY_NAME = "Description";
        protected internal const string FACT_ATTRIBUTE = "Xunit.SkippableFactAttribute";
        protected internal const string FACT_ATTRIBUTE_SKIP_PROPERTY_NAME = "Skip";
        protected internal const string THEORY_ATTRIBUTE_SKIP_PROPERTY_NAME = "Skip";
        protected internal const string SKIP_REASON = "Ignored";
        protected internal const string TRAIT_ATTRIBUTE = "Xunit.TraitAttribute";
        protected internal const string CATEGORY_PROPERTY_NAME = "Category";
        protected internal const string IGNORE_TEST_CLASS = "IgnoreTestClass";
        protected internal const string NONPARALLELIZABLE_COLLECTION_NAME = "ReqnrollNonParallelizableFeatures";
        protected internal const string IASYNCLIFETIME_INTERFACE = "Xunit.IAsyncLifetime";

        public XUnit2TestGeneratorProvider(CodeDomHelper codeDomHelper)
        {
            CodeDomHelper = codeDomHelper;
        }

        public virtual void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
        }

        public virtual UnitTestGeneratorTraits GetTraits()
        {
            return UnitTestGeneratorTraits.RowTests | UnitTestGeneratorTraits.ParallelExecution;
        }

        protected virtual CodeTypeReference CreateFixtureInterface(TestClassGenerationContext generationContext, CodeTypeReference fixtureDataType)
        {
            // Add a field for the ITestOutputHelper
            generationContext.TestClass.Members.Add(new CodeMemberField(new CodeTypeReference(OUTPUT_INTERFACE, CodeTypeReferenceOptions.GlobalReference), OUTPUT_INTERFACE_FIELD_NAME));

            // Store the fixture data type for later use in constructor
            generationContext.CustomData.Add(FIXTUREDATA_PARAMETER_NAME, fixtureDataType);

            var classFixtureCodeType = new CodeTypeReference(ICLASSFIXTURE_INTERFACE, fixtureDataType);
            classFixtureCodeType.Options = CodeTypeReferenceOptions.GlobalReference;
            return classFixtureCodeType;
        }

        protected CodeDomHelper CodeDomHelper { get; set; }

        public virtual void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {
            if (generationContext.DisableFriendlyTestNames)
            {
                CodeDomHelper.AddAttribute(testMethod, THEORY_ATTRIBUTE);
            }
            else
            {
                CodeDomHelper.AddAttribute(testMethod, THEORY_ATTRIBUTE, new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(scenarioTitle)));
            }

            SetProperty(testMethod, FEATURE_TITLE_PROPERTY_NAME, generationContext.Feature.Name);
            SetDescription(testMethod, scenarioTitle);
        }

        public virtual void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
        {
            //TODO: better handle "ignored"
            if (isIgnored)
            {
                return;
            }

            var args = arguments.Select(arg => new CodeAttributeArgument(new CodePrimitiveExpression(arg))).ToList();

            args.Add(
                new CodeAttributeArgument(
                    new CodeArrayCreateExpression(typeof(string[]), tags.Select(t => (CodeExpression)new CodePrimitiveExpression(t)).ToArray())));

            CodeDomHelper.AddAttribute(testMethod, INLINEDATA_ATTRIBUTE, args.ToArray());
        }

        protected virtual void SetTestConstructor(TestClassGenerationContext generationContext, CodeConstructor constructor)
        {
            constructor.Parameters.Add(
                new CodeParameterDeclarationExpression((CodeTypeReference)generationContext.CustomData[FIXTUREDATA_PARAMETER_NAME], FIXTUREDATA_PARAMETER_NAME));
            constructor.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(OUTPUT_INTERFACE, CodeTypeReferenceOptions.GlobalReference), OUTPUT_INTERFACE_PARAMETER_NAME));

            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), OUTPUT_INTERFACE_FIELD_NAME),
                    new CodeVariableReferenceExpression(OUTPUT_INTERFACE_PARAMETER_NAME)));
        }

        private CodeStatement GetTryCatchStatementWithCombinedErrors(CodeExpression tryExpression, CodeExpression catchExpression)
        {
            var tryStatement = new CodeExpressionStatement(tryExpression);
            var catchStatement = new CodeExpressionStatement(catchExpression);

            // try { <tryExpression>; }
            // catch (Exception e1) {
            //   try { <catchExpression>; }
            //   catch (Exception e2) {
            //     throw new AggregateException(e1.Message, e1, e2);
            //   }
            //   throw;
            // }

            return new CodeTryCatchFinallyStatement(
                [tryStatement],
                [
                    new CodeCatchClause(
                        "e1",
                        new CodeTypeReference(typeof(Exception)),
                        new CodeTryCatchFinallyStatement(
                            [catchStatement],
                            [
                                new CodeCatchClause(
                                    "e2",
                                    new CodeTypeReference(typeof(Exception)),
                                    new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(AggregateException), new CodePrimitiveExpression("Test initialization failed"), new CodeVariableReferenceExpression("e1"), new CodeVariableReferenceExpression("e2"))))
                                ]),
                        new CodeThrowExceptionStatement())
                ]);
        }

        protected virtual void SetTestInitializeMethod(TestClassGenerationContext generationContext, CodeMemberMethod method)
        {
            var callTestInitializeMethodExpression = new CodeMethodInvokeExpression(
                new CodeThisReferenceExpression(),
                generationContext.TestInitializeMethod.Name);
            CodeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(callTestInitializeMethodExpression);

            // xUnit (v2) does not invoke the test level dispose through the IAsyncLifetime if
            // there is an error during the test initialization. So we need to call it manually, 
            // otherwise the test runner will not be released.

            var callDisposeException = new CodeMethodInvokeExpression(
                new CodeCastExpression(new CodeTypeReference(IASYNCLIFETIME_INTERFACE, CodeTypeReferenceOptions.GlobalReference), new CodeThisReferenceExpression()),
                "DisposeAsync");
            CodeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(callDisposeException);

            method.Statements.Add(
                GetTryCatchStatementWithCombinedErrors(callTestInitializeMethodExpression, callDisposeException));
        }

        public virtual void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            var factAttr = testMethod.CustomAttributes
                                     .OfType<CodeAttributeDeclaration>()
                                     .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == FACT_ATTRIBUTE);

            // set [FactAttribute(Skip="reason")]
            factAttr?.Arguments.Add(new CodeAttributeArgument(FACT_ATTRIBUTE_SKIP_PROPERTY_NAME, new CodePrimitiveExpression(SKIP_REASON)));

            var theoryAttr = testMethod.CustomAttributes
                                       .OfType<CodeAttributeDeclaration>()
                                       .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == THEORY_ATTRIBUTE);

            // set [TheoryAttribute(Skip="reason")]
            theoryAttr?.Arguments.Add(new CodeAttributeArgument(THEORY_ATTRIBUTE_SKIP_PROPERTY_NAME, new CodePrimitiveExpression(SKIP_REASON)));
        }

        public virtual void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
        {
            var featureCategoriesArray = featureCategories.ToArray();
            var collectionCategories = featureCategoriesArray.Where(f => f.StartsWith(COLLECTION_TAG, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (collectionCategories.Any())
            {
                //Only one 'Xunit.Collection' can exist per class.
                SetTestClassCollection(generationContext, collectionCategories.FirstOrDefault());
            }

            // Set Category trait which can be used with the /trait or /-trait xunit flags to include/exclude tests
            foreach (string str in featureCategoriesArray)
            {
                SetProperty(generationContext.TestClass, CATEGORY_PROPERTY_NAME, str);
            }
        }

        public void SetTestClassCollection(TestClassGenerationContext generationContext, string collection)
        {
            //No spaces. 
            //'-', and '_' are allowed.
            string collectionMatch = $@"(?<={COLLECTION_TAG}[(])[A-Za-z0-9\-_]+.*?(?=[)])";
            string description = Regex.Match(collection, collectionMatch, RegexOptions.IgnoreCase).Value;
            CodeDomHelper.AddAttribute(generationContext.TestClass, COLLECTION_DEF, description);
        }

        public virtual void SetTestClassNonParallelizable(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, COLLECTION_ATTRIBUTE, new CodeAttributeArgument(new CodePrimitiveExpression(NONPARALLELIZABLE_COLLECTION_NAME)));
        }

        public virtual void SetTestMethodNonParallelizable(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            // xUnit does not support method-level parallelization
        }

        public virtual void FinalizeTestClass(TestClassGenerationContext generationContext)
        {
            IgnoreFeature(generationContext);

            // testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<ITestOutputHelper>(_testOutputHelper);
            generationContext.ScenarioInitializeMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodePropertyReferenceExpression(
                            new CodePropertyReferenceExpression(
                                new CodeFieldReferenceExpression(null, generationContext.TestRunnerField.Name),
                                nameof(ScenarioContext)),
                            nameof(ScenarioContext.ScenarioContainer)),
                        nameof(IObjectContainer.RegisterInstanceAs),
                        new CodeTypeReference(OUTPUT_INTERFACE, CodeTypeReferenceOptions.GlobalReference)),
                    new CodeVariableReferenceExpression(OUTPUT_INTERFACE_FIELD_NAME)));
        }

        protected virtual void IgnoreFeature(TestClassGenerationContext generationContext)
        {
            var featureHasIgnoreTag = generationContext.CustomData.TryGetValue(IGNORE_TEST_CLASS, out var featureHasIgnoreTagValue) && (bool)featureHasIgnoreTagValue;

            if (featureHasIgnoreTag)
            {
                //Ignore all test methods
                foreach (CodeTypeMember member in generationContext.TestClass.Members)
                {
                    var method = member as CodeMemberMethod;
                    if (method != null && !IsTestMethodAlreadyIgnored(method))
                    {
                        SetTestMethodIgnore(generationContext, method);
                    }
                }

                //Clear FixtureData method statements to skip the Before/AfterFeature hooks
                foreach (CodeTypeMember member in _currentFixtureDataTypeDeclaration.Members)
                {
                    var method = member as CodeMemberMethod;
                    if (method != null)
                    {
                        method.Statements.Clear();
                    }
                }
            }
        }

        protected virtual bool IsTestMethodAlreadyIgnored(CodeMemberMethod testMethod)
        {
            return IsTestMethodAlreadyIgnored(testMethod, FACT_ATTRIBUTE, THEORY_ATTRIBUTE);
        }

        public void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            // xUnit uses IUseFixture<T> on the class
            generationContext.TestClassInitializeMethod.Attributes |= MemberAttributes.Static;

            _currentFixtureDataTypeDeclaration = CodeDomHelper.CreateGeneratedTypeDeclaration("FixtureData");
            generationContext.TestClass.Members.Add(_currentFixtureDataTypeDeclaration);

            var fixtureDataType =
                CodeDomHelper.CreateNestedTypeReference(generationContext.TestClass, _currentFixtureDataTypeDeclaration.Name);

            var useFixtureType = CreateFixtureInterface(generationContext, fixtureDataType);

            generationContext.TestClass.BaseTypes.Add(_objectCodeTypeReference);
            generationContext.TestClass.BaseTypes.Add(useFixtureType);

            // Task IAsyncLifetime.InitializeAsync() { <fixtureSetupMethod>(); }
            var initializeMethod = new CodeMemberMethod();
            initializeMethod.PrivateImplementationType = new CodeTypeReference(IASYNCLIFETIME_INTERFACE, CodeTypeReferenceOptions.GlobalReference);
            initializeMethod.Name = "InitializeAsync";

            CodeDomHelper.MarkCodeMemberMethodAsAsync(initializeMethod);

            _currentFixtureDataTypeDeclaration.Members.Add(initializeMethod);

            var expression = new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(new CodeTypeReference(generationContext.TestClass.Name)),
                generationContext.TestClassInitializeMethod.Name);

            CodeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            initializeMethod.Statements.Add(expression);
        }

        public void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            // xUnit uses IUseFixture<T> on the class

            generationContext.TestClassCleanupMethod.Attributes |= MemberAttributes.Static;

            var iasyncLifetimeInterface = new CodeTypeReference(IASYNCLIFETIME_INTERFACE, CodeTypeReferenceOptions.GlobalReference);

            // have to add the explicit object base class because of VB.NET
            _currentFixtureDataTypeDeclaration.BaseTypes.Add(typeof(object));
            _currentFixtureDataTypeDeclaration.BaseTypes.Add(iasyncLifetimeInterface);

            // Task IAsyncLifetime.DisposeAsync() { <fixtureTearDownMethod>(); }
            var disposeMethod = new CodeMemberMethod();
            disposeMethod.PrivateImplementationType = iasyncLifetimeInterface;
            disposeMethod.Name = "DisposeAsync";
            disposeMethod.ImplementationTypes.Add(iasyncLifetimeInterface);

            CodeDomHelper.MarkCodeMemberMethodAsAsync(disposeMethod);

            _currentFixtureDataTypeDeclaration.Members.Add(disposeMethod);

            var expression = new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(new CodeTypeReference(generationContext.TestClass.Name)),
                generationContext.TestClassCleanupMethod.Name);

            CodeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            disposeMethod.Statements.Add(expression);
        }

        public virtual void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string friendlyTestName)
        {
            if (generationContext.DisableFriendlyTestNames)
            {
                CodeDomHelper.AddAttribute(testMethod, FACT_ATTRIBUTE);
            }
            else
            {
                CodeDomHelper.AddAttribute(testMethod, FACT_ATTRIBUTE, new CodeAttributeArgument("DisplayName", new CodePrimitiveExpression(friendlyTestName)));
            }

            SetProperty(testMethod, FEATURE_TITLE_PROPERTY_NAME, generationContext.Feature.Name);
            SetDescription(testMethod, friendlyTestName);
        }

        public virtual void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
        {
            foreach (string str in scenarioCategories)
            {
                SetProperty(testMethod, "Category", str);
            }
        }

        public void SetTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            // xUnit uses a parameterless constructor & IAsyncLifetime.InitializeAsync

            // public <_currentTestTypeDeclaration>() { <memberMethod>(); }

            var ctorMethod = new CodeConstructor();
            ctorMethod.Attributes = MemberAttributes.Public;
            generationContext.TestClass.Members.Add(ctorMethod);

            SetTestConstructor(generationContext, ctorMethod);

            // Task IAsyncLifetime.InitializeAsync() { <memberMethod>(); }

            var initializeMethod = new CodeMemberMethod();
            initializeMethod.Attributes = MemberAttributes.Public;
            initializeMethod.PrivateImplementationType = new CodeTypeReference(IASYNCLIFETIME_INTERFACE, CodeTypeReferenceOptions.GlobalReference);
            initializeMethod.Name = "InitializeAsync";

            CodeDomHelper.MarkCodeMemberMethodAsAsync(initializeMethod);

            generationContext.TestClass.Members.Add(initializeMethod);

            SetTestInitializeMethod(generationContext, initializeMethod);
        }

        public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            // xUnit supports test tear down through the IAsyncLifetime interface

            var iasyncLifetimeInterface = new CodeTypeReference(IASYNCLIFETIME_INTERFACE, CodeTypeReferenceOptions.GlobalReference);

            generationContext.TestClass.BaseTypes.Add(iasyncLifetimeInterface);

            // Task IAsyncLifetime.DisposeAsync() { <memberMethod>(); }

            var disposeMethod = new CodeMemberMethod();
            disposeMethod.PrivateImplementationType = iasyncLifetimeInterface;
            disposeMethod.Name = "DisposeAsync";

            CodeDomHelper.MarkCodeMemberMethodAsAsync(disposeMethod);

            generationContext.TestClass.Members.Add(disposeMethod);

            var expression = new CodeMethodInvokeExpression(
                new CodeThisReferenceExpression(),
                generationContext.TestCleanupMethod.Name);

            CodeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

            disposeMethod.Statements.Add(expression);
        }

        public void SetTestClassIgnore(TestClassGenerationContext generationContext)
        {
            // The individual tests have to get Skip argument in their attributes
            // xUnit does not provide a way to Skip a set of tests - https://xunit.github.io/docs/comparisons.html#attributes
            // This is handled in FinalizeTestClass

            // Store in custom data that the test class should be ignored
            generationContext.CustomData.Add(IGNORE_TEST_CLASS, true);
        }

        protected void SetProperty(CodeTypeMember codeTypeMember, string name, string value)
        {
            CodeDomHelper.AddAttribute(codeTypeMember, TRAIT_ATTRIBUTE, name, value);
        }

        protected void SetDescription(CodeTypeMember codeTypeMember, string description)
        {
            // xUnit doesn't have a DescriptionAttribute so using a TraitAttribute instead
            SetProperty(codeTypeMember, DESCRIPTION_PROPERTY_NAME, description);
        }

        protected bool IsTestMethodAlreadyIgnored(CodeMemberMethod testMethod, string factAttributeName, string theoryAttributeName)
        {
            var factAttr = testMethod.CustomAttributes.OfType<CodeAttributeDeclaration>()
                                     .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == factAttributeName);

            var hasIgnoredFact = factAttr?.Arguments
                                         .OfType<CodeAttributeArgument>()
                                         .Any(x => string.Equals(x.Name, FACT_ATTRIBUTE_SKIP_PROPERTY_NAME, StringComparison.InvariantCultureIgnoreCase));

            var theoryAttr = testMethod.CustomAttributes
                                       .OfType<CodeAttributeDeclaration>()
                                       .FirstOrDefault(codeAttributeDeclaration => codeAttributeDeclaration.Name == theoryAttributeName);

            var hasIgnoredTheory = theoryAttr?.Arguments
                                             .OfType<CodeAttributeArgument>()
                                             .Any(x => string.Equals(x.Name, THEORY_ATTRIBUTE_SKIP_PROPERTY_NAME, StringComparison.InvariantCultureIgnoreCase));

            bool result = hasIgnoredFact.GetValueOrDefault() || hasIgnoredTheory.GetValueOrDefault();

            return result;
        }

        public void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            // doing nothing since we support RowTest
        }

        public void MarkCodeMethodInvokeExpressionAsAwait(CodeMethodInvokeExpression expression)
        {
            CodeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);
        }
    }
}
