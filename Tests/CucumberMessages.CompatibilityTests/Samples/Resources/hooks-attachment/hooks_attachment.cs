using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CucumberMessages.CompatibilityTests.CCK.hooksMulti
{
    [Binding]
    internal class Hooks
    {
        private readonly IReqnrollOutputHelper reqnrollOutputHelper;

        public Hooks(IReqnrollOutputHelper reqnrollOutputHelper)
        {
            this.reqnrollOutputHelper = reqnrollOutputHelper;
        }

        [When("a step passes")]
        public void AStepPasses()
        {
        }

        // Hook implementations
        [BeforeScenario]
        public void BeforeScenarioHookWithAttachment()
        {
            AttachFile();
        }


        [AfterScenario()]
        public void AfterScenarioHookWithAttachment()
        {
            AttachFile();
        }


        private void AttachFile()
        {

            var ext = "svg";
            var path = FileSystemPath.GetFilePathForAttachments();
            var attachment = Path.Combine(path, "hooks-attachment", $"cucumber.{ext}");

            reqnrollOutputHelper.AddAttachment(attachment);
        }
    }
}
