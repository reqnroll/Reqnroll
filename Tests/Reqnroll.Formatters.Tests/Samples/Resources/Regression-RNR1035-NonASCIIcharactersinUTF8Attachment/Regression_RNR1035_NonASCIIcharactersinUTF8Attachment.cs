using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.RegressionRNR1035NonASCIIcharactersinUTF8Attachment;

[Binding]
internal class Attachments
{
    private readonly IReqnrollOutputHelper reqnrollOutputHelper;

    internal Attachments(IReqnrollOutputHelper reqnrollOutputHelper)
    {
        this.reqnrollOutputHelper = reqnrollOutputHelper;
    }

    [When(@"attaching the non-ASCII string")]
    public void WhenAttachTextAs(string text)
    {
        // Normalize line endings to LF so the file content — and therefore
        // its Base64 encoding — is identical on Windows and Linux.
        var normalizedText = text.Replace("\r\n", "\n");

        // write the string to a file as UTF-8 in current directory
        var fileName = $"Regression-RNR1035-NonASCIIcharactersinUTF8Attachment.txt";
        System.IO.File.WriteAllText(fileName, normalizedText, Encoding.UTF8);
        reqnrollOutputHelper.AddAttachment(fileName);
    }

}