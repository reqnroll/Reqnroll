using System.CodeDom;
using System.Collections.Generic;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;

namespace Reqnroll.Generator
{
    public class TestClassGenerationContext
    {
        public IUnitTestGeneratorProvider UnitTestGeneratorProvider { get; private set; }
        public ReqnrollDocument Document { get; private set; }

        public ReqnrollFeature Feature => Document.ReqnrollFeature;

        public CodeNamespace Namespace { get; private set; }
        public CodeTypeDeclaration TestClass { get; private set; }
        public CodeMemberMethod TestClassInitializeMethod { get; private set; }
        public CodeMemberMethod TestClassCleanupMethod { get; private set; }
        public CodeMemberMethod TestInitializeMethod { get; private set; }
        public CodeMemberMethod TestCleanupMethod { get; private set; }
        public CodeMemberMethod ScenarioInitializeMethod { get; private set; }
        public CodeMemberMethod ScenarioStartMethod { get; private set; }
        public CodeMemberMethod ScenarioCleanupMethod { get; private set; }
        public CodeMemberMethod FeatureBackgroundMethod { get; private set; }
        public CodeMemberField TestRunnerField { get; private set; }
        public CodeMemberMethod CucumberMessagesInitializationMethod { get; private set; }

        public bool GenerateRowTests { get; private set; }

        public bool DisableFriendlyTestNames { get; private set; }

        public IDictionary<string, object> CustomData { get; private set; }

        public ICollection<string> GenerationWarnings { get; private set; }

        public string FeatureMessagesResourceName { get; set; }
        internal string FeatureMessages { get; set; }

        public TestClassGenerationContext(
            IUnitTestGeneratorProvider unitTestGeneratorProvider,
            ReqnrollDocument document,
            CodeNamespace ns,
            CodeTypeDeclaration testClass,
            CodeMemberField testRunnerField,
            CodeMemberMethod testClassInitializeMethod,
            CodeMemberMethod testClassCleanupMethod,
            CodeMemberMethod testInitializeMethod,
            CodeMemberMethod testCleanupMethod,
            CodeMemberMethod scenarioInitializeMethod,
            CodeMemberMethod scenarioStartMethod,
            CodeMemberMethod scenarioCleanupMethod,
            CodeMemberMethod featureBackgroundMethod,
            CodeMemberMethod cucumberMessagesInitializationMethod,
            bool generateRowTests,
            bool disableFriendlyTestNames)
        {
            UnitTestGeneratorProvider = unitTestGeneratorProvider;
            Document = document;
            Namespace = ns;
            TestClass = testClass;
            TestRunnerField = testRunnerField;
            TestClassInitializeMethod = testClassInitializeMethod;
            TestClassCleanupMethod = testClassCleanupMethod;
            TestInitializeMethod = testInitializeMethod;
            TestCleanupMethod = testCleanupMethod;
            ScenarioInitializeMethod = scenarioInitializeMethod;
            ScenarioStartMethod = scenarioStartMethod;
            ScenarioCleanupMethod = scenarioCleanupMethod;
            FeatureBackgroundMethod = featureBackgroundMethod;
            CucumberMessagesInitializationMethod = cucumberMessagesInitializationMethod;
            GenerateRowTests = generateRowTests;
            DisableFriendlyTestNames = disableFriendlyTestNames;

            CustomData = new Dictionary<string, object>();
            GenerationWarnings = new List<string>();
        }
    }
}