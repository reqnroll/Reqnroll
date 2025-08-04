using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.RegularExpressions;
using Reqnroll.TestProjectGenerator.Conventions;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.TestProjectGenerator.Cli
{
    class Program
    {
        private const string DefaultReqnrollNuGetVersion = "3.4.3";
        private const UnitTestProvider DefaultUnitTestProvider = UnitTestProvider.MSTest;
        private const TargetFramework DefaultTargetFramework = TargetFramework.Net80;
        private const ProjectFormat DefaultProjectFormat = ProjectFormat.New;
        private const ConfigurationFormat DefaultConfigurationFormat = ConfigurationFormat.Json;

        static int Main(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new Option<DirectoryInfo>(
                    "--out-dir",
                    "The root directory of the code generation output. Default: current directory."),
                new Option<string>(
                    "--sln-name",
                    "The name of the generated solution (directory and sln file). Default: random string."),
                new Option<string>(
                    "--reqnroll-nuget-version",
                    () => DefaultReqnrollNuGetVersion,
                    $"The Reqnroll NuGet version referenced in the generated project. Default: '{DefaultReqnrollNuGetVersion}'."),
                new Option<UnitTestProvider>(
                    "--unit-test-provider",
                    () => DefaultUnitTestProvider,
                    $"The unit test provider used in the generated project. Default: '{DefaultUnitTestProvider}'."),
                new Option<TargetFramework>(
                    "--target-framework",
                    () => DefaultTargetFramework,
                    $"The target framework of the generated project. Default: '{DefaultTargetFramework}'."),
                new Option<string>(
                    "--sdk-version",
                    $"The sdk version used in the global.json of the generated project. Default: based on a default mapping from the target framework."),
                new Option<ProjectFormat>(
                    "--project-format",
                    () => DefaultProjectFormat,
                    $"The project format of the generated project file. Default: '{DefaultProjectFormat}'."),
                new Option<ConfigurationFormat>(
                    "--configuration-format",
                    () => DefaultConfigurationFormat,
                    $"The format of the generated Reqnroll configuration file. Default: '{DefaultConfigurationFormat}'."),
                new Option<int>(
                    "--feature-count",
                    () => 1,
                    $"Number of feature files to generate. Default: 1"),
            };

            rootCommand.Description = "Reqnroll Test Project Generator";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<GenerateSolutionParams>(
                (generateSolutionParams) =>
                {
                    var services = ConfigureServices();

                    services.AddSingleton(_ => new SolutionConfiguration
                    {
                        OutDir = generateSolutionParams.OutDir,
                        SolutionName = generateSolutionParams.SlnName
                    });

                    services.AddSingleton(_ => new TestRunConfiguration
                    {
                        ProgrammingLanguage = ProgrammingLanguage.CSharp,
                        UnitTestProvider = generateSolutionParams.UnitTestProvider,
                        ConfigurationFormat = generateSolutionParams.ConfigurationFormat,
                        ProjectFormat = generateSolutionParams.ProjectFormat,
                        TargetFramework = generateSolutionParams.TargetFramework,
                    });

                    Version GetReqnrollVersion(string reqnrollNuGetVersion)
                    {
                        var m = new Regex("^(\\d+)\\.(\\d+)").Match(reqnrollNuGetVersion);
                        return new Version(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), 0);
                    }

                    services.AddSingleton(_ => new CurrentVersionDriver
                    {
                        ReqnrollNuGetVersion = generateSolutionParams.ReqnrollNuGetVersion.ToString(),
                        ReqnrollVersion = GetReqnrollVersion(generateSolutionParams.ReqnrollNuGetVersion),
                    });

                    services.AddSingleton(_ => generateSolutionParams);

                    var serviceProvider = services.BuildServiceProvider();

                    SolutionWriteToDiskDriver d = serviceProvider.GetService<SolutionWriteToDiskDriver>();

                    //Create test project
                    var pd = serviceProvider.GetService<ProjectsDriver>();
                    var pb = pd.CreateProject("Proj1", "C#");


                    var projectContentGenerator = serviceProvider.GetService<IProjectContentGenerator>();

                    projectContentGenerator.Generate(pb);

                    var sd = serviceProvider.GetService<SolutionDriver>();

                    //Remove local NuGet source
                    sd.NuGetSources.Clear();

                    sd.SdkVersion = generateSolutionParams.SdkVersion;

                    d.WriteSolutionToDisk();
                });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IOutputWriter, OutputWriter>();
            services.AddSingleton<Folders, FoldersOverride>();
            services.AddSingleton<ArtifactNamingConvention, ArtifactNamingConventionOverride>();

            services.AddSingleton<IProjectContentGenerator, ComplexProjectContentGenerator>();

            services.Scan(scan => scan
                .FromAssemblyOf<SolutionWriteToDiskDriver>()
                    .AddClasses()
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsSelf()
                        .WithScopedLifetime());

            return services;
        }
    }
}
