using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public static class AssemblyFolderHelper
    {
        public static string GetAssemblyFolder()
        {
            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);
            Debug.Assert(assemblyFolder != null);
            return assemblyFolder;
        }
    }
}