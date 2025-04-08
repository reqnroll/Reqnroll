using System.Runtime.InteropServices;
using FluentAssertions;
using Reqnroll.Plugins;
using Xunit;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    public class RuntimePluginLocationMergerTests
    {
        [Fact]
        public void Merge_EmptyList_EmptyList()
        {
            //ARRANGE
            var runtimePluginLocationMerger = new RuntimePluginLocationMerger();


            //ACT
            var result = runtimePluginLocationMerger.Merge(new string[] { });


            //ASSERT
            result.Should().HaveCount(0);
        }

        [SkippableFact]
        public void Merge_SingleEntry_ThisIsReturned_Windows()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

            //ARRANGE
            var runtimePluginLocationMerger = new RuntimePluginLocationMerger();


            //ACT
            var result = runtimePluginLocationMerger.Merge(new string[]{ "C:\\temp\\Plugin.ReqnrollPlugin.dll" } );


            //ASSERT
            result.Should().Contain("C:\\temp\\Plugin.ReqnrollPlugin.dll");
            result.Should().HaveCount(1);
        }

        [SkippableFact]
        public void Merge_SingleEntry_ThisIsReturned_Unix()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||RuntimeInformation.IsOSPlatform(OSPlatform.OSX));

            //ARRANGE
            var runtimePluginLocationMerger = new RuntimePluginLocationMerger();


            //ACT
            var result = runtimePluginLocationMerger.Merge(new string[] { "/temp/Plugin.ReqnrollPlugin.dll" });


            //ASSERT
            result.Should().Contain("/temp/Plugin.ReqnrollPlugin.dll");
            result.Should().HaveCount(1);
        }


        [SkippableFact]
        public void Merge_SamePluginDifferentPath_FirstEntryIsReturned_Windows()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

            //ARRANGE
            var runtimePluginLocationMerger = new RuntimePluginLocationMerger();


            //ACT
            var result = runtimePluginLocationMerger.Merge(new string[] { "C:\\temp\\Plugin.ReqnrollPlugin.dll", "C:\\anotherFolder\\Plugin.ReqnrollPlugin.dll" });

            //ASSERT
            result.Should().Contain("C:\\temp\\Plugin.ReqnrollPlugin.dll");
            result.Should().HaveCount(1);
        }

        [SkippableFact]
        public void Merge_SamePluginDifferentPath_FirstEntryIsReturned_Unix()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX));

            //ARRANGE
            var runtimePluginLocationMerger = new RuntimePluginLocationMerger();


            //ACT
            var result = runtimePluginLocationMerger.Merge(new string[] { "/temp/Plugin.ReqnrollPlugin.dll", "/anotherFolder/Plugin.ReqnrollPlugin.dll" });

            //ASSERT
            result.Should().Contain("/temp/Plugin.ReqnrollPlugin.dll");
            result.Should().HaveCount(1);
        }

        [SkippableFact]
        public void Merge_DifferendPluginSamePath_BothAreReturned_Windows()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

            //ARRANGE
            var runtimePluginLocationMerger = new RuntimePluginLocationMerger();


            //ACT
            var result = runtimePluginLocationMerger.Merge(new string[] { "C:\\temp\\Plugin.ReqnrollPlugin.dll", "C:\\temp\\AnotherPlugin.ReqnrollPlugin.dll" });


            //ASSERT
            result.Should().Contain("C:\\temp\\Plugin.ReqnrollPlugin.dll");
            result.Should().Contain("C:\\temp\\AnotherPlugin.ReqnrollPlugin.dll");
            result.Should().HaveCount(2);
        }

        [SkippableFact]
        public void Merge_DifferendPluginSamePath_BothAreReturned_Unix()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX));

            //ARRANGE
            var runtimePluginLocationMerger = new RuntimePluginLocationMerger();


            //ACT
            var result = runtimePluginLocationMerger.Merge(new string[] { "/temp/Plugin.ReqnrollPlugin.dll", "/temp/AnotherPlugin.ReqnrollPlugin.dll" });


            //ASSERT
            result.Should().Contain("/temp/Plugin.ReqnrollPlugin.dll");
            result.Should().Contain("/temp/AnotherPlugin.ReqnrollPlugin.dll");
            result.Should().HaveCount(2);
        }

        [SkippableFact]
        public void Merge_UnsortedListOfPlugins_SortedListIsReturned_Windows()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

            //ARRANGE
            var runtimePluginLocationMerger = new RuntimePluginLocationMerger();


            //ACT
            var result = runtimePluginLocationMerger.Merge(
            [
                "C:\\temp1\\B.ReqnrollPlugin.dll",
                "C:\\temp2\\D.ReqnrollPlugin.dll",
                "C:\\temp\\AA.ReqnrollPlugin.dll",
                "C:\\temp1\\Z.ReqnrollPlugin.dll",
                "C:\\temp2\\A.ReqnrollPlugin.dll",
                "C:\\temp\\C.ReqnrollPlugin.dll",
            ]);


            //ASSERT
            result.Should().HaveCount(6);
            result[0].Should().Be("C:\\temp2\\A.ReqnrollPlugin.dll");
            result[1].Should().Be("C:\\temp\\AA.ReqnrollPlugin.dll");
            result[2].Should().Be("C:\\temp1\\B.ReqnrollPlugin.dll");
            result[3].Should().Be("C:\\temp\\C.ReqnrollPlugin.dll");
            result[4].Should().Be("C:\\temp2\\D.ReqnrollPlugin.dll");
            result[5].Should().Be("C:\\temp1\\Z.ReqnrollPlugin.dll");
        }

        [SkippableFact]
        public void Merge_UnsortedListOfPlugins_SortedListIsReturned_Unix()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX));

            //ARRANGE
            var runtimePluginLocationMerger = new RuntimePluginLocationMerger();


            //ACT
            var result = runtimePluginLocationMerger.Merge(
            [
                "C:/temp1/B.ReqnrollPlugin.dll",
                "C:/temp2/D.ReqnrollPlugin.dll",
                "C:/temp/AA.ReqnrollPlugin.dll",
                "C:/temp1/Z.ReqnrollPlugin.dll",
                "C:/temp2/A.ReqnrollPlugin.dll",
                "C:/temp/C.ReqnrollPlugin.dll",
            ]);


            //ASSERT
            result.Should().HaveCount(6);
            result[0].Should().Be("C:/temp2/A.ReqnrollPlugin.dll");
            result[1].Should().Be("C:/temp/AA.ReqnrollPlugin.dll");
            result[2].Should().Be("C:/temp1/B.ReqnrollPlugin.dll");
            result[3].Should().Be("C:/temp/C.ReqnrollPlugin.dll");
            result[4].Should().Be("C:/temp2/D.ReqnrollPlugin.dll");
            result[5].Should().Be("C:/temp1/Z.ReqnrollPlugin.dll");
        }
    }
}