using Reqnroll.Bindings;

namespace Reqnroll.RuntimeTests
{
    /// <summary>
    /// Shared test fixtures for binding registry builder tests.
    /// These classes are used by both RuntimeBindingRegistryBuilderTests and MetadataLoadContextBindingRegistryBuilderTests.
    /// </summary>
    public class BindingRegistryBuilderTestFixtures
    {
        [Binding]
        public class StepTransformationExample
        {
            [StepArgumentTransformation("BindingRegistryTests")]
            public int Transform(string val)
            {
                return 42;
            }
            
            [StepArgumentTransformation(Regex="BindingRegistryTests2", Order = 5)]
            public int TransformWithRegexAndOrder(string val)
            {
                return 43;
            }
            
            [StepArgumentTransformation(Order = 10)]
            public int TransformWithOrderAndWithoutRegex(string val)
            {
                return 44;
            } 
        }

        [Binding]
        public class ScopedStepTransformationExample
        {
            [Then("SpecificBindingRegistryTests")]
            [Scope(Feature = "SomeFeature")]
            public int Transform(string val)
            {
                return 42;
            }
        }

        [Binding]
        public class ScopedStepTransformationExampleTheOther
        {
            [Then("SpecificBindingRegistryTests")]
            [Scope(Feature = "AnotherFeature")]
            public int Transform(string val)
            {
                return 24;
            }
        }

        [Binding]
        public class ScopedHookExample
        {
            [BeforeScenario]
            [Scope(Tag = "tag1")]
            public void Tag1BeforeScenario()
            {
            }

            [BeforeScenario("tag2")]
            public void Tag2BeforeScenario()
            {
            }

            [BeforeScenario("tag3", "tag4")]
            public void Tag34BeforeScenario()
            {
            }
        }

        [Binding]
        public class PrioritizedHookExample
        {
            [BeforeScenario]
            public void OrderTenThousand()
            {
            }

            [Before(Order = 9000)]
            public void OrderNineThousand()
            {
            }

            [BeforeScenarioBlock(Order = 10001)]
            public void OrderTenThousandAnd1()
            {
            }

            [BeforeFeature(Order = 10002)]
            public static void OrderTenThousandAnd2()
            {
            }

            [BeforeStep(Order = 10003)]
            public void OrderTenThousandAnd3()
            {
            }

            [BeforeTestRun(Order = 10004)]
            public static void OrderTenThousandAnd4()
            {
            }

            [AfterScenario]
            public void AfterOrderTenThousand()
            {
            }

            [After(Order = 9000)]
            public void AfterOrderNineThousand()
            {
            }

            [AfterScenarioBlock(Order = 10001)]
            public void AfterOrderTenThousandAnd1()
            {
            }

            [AfterFeature(Order = 10002)]
            public static void AfterOrderTenThousandAnd2()
            {
            }

            [AfterStep(Order = 10003)]
            public void AfterOrderTenThousandAnd3()
            {
            }

            [AfterTestRun(Order = 10004)]
            public static void AfterOrderTenThousandAnd4()
            {
            }
        }

        [Binding]
        public class BindingClassWithStepDefinitionAttributes
        {
            [Given("I have done something")]
            public void GivenIHaveDoneSomething()
            {
            }

            [When("I do something")]
            public void WhenIDoSomething()
            {
            }

            [Then("something should happen")]
            public void ThenSomethingShouldHappen()
            {
            }
        }

        [Binding]
        public class BindingClassWithTranslatedStepDefinitionAttributes
        {
            public class AngenommenAttribute : GivenAttribute
            {
                public AngenommenAttribute(string expression) : base(expression)
                {
                }
            }
            public class WennAttribute : WhenAttribute
            {
                public WennAttribute(string expression) : base(expression)
                {
                }
            }
            public class DannAttribute : ThenAttribute
            {
                public DannAttribute(string expression) : base(expression)
                {
                }
            }

            [Angenommen("mache ich was")]
            public void AngenommenMacheIchWas()
            {
            }

            [Wenn("ich etwas mache")]
            public void WennIchEtwasMache()
            {
            }

            [Dann("sollte etwas passieren")]
            public void DannSollteEtwasPassieren()
            {
            }
        }

        [Binding]
        public class BindingClassWithCustomStepDefinitionAttribute
        {
            public class GivenAndWhenAttribute : StepDefinitionBaseAttribute
            {
                public GivenAndWhenAttribute(string expression)
                    : base(expression, new[] { StepDefinitionType.Given, StepDefinitionType.When } )
                {
                }
            }

            [GivenAndWhen("given and when")]
            public void GivenAndWhen()
            {
            }
        }
    }
}
