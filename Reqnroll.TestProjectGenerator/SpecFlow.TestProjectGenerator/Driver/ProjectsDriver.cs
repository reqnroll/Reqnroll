using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Extensions;
using TechTalk.SpecFlow.TestProjectGenerator.Factories;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class ProjectsDriver
    {
        private readonly SolutionDriver _solutionDriver;
        private readonly ProjectBuilderFactory _projectBuilderFactory;
        private readonly TestProjectFolders _testProjectFolders;

        public ProjectsDriver(SolutionDriver solutionDriver, ProjectBuilderFactory projectBuilderFactory, TestProjectFolders testProjectFolders)
        {
            _solutionDriver = solutionDriver;
            _projectBuilderFactory = projectBuilderFactory;
            _testProjectFolders = testProjectFolders;
        }

        public string CreateProject(string language)
        {
            var projectBuilder = _projectBuilderFactory.CreateProject(language);
            _solutionDriver.AddProject(projectBuilder);
            return projectBuilder.ProjectName;
        }

        public ProjectBuilder CreateProject(string projectName, string language)
        {
            var projectBuilder = _projectBuilderFactory.CreateProject(projectName, language);
            _solutionDriver.AddProject(projectBuilder);
            return projectBuilder;
        }

        public ProjectBuilder CreateProject(string projectName, string language, ProjectType projectType)
        {
            var projectBuilder = _projectBuilderFactory.CreateProject(projectName, language);
            projectBuilder.ProjectType = projectType;
            _solutionDriver.AddProject(projectBuilder);
            return projectBuilder;
        }

        public ProjectBuilder CreateSpecFlowProject(string programmingLanguage)
        {
            var projectBuilder = _projectBuilderFactory.CreateProject(programmingLanguage);
            projectBuilder.IsSpecFlowFeatureProject = true;
            _solutionDriver.AddProject(projectBuilder);
            return projectBuilder;
        }

        public void AddHookBinding(string eventType, string name, string hookTypeAttributeTagsString, string methodScopeAttributeTagsString = null, string classScopeAttributeTagsString = null, string code = "", int? order = null)
        {
            var hookTypeAttributeTags = hookTypeAttributeTagsString?.Split(',').Select(t => t.Trim()).ToArray();
            var methodScopeAttributeTags = methodScopeAttributeTagsString?.Split(',').Select(t => t.Trim()).ToArray();
            var classScopeAttributeTags = classScopeAttributeTagsString?.Split(',').Select(t => t.Trim()).ToArray();

            AddHookBinding(_solutionDriver.DefaultProject, eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        public void AddHookBinding(string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null, IList<string> classScopeAttributeTags = null)
        {
            AddHookBinding(_solutionDriver.DefaultProject, eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        private void AddHookBinding(ProjectBuilder project, string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null,  IList<string> classScopeAttributeTags = null)
        {
            project.AddHookBinding(eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        public void AddAsyncHookBindingIncludingLocking(string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null, IList<string> classScopeAttributeTags = null)
        {
            AddAsyncHookBindingIncludingLocking(_solutionDriver.DefaultProject, eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        private void AddAsyncHookBindingIncludingLocking(ProjectBuilder project, string eventType, string name, string code = "", int? order = null, IList<string> hookTypeAttributeTags = null, IList<string> methodScopeAttributeTags = null,  IList<string> classScopeAttributeTags = null)
        {
            project.AddAsyncHookBindingIncludingLocking(eventType, name, code, order, hookTypeAttributeTags, methodScopeAttributeTags, classScopeAttributeTags);
        }

        public void AddFeatureFile(string featureFileContent)
        {
            _solutionDriver.DefaultProject.AddFeatureFile(featureFileContent);
        }

        public void AddStepBinding(string attributeName, string regex, string csharpcode, string vbnetcode)
        {
            _solutionDriver.DefaultProject.AddStepBinding(attributeName, regex, csharpcode, vbnetcode);
        }

        public void AddLoggingStepBinding(string attributeName, string methodName, string regex)
        {
            _solutionDriver.DefaultProject.AddLoggingStepBinding(attributeName, methodName, Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log"), regex);
        }

        public void AddStepBinding(string projectName, string bindingCode) => AddStepBinding(_solutionDriver.Projects[projectName], bindingCode);

        public void AddStepBinding(string bindingCode) => AddStepBinding(_solutionDriver.DefaultProject, bindingCode);

        public void AddProjectReference(string projectNameToReference)
        {
            AddProjectReference(_solutionDriver.DefaultProject, projectNameToReference);
        }

        public void AddProjectReference(string projectNameToReference, string targetProjectName)
        {
            var targetProject = _solutionDriver.Projects[targetProjectName];
            AddProjectReference(targetProject, projectNameToReference);
        }

        public void AddBindingClass(string rawBindingClass) => AddBindingClass(_solutionDriver.DefaultProject, rawBindingClass);

        private void AddStepBinding(ProjectBuilder targetProject, string bindingCode)
        {
            targetProject.AddStepBinding(bindingCode);
        }

        private void AddProjectReference(ProjectBuilder targetProject, string projectNameToReference)
        {
            var projectToReference = _solutionDriver.Projects[projectNameToReference];
            targetProject.AddProjectReference(Path.Combine(@"..", projectNameToReference, $"{projectNameToReference}.{projectToReference.Language.ToProjectFileExtension()}"), projectToReference);
        }

        private void AddBindingClass(ProjectBuilder project, string rawBindingClass)
        {
            project.AddBindingClass(rawBindingClass);
        }

        public void AddFile(string fileName, string fileContent, string compileAction, Dictionary<string, string> additionalMSBuildProperties)
        {
            _solutionDriver.DefaultProject.AddFile(new ProjectFile(fileName, compileAction, fileContent, CopyToOutputDirectory.CopyAlways, additionalMSBuildProperties));
        }

        public void AddFile(string fileName, string fileContent, string compileAction = "None")
        {
            _solutionDriver.DefaultProject.AddFile(new ProjectFile(fileName, compileAction, fileContent, CopyToOutputDirectory.CopyAlways, new Dictionary<string, string>()));
        }

        public void EnableTestParallelExecution()
        {
            _solutionDriver.DefaultProject.EnableParallelTestExecution();
        }

        public void AddNuGetPackage(string nugetPackage, string nugetVersion)
        {
            _solutionDriver.DefaultProject.AddNuGetPackage(nugetPackage, nugetVersion);
        }

        public void AddFailingStepBinding(string scenarioBlock, string stepRegex)
        {
            AddStepBinding(scenarioBlock, stepRegex, @"throw new System.Exception(""simulated failure"");", @"Throw New System.Exception(""simulated failure"")");
        }
    }
}
