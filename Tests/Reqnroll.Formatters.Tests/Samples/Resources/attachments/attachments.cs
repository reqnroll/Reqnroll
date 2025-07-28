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

        [When(@"the string {string} is logged")]
        public void WhenLogText(string text)
        {
            reqnrollOutputHelper.WriteLine(text);
        }

        [When(@"text with ANSI escapes is logged")]
        public void WhenTextWithANSIEscapedIsLogged()
        {
            reqnrollOutputHelper.WriteLine("This displays a \x1b[31mr\x1b[0m\x1b[91ma\x1b[0m\x1b[33mi\x1b[0m\x1b[32mn\x1b[0m\x1b[34mb\x1b[0m\x1b[95mo\x1b[0m\x1b[35mw\x1b[0m");
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
