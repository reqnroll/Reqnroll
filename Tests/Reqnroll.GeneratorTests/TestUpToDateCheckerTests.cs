using System.IO;
using System.Threading;
using Xunit;
using Reqnroll.Generator;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Interfaces;
using FluentAssertions;
using Reqnroll.GeneratorTests.Helper;

namespace Reqnroll.GeneratorTests
{
    
    public class TestUpToDateCheckerTests
    {
        private TestUpToDateChecker CreateUpToDateChecker()
        {
            var net35CsSettings = new ProjectPlatformSettings
            {
                Language = GenerationTargetLanguage.CSharp
            };

            return new TestUpToDateChecker(
                new GeneratorInfo { GeneratorVersion = GeneratorInfoProvider.GeneratorVersion}, 
                new ProjectSettings { ProjectFolder = Path.GetTempPath(), ProjectPlatformSettings = net35CsSettings });
        }

        [Fact]
        public void Should_detect_up_to_date_test_file_based_on_modification_time()
        {
            using (var tempFile = new TempFile(".feature"))
            {
                tempFile.SetContent("any");

                using (var tempTestFile = new TempFile(".cs"))
                {
                    // set test content
                    tempTestFile.SetContent("any_code");

                    var testUpToDateChecker = CreateUpToDateChecker();

                    var result = testUpToDateChecker.IsUpToDatePreliminary(new FeatureFileInput(tempFile.FileName),
                        tempTestFile.FullPath, UpToDateCheckingMethod.ModificationTime);

                    result.Should().Be(true);
                }
            }
        }

        [Fact]
        public void Should_detect_outdated_date_test_file_if_feature_file_missing()
        {
            using (var tempFile = new TempFile(".feature"))
            {
                tempFile.SetContent("any");

                var testUpToDateChecker = CreateUpToDateChecker();
                var result = testUpToDateChecker.IsUpToDatePreliminary(new FeatureFileInput(tempFile.FileName),
                    tempFile.FileName + ".cs", UpToDateCheckingMethod.ModificationTime);

                result.Should().Be(false);
            }
        }

        [Fact]
        public void Should_detect_outdated_date_test_file_if_feature_file_changed_based_on_modification_time()
        {
            using (var tempFile = new TempFile(".feature"))
            {
                tempFile.SetContent("any");

                using (var tempTestFile = new TempFile(".cs"))
                {
                    // set test content
                    tempTestFile.SetContent("any_code");

                    //re-set content with a slight delay
                    Thread.Sleep(10);
                    tempFile.SetContent("new_feature");

                    var testUpToDateChecker = CreateUpToDateChecker();

                    var result = testUpToDateChecker.IsUpToDatePreliminary(new FeatureFileInput(tempFile.FileName),
                        tempTestFile.FullPath, UpToDateCheckingMethod.ModificationTime);

                    result.Should().Be(false);
                }
            }
        }

        [Fact]
        public void Should_not_give_preliminary_positive_result_if_file_content_check_was_requested()
        {
            using (var tempFile = new TempFile(".feature"))
            {
                tempFile.SetContent("any");

                using (var tempTestFile = new TempFile(".cs"))
                {
                    // set test content
                    tempTestFile.SetContent("any_code");

                    var testUpToDateChecker = CreateUpToDateChecker();

                    var result = testUpToDateChecker.IsUpToDatePreliminary(new FeatureFileInput(tempFile.FileName),
                        tempTestFile.FullPath, UpToDateCheckingMethod.FileContent);

                    result.Should().NotHaveValue();
                }
            }
        }

        [Fact]
        public void Should_detect_up_to_date_test_file_based_on_content_compare_from_file()
        {
            using (var tempFile = new TempFile(".feature"))
            {
                tempFile.SetContent("any");

                using (var tempTestFile = new TempFile(".cs"))
                {
                    // set test content
                    tempTestFile.SetContent("any_code");

                    var testUpToDateChecker = CreateUpToDateChecker();

                    var result = testUpToDateChecker.IsUpToDate(new FeatureFileInput(tempFile.FileName),
                        tempTestFile.FullPath, "any_code", UpToDateCheckingMethod.FileContent);

                    result.Should().Be(true);
                }
            }
        }

        [Fact]
        public void Should_detect_up_to_date_test_file_based_on_content_compare_from_provided_content()
        {
            using (var tempFile = new TempFile(".feature"))
            {
                tempFile.SetContent("any");

                using (var tempTestFile = new TempFile(".cs"))
                {
                    // set test content
                    tempTestFile.SetContent("any_old_code");

                    var testUpToDateChecker = CreateUpToDateChecker();

                    var result = testUpToDateChecker.IsUpToDate(new FeatureFileInput(tempFile.FileName) { GeneratedTestFileContent = "any_code" },
                        tempTestFile.FullPath, "any_code", UpToDateCheckingMethod.FileContent);

                    result.Should().Be(true);
                }
            }
        }

        [Fact]
        public void Should_outdated_test_file_based_on_content_compare_from_file()
        {
            using (var tempFile = new TempFile(".feature"))
            {
                tempFile.SetContent("any");

                using (var tempTestFile = new TempFile(".cs"))
                {
                    // set test content
                    tempTestFile.SetContent("any_code");

                    var testUpToDateChecker = CreateUpToDateChecker();

                    var result = testUpToDateChecker.IsUpToDate(new FeatureFileInput(tempFile.FileName),
                        tempTestFile.FullPath, "new_code", UpToDateCheckingMethod.FileContent);

                    result.Should().Be(false);
                }
            }
        }
    }
}
