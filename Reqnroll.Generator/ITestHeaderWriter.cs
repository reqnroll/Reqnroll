using System;
using System.Linq;

namespace Reqnroll.Generator
{
    public interface ITestHeaderWriter
    {
        Version DetectGeneratedTestVersion(string generatedTestContent);
    }
}
