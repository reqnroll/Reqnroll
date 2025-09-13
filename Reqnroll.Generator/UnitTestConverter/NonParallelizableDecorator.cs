using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Configuration;
using Reqnroll.Generator.UnitTestProvider;

namespace Reqnroll.Generator.UnitTestConverter
{
    public class NonParallelizableDecorator : ITestClassTagDecorator, ITestMethodTagDecorator
    {
        private readonly string[] nonParallelizableTags;
        private readonly ITagFilterMatcher tagFilterMatcher;

        public NonParallelizableDecorator(ITagFilterMatcher tagFilterMatcher, ReqnrollConfiguration generatorConfiguration)
        {
            this.tagFilterMatcher = tagFilterMatcher;
            nonParallelizableTags = generatorConfiguration.AddNonParallelizableMarkerForTags;
        }

        public int Priority
        {
            get { return PriorityValues.Low; }
        }

        public bool RemoveProcessedTags
        {
            get { return false; }
        }

        public bool ApplyOtherDecoratorsForProcessedTags
        {
            get { return true; }
        }

        public bool CanDecorateFrom(string tagName, TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            return ProviderSupportsParallelExecution(generationContext) && ConfiguredTagIsPresent(new[] { tagName });
        }

        public void DecorateFrom(string tagName, TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            generationContext.UnitTestGeneratorProvider.SetTestMethodNonParallelizable(generationContext, testMethod);
        }

        public bool CanDecorateFrom(string tagName, TestClassGenerationContext generationContext)
        {
            return ProviderSupportsParallelExecution(generationContext) && ConfiguredTagIsPresent(new[] { tagName });
        }

        public void DecorateFrom(string tagName, TestClassGenerationContext generationContext)
        {
            generationContext.UnitTestGeneratorProvider.SetTestClassNonParallelizable(generationContext);
        }

        private bool ProviderSupportsParallelExecution(TestClassGenerationContext generationContext)
        {
            return generationContext.UnitTestGeneratorProvider.GetTraits()
                .HasFlag(UnitTestGeneratorTraits.ParallelExecution);
        }

        private bool ConfiguredTagIsPresent(IEnumerable<string> tagName)
        {
            return nonParallelizableTags?.Any(x => tagFilterMatcher.Match(x, tagName)) ?? false;
        }
    }
}