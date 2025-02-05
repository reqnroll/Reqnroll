using System.Collections.Generic;
using Gherkin.Ast;

namespace Reqnroll.ExternalData.ReqnrollPlugin.DataSources
{
    public interface ISpecificationProvider
    {
        ExternalDataSpecification GetSpecification(IEnumerable<Tag> tags, string sourceFilePath);
    }
}
