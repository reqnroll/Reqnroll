using Xunit;
using Reqnroll.BindingSkeletons;
using FluentAssertions;

namespace Reqnroll.RuntimeTests.BindingSkeletons
{
    
    public class ResourceSkeletonTemplateProviderTests
    {
        private void ShouldNotBeMissing(string template)
        {
            template.Should().NotBeNull();
            template.Should().NotBeEmpty();
            template.Should().NotBe("undefined template");
        }

        [Fact]
        public void Should_provide_csharp_templates()
        {
            var sut = new ResourceSkeletonTemplateProvider();

            ShouldNotBeMissing(sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.CSharp));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, true, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, false, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, true, true));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, false, true));
        }

        [Fact]
        public void Should_provide_vb_templates()
        {
            var sut = new ResourceSkeletonTemplateProvider();

            ShouldNotBeMissing(sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.VB));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.VB, true, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.VB, false, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.VB, true, true));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.VB, false, true));
        }

        [Fact]
        public void Should_provide_fsharp_templates()
        {
            var sut = new ResourceSkeletonTemplateProvider();

            ShouldNotBeMissing(sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.FSharp));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.FSharp, true, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.FSharp, false, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.FSharp, true, true));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.FSharp, false, true));
        }
    }

    
    public class DefaultSkeletonTemplateProviderTests
    {
        private void ShouldNotBeMissing(string template)
        {
            template.Should().NotBeNull();
            template.Should().NotBeEmpty();
            template.Should().NotBe("undefined template");
        }

        [Fact]
        public void Should_provide_csharp_templates()
        {
            var sut = new DefaultSkeletonTemplateProvider(new ResourceSkeletonTemplateProvider());

            ShouldNotBeMissing(sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.CSharp));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, true, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, false, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, true, true));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, false, true));
        }

        [Fact]
        public void Should_provide_vb_templates()
        {
            var sut = new DefaultSkeletonTemplateProvider(new ResourceSkeletonTemplateProvider());

            ShouldNotBeMissing(sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.VB));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.VB, true, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.VB, false, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.VB, true, true));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.VB, false, true));
        }

        [Fact]
        public void Should_provide_fsharp_templates()
        {
            var sut = new DefaultSkeletonTemplateProvider(new ResourceSkeletonTemplateProvider());

            ShouldNotBeMissing(sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.FSharp));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.FSharp, true, false));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.FSharp, false, false)); 
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.FSharp, true, true));
            ShouldNotBeMissing(sut.GetStepDefinitionTemplate(ProgrammingLanguage.FSharp, false, true));

        }
    }
}
