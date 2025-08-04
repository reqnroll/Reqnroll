using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.attachments
{
    [Binding]
    internal class Attachments
    {
        private readonly IReqnrollOutputHelper reqnrollOutputHelper;

        internal Attachments(IReqnrollOutputHelper reqnrollOutputHelper)
        {
            this.reqnrollOutputHelper = reqnrollOutputHelper;
        }

        [When(@"a JPEG image is attached")]
        public void WhenAJPEGImageIsAttached()
        {
            var ext = "jpeg" ;
            var path = FileSystemPath.GetFilePathForAttachments();
            var attachment = Path.Combine(path, "attachments", $"cucumber.{ext}");

            reqnrollOutputHelper.AddAttachment(attachment);
        }

        [When(@"a PNG image is attached")]
        public void WhenAPNGImageIsAttached()
        {
            var ext = "png";
            var path = FileSystemPath.GetFilePathForAttachments();
            var attachment = Path.Combine(path, "attachments", $"cucumber.{ext}");

            reqnrollOutputHelper.AddAttachment(attachment);
        }

    }
}
