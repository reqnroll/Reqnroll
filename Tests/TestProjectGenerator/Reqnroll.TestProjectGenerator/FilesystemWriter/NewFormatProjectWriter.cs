using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Dotnet;
using Reqnroll.TestProjectGenerator.Extensions;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public class NewFormatProjectWriter : IProjectWriter
    {
        private readonly IOutputWriter _outputWriter;
        private readonly TargetFrameworkMonikerStringBuilder _targetFrameworkMonikerStringBuilder;

        public NewFormatProjectWriter(IOutputWriter outputWriter, TargetFrameworkMonikerStringBuilder targetFrameworkMonikerStringBuilder)
        {
            _outputWriter = outputWriter;
            _targetFrameworkMonikerStringBuilder = targetFrameworkMonikerStringBuilder;
        }

        public virtual string WriteProject(DotNetSdkInfo sdk, Project project, string projRootPath)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }


            CreateProjectFile(sdk, project, projRootPath);

            string projFileName = $"{project.Name}.{project.ProgrammingLanguage.ToProjectFileExtension()}";

            string projectFilePath = Path.Combine(projRootPath, projFileName);

            var xd = XDocument.Load(projectFilePath);
            var projectElement = xd.Element(XName.Get("Project")) ?? throw new ProjectCreationNotPossibleException($"No 'Project' tag could be found in project file '{projectFilePath}'");

            AdjustForASPNetCore(project, projectElement);
            SetTargetFramework(project, projectElement);
            WriteAssemblyReferences(project, projectElement);
            WriteNuGetPackages(project, projectElement);
            WriteFileReferences(project, projectElement);
            WriteSatelliteResourceLanguages(project, projectElement);

            SetTreatWarningsAsErrors(project, projectElement);

            xd.Save(projectFilePath);

            WriteProjectFiles(project, projRootPath);

            if (project.ProjectType == ProjectType.Exe && project.NuGetPackages.Any(n => n.Name.StartsWith("xunit.v3")))
            {
                string programMainFilePath = Path.Combine(projRootPath, $"Program.{project.ProgrammingLanguage.ToCodeFileExtension()}");
                if (File.Exists(programMainFilePath))
                {
                    File.Delete(programMainFilePath);
                }
            }

            return projectFilePath;
        }

        private void SetTreatWarningsAsErrors(Project project, XElement projectElement)
        {
            if (project.IsTreatWarningsAsErrors is bool treatWarningsAsErrors)
            {
                var propertyGroupElement = projectElement.Element("PropertyGroup") ?? throw new ProjectCreationNotPossibleException();
                var treatWarningsAsErrorsElement = new XElement("TreatWarningsAsErrors");
                treatWarningsAsErrorsElement.SetValue(treatWarningsAsErrors);
                propertyGroupElement.Add(treatWarningsAsErrorsElement);
            }
        }

        private void AdjustForASPNetCore(Project project, XElement projectElement)
        {
            if (project.NuGetPackages.Any(n => n.Name == "Microsoft.AspNetCore.App"))
            {
                projectElement.Attribute("Sdk").SetValue("Microsoft.NET.Sdk.Web");

                var itemGroup = new XElement("ItemGroup");
                using (var xw = itemGroup.CreateWriter())
                {
                    xw.WriteStartElement("Content");
                    xw.WriteAttributeString("Remove", "*.cshtml");
                    xw.WriteEndElement();
                }

                projectElement.Add(itemGroup);
            }
            
        }

        private void WriteFileReferences(Project project, XElement projectElement)
        {
            bool created = false;

            var itemGroup = new XElement("ItemGroup");
            using (var xw = itemGroup.CreateWriter())
            {
                if (project.ProgrammingLanguage == ProgrammingLanguage.FSharp)
                {
                    foreach (var file in project.Files.Where(f => f.BuildAction.ToUpper() == "COMPILE"))
                    {
                        WriteFileReference(xw, file);
                        created = true;
                    }
                }

                foreach (var file in project.Files.Where(f => f.BuildAction.ToUpper() == "CONTENT" || f.BuildAction.ToUpper() == "NONE" && (f.CopyToOutputDirectory != CopyToOutputDirectory.DoNotCopy || f.AdditionalMsBuildProperties.Any())))
                {
                    WriteFileReference(xw, file);
                    created = true;
                }
            }

            if (created)
            {
                projectElement.Add(itemGroup);
            }
        }

        /// <summary>
        /// This avoids the need to copy language-specific files (reduced I/O) and therefore increases test execution time.
        /// </summary>
        private void WriteSatelliteResourceLanguages(Project project, XElement projectElement)
        {
            var propertyGroupElement = projectElement.Element("PropertyGroup") ?? throw new ProjectCreationNotPossibleException();
            var satelliteResourceLanguagesElement = new XElement("SatelliteResourceLanguages");
            satelliteResourceLanguagesElement.SetValue("en");
            propertyGroupElement.Add(satelliteResourceLanguagesElement);
        }

        private void WriteFileReference(XmlWriter xw, ProjectFile projectFile)
        {
            xw.WriteStartElement(projectFile.BuildAction);
            xw.WriteAttributeString("Include", projectFile.Path);

            if (projectFile.CopyToOutputDirectory != CopyToOutputDirectory.DoNotCopy)
            {
                xw.WriteElementString("CopyToOutputDirectory", projectFile.CopyToOutputDirectory.GetCopyToOutputDirectoryString());
            }

            foreach (var additionalMsBuildProperty in projectFile.AdditionalMsBuildProperties)
            {
                xw.WriteElementString(additionalMsBuildProperty.Key, additionalMsBuildProperty.Value);
            }

            xw.WriteEndElement();
        }

        public void WriteReferences(Project project, string projectFilePath)
        {
            WriteProjectReferences(project, projectFilePath);
        }

        private void WriteNuGetPackages(Project project, XElement projectElement)
        {
            if (!project.NuGetPackages.Any())
            {
                return;
            }

            var newNode = new XElement("ItemGroup");

            using (var xw = newNode.CreateWriter())
            {
                foreach (var nugetPackage in project.NuGetPackages)
                {
                    WritePackageReference(xw, nugetPackage);
                }
            }

            projectElement.Add(newNode);
        }

        private void WritePackageReference(XmlWriter xw, NuGetPackage nuGetPackage)
        {
            xw.WriteStartElement("PackageReference");
            xw.WriteAttributeString("Include", nuGetPackage.Name);

            if (nuGetPackage.Version.IsNotNullOrWhiteSpace())
            {
                xw.WriteAttributeString("Version", nuGetPackage.Version);
            }

            xw.WriteEndElement();
        }

        private void WriteProjectReferences(Project project, string projFilePath)
        {
            if (project.ProjectReferences.Count > 0)
            {
                var reference = DotNet.Add(_outputWriter)
                                      .Reference();
                foreach (var projReference in project.ProjectReferences)
                {
                    reference.ReferencingProject(projReference.Path);
                }

                reference.ToProject(projFilePath)
                         .Build()
                         .Execute(innerException => new ProjectCreationNotPossibleException($"Writing ProjectRefences failed.", innerException));
            }
        }

        private void WriteProjectFiles(Project project, string projRootPath)
        {
            var fileWriter = new FileWriter();
            foreach (var file in project.Files)
            {
                fileWriter.Write(file, projRootPath);
            }
        }

        private void SetTargetFramework(Project project, XElement projectElement)
        {
            var targetFrameworkElement = projectElement.Element("PropertyGroup")?.Element("TargetFramework") ?? throw new ProjectCreationNotPossibleException();

            string newTargetFrameworks = _targetFrameworkMonikerStringBuilder.BuildTargetFrameworkMoniker(project.TargetFrameworks);
            targetFrameworkElement.SetValue(newTargetFrameworks);
        }

        private void WriteAssemblyReferences(Project project, XElement projectElement)
        {
            if (!project.References.Any())
            {
                return;
            }

            // GAC and library references cannot be added in new Csproj format (via dotnet CLI)
            // see https://github.com/dotnet/sdk/issues/987
            // Therefore, write them manually into the project file
            var itemGroup = new XElement("ItemGroup");

            using (var xw = itemGroup.CreateWriter())
            {
                foreach (var reference in project.References)
                {
                    WriteAssemblyReference(xw, reference);
                }
            }

            projectElement.Add(itemGroup);
        }

        private void WriteAssemblyReference(XmlWriter xw, Reference reference)
        {
            xw.WriteStartElement("Reference");
            xw.WriteAttributeString("Include", reference.Name);
            xw.WriteEndElement();
        }

        private void CreateProjectFile(DotNetSdkInfo sdk, Project project, string projRootPath)
        {
            string template;

            switch (project.ProjectType)
            {
                case ProjectType.Library:
                    template = "classlib";
                    break;
                case ProjectType.Exe:
                    template = "console";
                    break;
                case ProjectType.ASPNetCore:
                    template = "web";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(project.ProjectType), $"ProjectType {project.ProjectType} is not supported");
            }

            

            var newProjCommand = DotNet.New(_outputWriter, sdk)
                                       .Project()
                                       .InFolder(projRootPath)
                                       .WithName(project.Name)
                                       .UsingTemplate(template)
                                       .WithLanguage(project.ProgrammingLanguage)
                                       .Build();

            newProjCommand.Execute(innerExceptions => new ProjectCreationNotPossibleException("Execution of dotnet new failed.", innerExceptions));
        }
    }
}
