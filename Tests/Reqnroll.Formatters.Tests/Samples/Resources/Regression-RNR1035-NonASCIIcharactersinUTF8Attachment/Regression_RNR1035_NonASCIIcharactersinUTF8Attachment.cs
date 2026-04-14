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
        // write the string to a file as UTF-8 in current directory
        var fileName = $"Regression-RNR1035-NonASCIIcharactersinUTF8Attachment.txt";
        System.IO.File.WriteAllText(fileName, text, Encoding.UTF8);
        reqnrollOutputHelper.AddAttachment(fileName);
    }

}