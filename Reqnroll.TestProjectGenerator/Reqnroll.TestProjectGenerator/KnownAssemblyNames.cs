using System;
using System.Collections.Generic;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator
{
    class KnownAssemblyNames
    {
        private static readonly Lazy<Dictionary<string, Dictionary<string, NuGetPackageAssembly>>> _assemblies = new Lazy<Dictionary<string, Dictionary<string, NuGetPackageAssembly>>>(() =>
        {
            return new Dictionary<string, Dictionary<string, NuGetPackageAssembly>>()
            {
                {
                    "Newtonsoft.Json", new Dictionary<string, NuGetPackageAssembly>()
                    {
                        {"default", new NuGetPackageAssembly("Newtonsoft.Json, Version={0}, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL", "net45\\Newtonsoft.Json.dll")},
                        {"10.0.3", new NuGetPackageAssembly("Newtonsoft.Json, Version=10.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL", "net45\\Newtonsoft.Json.dll")},
                        {"11.0.2", new NuGetPackageAssembly("Newtonsoft.Json, Version=11.0.0.0}, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL", "net45\\Newtonsoft.Json.dll")},
                    }
                },
                {
                    "FluentAssertions", new Dictionary<string, NuGetPackageAssembly>()
                    {
                        {"default", new NuGetPackageAssembly("FluentAssertions, Version={0}.0, Culture=neutral, PublicKeyToken=33f2691a05b67b6a", @"net45\FluentAssertions.dll")},
                    }
                },
                {
                    "NUnit", new Dictionary<string, NuGetPackageAssembly>()
                    {
                        {"default", new NuGetPackageAssembly("nunit.framework, Version={0}.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL", "net45\\nunit.framework.dll")},
                        {"2.6.4", new NuGetPackageAssembly("nunit.framework", "nunit.framework.dll")},
                        {"3.0.0", new NuGetPackageAssembly("nunit.framework", "net45\\nunit.framework.dll")},
                        {"3.0.1", new NuGetPackageAssembly("nunit.framework", "net45\\nunit.framework.dll")},
                    }
                }
            };
        });

        protected static Dictionary<string, Dictionary<string, NuGetPackageAssembly>> Assemblies = _assemblies.Value;


        public static NuGetPackageAssembly Get(string nugetPackageName, string version)
        {
            if (Assemblies.ContainsKey(nugetPackageName))
            {
                var packageAssembly = Assemblies[nugetPackageName];

                NuGetPackageAssembly nugetPackageAssembly;
                if (packageAssembly.ContainsKey(version))
                {
                    nugetPackageAssembly = packageAssembly[version];
                }
                else
                {
                    nugetPackageAssembly = packageAssembly["default"];
                }

                return new NuGetPackageAssembly(GetPublicAssemblyName(version, nugetPackageAssembly), nugetPackageAssembly.RelativeHintPath);
            }

            return null;
        }

        private static string GetPublicAssemblyName(string version, NuGetPackageAssembly nugetPackageAssembly)
        {
            if (nugetPackageAssembly.PublicAssemblyName.Contains("{0}"))
                return string.Format(nugetPackageAssembly.PublicAssemblyName, version);
            return nugetPackageAssembly.PublicAssemblyName;
        }
    }
}