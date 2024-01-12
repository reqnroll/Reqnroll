using System;
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
    }
}