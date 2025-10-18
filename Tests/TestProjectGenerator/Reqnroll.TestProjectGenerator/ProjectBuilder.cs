using System;
using System.Collections.Generic;
using System.IO;
using Reqnroll.TestProjectGenerator.ConfigurationModel;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator.Factories.BindingsGenerator;
using Reqnroll.TestProjectGenerator.Factories.ConfigurationGenerator;

namespace Reqnroll.TestProjectGenerator
{
    public class ProjectBuilder
    {
        public const string NUnit3PackageName = "NUnit";
        public const string NUnit3PackageVersion = "3.13.1";
        public const string NUnit3TestAdapterPackageName = "NUnit3TestAdapter";
        public const string NUnit3TestAdapterPackageVersion = "3.17.0";
        public const string NUnit4PackageName = "NUnit";
        public const string NUnit4PackageVersion = "4.2.2";
        public const string NUnit4TestAdapterPackageName = "NUnit3TestAdapter";
        public const string NUnit4TestAdapterPackageVersion = "4.6.0";
        public const string TUnitPackageName = "TUnit";
        public const string TUnitPackageVersion = "0.55.23";
        private const string XUnitPackageVersion = "2.8.1";
        private const string MSTestPackageVersion = "2.2.10";
        private const string MSTest4PackageVersion = "4.0.0";
        private const string XUnit3PackageVersion = "2.0.0";
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory;
        private readonly ConfigurationGeneratorFactory _configurationGeneratorFactory;
        protected readonly CurrentVersionDriver _currentVersionDriver;
        private readonly FeatureFileGenerator _featureFileGenerator;
        private readonly Folders _folders;
        private readonly TargetFrameworkMonikerStringBuilder _targetFrameworkMonikerStringBuilder;
        protected readonly TestProjectFolders _testProjectFolders;
        private bool _parallelTestExecution;
        private Project _project;

        public ProjectBuilder(
            TestProjectFolders testProjectFolders,
            FeatureFileGenerator featureFileGenerator,
            BindingsGeneratorFactory bindingsGeneratorFactory,
            ConfigurationGeneratorFactory configurationGeneratorFactory,
            Configuration configuration,
            CurrentVersionDriver currentVersionDriver,
            Folders folders,
            TargetFrameworkMonikerStringBuilder targetFrameworkMonikerStringBuilder)
        {
            _testProjectFolders = testProjectFolders;
            _featureFileGenerator = featureFileGenerator;
            _bindingsGeneratorFactory = bindingsGeneratorFactory;
            _configurationGeneratorFactory = configurationGeneratorFactory;
            Configuration = configuration;
            _currentVersionDriver = currentVersionDriver;
            _folders = folders;
            _targetFrameworkMonikerStringBuilder = targetFrameworkMonikerStringBuilder;
            var projectGuidString = $"{ProjectGuid:N}".Substring(24);
            ProjectName = $"TestProj_{projectGuidString}";
        }

        public Guid ProjectGuid { get; } = Guid.NewGuid();
        public Configuration Configuration { get; }
        public string ProjectName { get; set; }
        public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.CSharp;
        public TargetFramework TargetFramework { get; set; } = TargetFramework.Net80;
        public string TargetFrameworkMoniker => _targetFrameworkMonikerStringBuilder.BuildTargetFrameworkMoniker(TargetFramework);
        public ProjectFormat Format { get; set; } = ProjectFormat.New;
        public ConfigurationFormat ConfigurationFormat { get; set; } = ConfigurationFormat.Json;

        public bool IsReqnrollFeatureProject { get; set; } = true;

        public bool? IsTreatWarningsAsErrors { get; set; }

        public ProjectType ProjectType { get; set; } = ProjectType.Library;

        public void AddProjectReference(string projectPath, ProjectBuilder projectToReference)
        {
            EnsureProjectExists();
            _project.AddProjectReference(projectPath, projectToReference);
        }

        public void AddFile(ProjectFile projectFile)
        {
            EnsureProjectExists();
            _project.AddFile(projectFile ?? throw new ArgumentNullException(nameof(projectFile)));
        }

        public ProjectFile AddFeatureFile(string featureFileContent)
        {
            EnsureProjectExists();

            var featureFile = _featureFileGenerator.Generate(featureFileContent);

            _project.AddFile(featureFile);
            return featureFile;
        }

        public void AddStepBinding(string attributeName, string regex, string csharpcode, string vbnetcode)
        {
            EnsureProjectExists();

            var methodImplementation = GetCode(_project.ProgrammingLanguage, csharpcode, vbnetcode);
            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);

            _project.AddFile(bindingsGenerator.GenerateStepDefinition("StepBinding", methodImplementation, attributeName, regex));
            _project.AddFile(bindingsGenerator.GenerateStepDefinition("StepBinding", methodImplementation, attributeName, regex, ParameterType.Table, "tableArg"));
            _project.AddFile(bindingsGenerator.GenerateStepDefinition("StepBinding", methodImplementation, attributeName, regex, ParameterType.DocString, "docStringArg"));
        }

        public void AddLoggingStepBinding(string attributeName, string methodName, string regex)
        {
            EnsureProjectExists();

            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);

            _project.AddFile(bindingsGenerator.GenerateLoggingStepDefinition(methodName, attributeName, regex));
            _project.AddFile(bindingsGenerator.GenerateLoggingStepDefinition(methodName, attributeName, regex, ParameterType.Table, "tableArg"));
            _project.AddFile(bindingsGenerator.GenerateLoggingStepDefinition(methodName, attributeName, regex, ParameterType.DocString, "docStringArg"));
        }

        public void AddHookBinding(string eventType, string name, string code = "", bool? asyncHook = null, int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null,
            IList<string> classScopeAttributeTags = null)
        {
            EnsureProjectExists();

            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);
            _project.AddFile(bindingsGenerator.GenerateHookBinding(eventType, name, code, asyncHook, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags));
        }

        public void AddStepBinding(string bindingCode)
        {
            EnsureProjectExists();

            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);
            _project.AddFile(bindingsGenerator.GenerateStepDefinition(bindingCode));
        }

        public void AddBindingClass(string fullBindingClass)
        {
            EnsureProjectExists();

            var bindingsGenerator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);
            var replacedBindingClass = fullBindingClass.Replace("$ProjectDir$", _testProjectFolders.ProjectFolder);
            _project.AddFile(bindingsGenerator.GenerateBindingClassFile(replacedBindingClass));
        }

        public void GenerateReqnrollConfigurationFile()
        {
            if (!IsReqnrollFeatureProject)
            {
                return;
            }

            EnsureProjectExists();
            var generator = _configurationGeneratorFactory.FromConfigurationFormat(ConfigurationFormat);
            var generatedConfig = generator.Generate(Configuration);

            if (generatedConfig is ProjectFile _)
            {
                _project.AddFile(generatedConfig);
            }
        }

        public Project Build()
        {
            EnsureProjectExists();
            return _project;
        }

        private string GetCode(ProgrammingLanguage language, string csharpcode, string vbnetcode)
        {
            switch (language)
            {
                case ProgrammingLanguage.CSharp73:
                case ProgrammingLanguage.CSharp:
                    return csharpcode;
                case ProgrammingLanguage.VB:
                    return vbnetcode;
                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, null);
            }
        }

        private void AddInitialFSharpReferences()
        {
            switch (_project.ProjectFormat)
            {
                case ProjectFormat.Old:
                    AddInitialOldFormatFSharpReferences();
                    break;
                case ProjectFormat.New:
                    AddInitialNewFormatFSharpReferences();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void AddInitialOldFormatFSharpReferences()
        {
            _project.AddNuGetPackage("FSharp.Compiler.Tools", "10.2.1");
            _project.AddNuGetPackage("FSharp.Core", "4.6.2", new NuGetPackageAssembly("FSharp.Core, Version=4.6.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "net45\\FSharp.Core.dll"));
            _project.AddReference("System.Numerics");
        }

        private void AddInitialNewFormatFSharpReferences()
        {
        }

        private void EnsureProjectExists()
        {
            if (_project != null) return;

            _project = new Project(ProjectName, ProjectGuid, Language, TargetFramework, Format, ProjectType);

            _testProjectFolders.PathToNuGetPackages = _project.ProjectFormat == ProjectFormat.Old ? Path.Combine(_testProjectFolders.PathToSolutionDirectory, "packages") : _folders.GlobalNuGetPackages;
            if (!Directory.Exists(_testProjectFolders.PathToNuGetPackages))
            {
                Directory.CreateDirectory(_testProjectFolders.PathToNuGetPackages);
            }

            if (ProjectType is ProjectType.Library or ProjectType.Exe)
            {
                _testProjectFolders.ProjectFolder = Path.Combine(_testProjectFolders.PathToSolutionDirectory, _project.Name);
                _testProjectFolders.ProjectBinOutputPath = Path.Combine(_testProjectFolders.ProjectFolder, GetProjectCompilePath(_project));

                _testProjectFolders.TestAssemblyFileName = $"{_project.Name}.dll";
                _testProjectFolders.CompiledAssemblyPath = Path.Combine(_testProjectFolders.ProjectBinOutputPath, _testProjectFolders.TestAssemblyFileName);


                _project.AddNuGetPackage("Microsoft.NET.Test.Sdk", "17.12.0");

                if (_project.ProjectFormat == ProjectFormat.Old)
                {
                    _project.AddNuGetPackage("System.Runtime.CompilerServices.Unsafe", "6.0.0", new NuGetPackageAssembly("System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "netstandard2.0\\System.Runtime.CompilerServices.Unsafe.dll"));
                }

                _project.AddNuGetPackage("Reqnroll", _currentVersionDriver.ReqnrollNuGetVersion, new NuGetPackageAssembly("Reqnroll", "net462\\Reqnroll.dll"));

                var generator = _bindingsGeneratorFactory.FromLanguage(_project.ProgrammingLanguage);
                _project.AddFile(generator.GenerateLoggerClass(_testProjectFolders.LogFilePath));

                switch (_project.ProgrammingLanguage)
                {
                    case ProgrammingLanguage.FSharp:
                        AddInitialFSharpReferences();
                        break;
                    case ProgrammingLanguage.CSharp73:
                    case ProgrammingLanguage.CSharp:
                        AddUnitTestProviderSpecificConfig();
                        break;
                }

                switch (Configuration.UnitTestProvider)
                {
                    case UnitTestProvider.MSTest:
                        ConfigureMSTest();
                        break;
                    case UnitTestProvider.MSTest4:
                        ConfigureMSTest4();
                        break;
                    case UnitTestProvider.xUnit:
                        ConfigureXUnit();
                        break;
                    case UnitTestProvider.xUnit3:
                        ConfigureXUnit3();
                        break;
                    case UnitTestProvider.NUnit3:
                        ConfigureNUnit3();
                        break;
                    case UnitTestProvider.NUnit4:
                        ConfigureNUnit4();
                        break;
                    case UnitTestProvider.TUnit:
                        ConfigureTUnit();
                        break;
                    default:
                        throw new InvalidOperationException(@"Invalid unit test provider.");
                }
            }

            AddAdditionalStuff();
        }

        private void ConfigureNUnit3()
        {
            _project.AddNuGetPackage(NUnit3PackageName, NUnit3PackageVersion);
            _project.AddNuGetPackage(NUnit3TestAdapterPackageName, NUnit3TestAdapterPackageVersion);


            if (IsReqnrollFeatureProject)
            {
                _project.AddNuGetPackage("Reqnroll.NUnit", _currentVersionDriver.ReqnrollNuGetVersion,
                    new NuGetPackageAssembly(GetReqnrollPublicAssemblyName("Reqnroll.NUnit.ReqnrollPlugin.dll"), "net462\\Reqnroll.NUnit.ReqnrollPlugin.dll"));
                Configuration.Plugins.Add(new ReqnrollPlugin("Reqnroll.NUnit", ReqnrollPluginType.Runtime));
            }
        }

        private void ConfigureNUnit4()
        {
            _project.AddNuGetPackage(NUnit4PackageName, NUnit4PackageVersion);
            _project.AddNuGetPackage(NUnit4TestAdapterPackageName, NUnit4TestAdapterPackageVersion);


            if (IsReqnrollFeatureProject)
            {
                _project.AddNuGetPackage("Reqnroll.NUnit", _currentVersionDriver.ReqnrollNuGetVersion,
                    new NuGetPackageAssembly(GetReqnrollPublicAssemblyName("Reqnroll.NUnit.ReqnrollPlugin.dll"), "net462\\Reqnroll.NUnit.ReqnrollPlugin.dll"));
                Configuration.Plugins.Add(new ReqnrollPlugin("Reqnroll.NUnit", ReqnrollPluginType.Runtime));
            }
        }

        private void ConfigureXUnit()
        {
            if (_project.ProjectFormat == ProjectFormat.New)
            {
                _project.AddNuGetPackage("xunit", XUnitPackageVersion);
            }
            else
            {
                _project.AddNuGetPackage("xunit.core", XUnitPackageVersion);
                _project.AddNuGetPackage("xunit.extensibility.core", XUnitPackageVersion,
                    new NuGetPackageAssembly($"xunit.core, Version={XUnitPackageVersion}.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "net452\\xunit.core.dll"));
                _project.AddNuGetPackage("xunit.extensibility.execution", XUnitPackageVersion,
                    new NuGetPackageAssembly($"xunit.execution.desktop, Version={XUnitPackageVersion}.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "net452\\xunit.execution.desktop.dll"));
                _project.AddNuGetPackage("xunit.assert", XUnitPackageVersion,
                    new NuGetPackageAssembly($"xunit.assert, Version={XUnitPackageVersion}.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.1\\xunit.assert.dll"));
                _project.AddNuGetPackage("xunit.abstractions", "2.0.3",
                    new NuGetPackageAssembly("xunit.abstractions, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c", "netstandard1.0\\xunit.abstractions.dll"));
            }

            _project.AddNuGetPackage("xunit.runner.visualstudio", XUnitPackageVersion);
            _project.AddNuGetPackage("Xunit.SkippableFact", "1.4.13", new NuGetPackageAssembly("Xunit.SkippableFact, Version=1.4.0.0, Culture=neutral, PublicKeyToken=b2b52da82b58eb73", "net452\\Xunit.SkippableFact.dll"));

            if (_project.ProjectFormat == ProjectFormat.Old)
            {
                _project.AddNuGetPackage("Validation", "2.4.18", new NuGetPackageAssembly("Validation, Version=2.4.0.0, Culture=neutral, PublicKeyToken=2fc06f0d701809a7", "net45\\Validation.dll"));
            }

            if (IsReqnrollFeatureProject)
            {
                _project.AddNuGetPackage("Reqnroll.xUnit", _currentVersionDriver.ReqnrollNuGetVersion,
                    new NuGetPackageAssembly(GetReqnrollPublicAssemblyName("Reqnroll.xUnit.ReqnrollPlugin.dll"), "net462\\Reqnroll.xUnit.ReqnrollPlugin.dll"));
                Configuration.Plugins.Add(new ReqnrollPlugin("Reqnroll.xUnit", ReqnrollPluginType.Runtime));
            }
        }

        private void ConfigureXUnit3()
        {
            _project.AddNuGetPackage("xunit.v3", XUnit3PackageVersion);
            _project.AddNuGetPackage("xunit.runner.visualstudio", "3.0.2");
            if (IsReqnrollFeatureProject)
            {
                _project.AddNuGetPackage("Reqnroll.xunit.v3", _currentVersionDriver.ReqnrollNuGetVersion,
                                         new NuGetPackageAssembly(GetReqnrollPublicAssemblyName("Reqnroll.xUnit3.ReqnrollPlugin.dll"), "net462\\Reqnroll.xUnit3.ReqnrollPlugin.dll"));
                Configuration.Plugins.Add(new ReqnrollPlugin("Reqnroll.xunit.v3", ReqnrollPluginType.Runtime));
            }
        }

        private void ConfigureMSTest()
        {
            _project.AddNuGetPackage("MSTest.TestAdapter", MSTestPackageVersion);
            _project.AddNuGetPackage(
                "MSTest.TestFramework",
                MSTestPackageVersion,
                new NuGetPackageAssembly(
                    "Microsoft.VisualStudio.TestPlatform.TestFramework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL",
                    "net45\\Microsoft.VisualStudio.TestPlatform.TestFramework.dll"),
                new NuGetPackageAssembly(
                    "Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL",
                    "net45\\Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll"));

            if (IsReqnrollFeatureProject)
            {
                _project.AddNuGetPackage("Reqnroll.MSTest", _currentVersionDriver.ReqnrollNuGetVersion,
                    new NuGetPackageAssembly(GetReqnrollPublicAssemblyName("Reqnroll.MSTest.ReqnrollPlugin.dll"), "net462\\Reqnroll.MSTest.ReqnrollPlugin.dll"));
                Configuration.Plugins.Add(new ReqnrollPlugin("Reqnroll.MSTest", ReqnrollPluginType.Runtime));
            }
        }

        private void ConfigureMSTest4()
        {
            _project.AddNuGetPackage("MSTest.TestAdapter", MSTest4PackageVersion);
            _project.AddNuGetPackage("MSTest.TestFramework", MSTest4PackageVersion);

            if (IsReqnrollFeatureProject)
            {
                _project.AddNuGetPackage("Reqnroll.MSTest", _currentVersionDriver.ReqnrollNuGetVersion,
                                         new NuGetPackageAssembly(GetReqnrollPublicAssemblyName("Reqnroll.MSTest.ReqnrollPlugin.dll"), "net462\\Reqnroll.MSTest.ReqnrollPlugin.dll"));
                Configuration.Plugins.Add(new ReqnrollPlugin("Reqnroll.MSTest", ReqnrollPluginType.Runtime));
            }
        }

        private void ConfigureTUnit()
        {
            _project.AddNuGetPackage(
                TUnitPackageName,
                TUnitPackageVersion,
                new NuGetPackageAssembly(
                    "TUnit, Version=0.55.23, Culture=neutral, PublicKeyToken=b8d4030011dbd70c",
                    "netstandard2.0\\TUnit.dll")
                );
            _project.AddNuGetPackage(
                "Microsoft.Testing.Extensions.TrxReport",
                "1.8.2.0",
                new NuGetPackageAssembly(
                    "Microsoft.Testing.Extensions.TrxReport, Version=1.8.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    "netstandard2.0\\Microsoft.Testing.Extensions.TrxReport.dll")
                );

            if (IsReqnrollFeatureProject)
            {
                _project.AddNuGetPackage("Reqnroll.TUnit", _currentVersionDriver.ReqnrollNuGetVersion,
                                         new NuGetPackageAssembly(GetReqnrollPublicAssemblyName("Reqnroll.TUnit.ReqnrollPlugin.dll"), "netstandard2.0\\Reqnroll.TUnit.ReqnrollPlugin.dll"));
                Configuration.Plugins.Add(new ReqnrollPlugin("Reqnroll.TUnit", ReqnrollPluginType.Runtime));
            }
        }

        protected virtual void AddAdditionalStuff()
        {
        }

        private void AddUnitTestProviderSpecificConfig()
        {
            switch (Configuration.UnitTestProvider)
            {
                case UnitTestProvider.xUnit when !_parallelTestExecution:
                case UnitTestProvider.xUnit3 when !_parallelTestExecution:
                    _project.AddFile(new ProjectFile("XUnitConfiguration.cs", "Compile", "using Xunit; [assembly: CollectionBehavior(MaxParallelThreads = 1, DisableTestParallelization = true)]"));
                    break;
                case UnitTestProvider.xUnit:
                case UnitTestProvider.xUnit3:
                    _project.AddFile(new ProjectFile("XUnitConfiguration.cs", "Compile", "using Xunit; [assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, MaxParallelThreads = 4)]"));
                    break;
                case UnitTestProvider.NUnit3 when _parallelTestExecution:
                case UnitTestProvider.NUnit4 when _parallelTestExecution:
                    _project.AddFile(new ProjectFile("NUnitConfiguration.cs", "Compile", "[assembly: NUnit.Framework.Parallelizable(NUnit.Framework.ParallelScope.All)]"));
                    break;
                case UnitTestProvider.MSTest when _parallelTestExecution:
                case UnitTestProvider.MSTest4 when _parallelTestExecution:
                    _project.AddFile(
                        new ProjectFile("MsTestConfiguration.cs", "Compile", "using Microsoft.VisualStudio.TestTools.UnitTesting; [assembly: Parallelize(Workers = 4, Scope = ExecutionScope.MethodLevel)]"));
                    break;
                case UnitTestProvider.MSTest when !_parallelTestExecution:
                case UnitTestProvider.MSTest4 when !_parallelTestExecution:
                    _project.AddFile(new ProjectFile("MsTestConfiguration.cs", "Compile", "using Microsoft.VisualStudio.TestTools.UnitTesting; [assembly: DoNotParallelize]"));
                    break;
                case UnitTestProvider.TUnit when !_parallelTestExecution:
                    _project.AddFile(new ProjectFile("TUnitConfiguration.cs", "Compile", "using TUnit.Core; [assembly: NotInParallel]"));
                    break;
            }
        }

        private string GetReqnrollPublicAssemblyName(string assemblyName)
        {
#if REQNROLL_ENABLE_STRONG_NAME_SIGNING
            return $"{Path.GetFileNameWithoutExtension(assemblyName)}, Version={_currentVersionDriver.ReqnrollVersion}, Culture=neutral, PublicKeyToken=0778194805d6db41, processorArchitecture=MSIL";
#else
            return Path.GetFileNameWithoutExtension(assemblyName);
#endif
        }

        public void EnableParallelTestExecution()
        {
            _parallelTestExecution = true;
        }

        private string GetProjectCompilePath(Project project)
        {
            // TODO: hardcoded "Debug" value should be replaced by a configuration parameter
            if (project.ProjectFormat == ProjectFormat.New)
            {
                return Path.Combine("bin", "Debug", _targetFrameworkMonikerStringBuilder.BuildTargetFrameworkMoniker(project.TargetFrameworks).Split(';')[0]);
            }

            return Path.Combine("bin", "Debug");
        }

        public void AddMSBuildTarget(string targetName, string implementation)
        {
            _project.AddTarget(targetName, implementation);
        }

        public void AddNuGetPackage(string nugetPackage, string nugetVersion = null)
        {
            EnsureProjectExists();
            _project.AddNuGetPackage(nugetPackage, nugetVersion);
        }

        public void UpdateNuGetPackage(string nugetPackage, string nugetVersion)
        {
            EnsureProjectExists();
            _project.RemoveNuGetPackage(nugetPackage);
            _project.AddNuGetPackage(nugetPackage, nugetVersion);
        }
    }
}
