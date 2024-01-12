using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Reqnroll.Infrastructure;

namespace Reqnroll.Plugins
{
    internal sealed class RuntimePluginLocator : IRuntimePluginLocator
    {
        private readonly IRuntimePluginLocationMerger _runtimePluginLocationMerger;
        private readonly IReqnrollPath _reqnrollPath;
        private readonly Assembly _testAssembly;

        public RuntimePluginLocator(IRuntimePluginLocationMerger runtimePluginLocationMerger, IReqnrollPath reqnrollPath, ITestAssemblyProvider testAssemblyProvider)
        {
            _runtimePluginLocationMerger = runtimePluginLocationMerger;
            _reqnrollPath = reqnrollPath;
            _testAssembly = testAssemblyProvider.TestAssembly;
        }

        public IReadOnlyList<string> GetAllRuntimePlugins()
        {
            var allRuntimePlugins = new List<string>();

            var currentDirectory = Environment.CurrentDirectory;
            allRuntimePlugins.AddRange(SearchPluginsInFolder(currentDirectory));

            // Check to not search the same directory twice
            var reqnrollAssemblyDirectory = Path.GetDirectoryName(Path.GetFullPath(_reqnrollPath.GetPathToReqnrollDll()));
            if (currentDirectory != reqnrollAssemblyDirectory)
            {
                allRuntimePlugins.AddRange(SearchPluginsInFolder(reqnrollAssemblyDirectory));
            }

            var assemblyLocation = _testAssembly != null && !_testAssembly.IsDynamic ? _testAssembly.Location : null;
            if (assemblyLocation.IsNotNullOrWhiteSpace() && !allRuntimePlugins.Contains(assemblyLocation))
            {
                allRuntimePlugins.Add(assemblyLocation);

                // Check to not search the same directory twice
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                if (currentDirectory != assemblyDirectory && reqnrollAssemblyDirectory != assemblyDirectory)
                {
                    allRuntimePlugins.AddRange(SearchPluginsInFolder(assemblyDirectory));
                }
            }

            return _runtimePluginLocationMerger.Merge(allRuntimePlugins);
        }

        private static IEnumerable<string> SearchPluginsInFolder(string folder)
        {
            return Directory.EnumerateFiles(folder, "*.ReqnrollPlugin.dll", SearchOption.TopDirectoryOnly);
        }
    }
}