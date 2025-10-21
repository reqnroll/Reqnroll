using System;
using Xunit;
using Reqnroll.BindingSkeletons;
using FluentAssertions;

namespace Reqnroll.RuntimeTests.BindingSkeletons
{
    
    public class FileBasedSkeletonTemplateProviderTests
    {
        private class FileBasedSkeletonTemplateProviderStub : FileBasedSkeletonTemplateProvider
        {
            private readonly string templateFileContent;
            private readonly string missingTemplateResult;

            public FileBasedSkeletonTemplateProviderStub(string templateFileContent, string missingTemplateResult = null)
            {
                this.templateFileContent = StringHelpers.ConsolidateVerbatimStringLineEndings(templateFileContent);
                this.missingTemplateResult = missingTemplateResult;
            }

            protected override string GetTemplateFileContent()
            {
                return templateFileContent;
            }

            protected override string MissingTemplate(string key)
            {
                return missingTemplateResult;
            }
        }

        [Fact]
        public void Should_parse_step_definition_class_template()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>CSharp/StepDefinitionClass
mytemplate
>>>other");

            var result = sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.CSharp);
            result.Should().Be("mytemplate" + Environment.NewLine);
        }

        [Fact]
        public void Should_parse_sync_step_definition_expression_template()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>CSharp/StepDefinitionExpression
mytemplate
>>>other");

            var result = sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, true, false);
            result.Should().Be("mytemplate" + Environment.NewLine);
        }

        [Fact]
        public void Should_parse_sync_step_definition_template()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>CSharp/StepDefinition
mytemplate
>>>other");

            var result = sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, false, false);
            result.Should().Be("mytemplate" + Environment.NewLine);
        }
        [Fact]
        public void Should_parse_async_step_definition_expression_template()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>CSharp/AsyncStepDefinitionExpression
mytemplate
>>>other");

            var result = sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, true, true);
            result.Should().Be("mytemplate" + Environment.NewLine);
        }

        [Fact]
        public void Should_parse_async_step_definition_template()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>CSharp/AsyncStepDefinition
mytemplate
>>>other");

            var result = sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, false, true);
            result.Should().Be("mytemplate" + Environment.NewLine);
        }


        [Fact]
        public void Should_handle_missing_step_definition_class_template()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>other", "missing");

            var result = sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.CSharp);
            result.Should().Be("missing");
        }

        [Fact]
        public void Should_handle_missing_step_definition_regex_template()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>other", "missing");

            var result = sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, true, false);
            result.Should().Be("missing");
        }

        [Fact]
        public void Should_handle_missing_step_definition_template()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>other", "missing");

            var result = sut.GetStepDefinitionTemplate(ProgrammingLanguage.CSharp, false, false);
            result.Should().Be("missing");
        }

        [Fact]
        public void Should_handle_invalid_template_file_missing_title()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>
foo");

            Assert.Throws<ReqnrollException>(
                () => sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.CSharp));
        }

        [Fact]
        public void Should_handle_invalid_template_file_duplicate_title()
        {
            var sut = new FileBasedSkeletonTemplateProviderStub(@">>>title1
foo
>>>title1
bar");

            Assert.Throws<ReqnrollException>(
                () => sut.GetStepDefinitionClassTemplate(ProgrammingLanguage.CSharp));
        }

    }
}
