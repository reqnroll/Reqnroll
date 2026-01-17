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

        [When(@"the string {string} is attached as {string}")]
        public void WhenAttachTextAs(string text, string mediaType)
        {
            throw new PendingException("attaching strings with mediaTypes unsupported");
        }

        [When(@"the following string is attached as {string}:")]
        public void WhenAttachTextAsMultilineString(string mediaType, string text)
        {
            throw new PendingException("attaching strings with mediaTypes unsupported");
        }

        [When(@"an array with {int} bytes is attached as {string}")]
        public void WhenAttachByteArrayAs(int byteCount, string mediaType)
        {
            throw new PendingException("attaching byte arrays with mediaTypes unsupported");
        }

        [When(@"a PDF document is attached and renamed")]
        public void WhenAttachPdfDocumentRenamed()
        {
            throw new PendingException("attaching files and renaming them unsupported");
        }

        [When(@"a link to {string} is attached")]
        public void WhenAttachLinkTo(string url)
        {
            throw new PendingException("attaching links unsupported");
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

    }
}
