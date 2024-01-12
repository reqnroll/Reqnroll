using System.Linq;
using Reqnroll.Generator.Interfaces;

namespace Reqnroll.Generator
{
    public interface ITestUpToDateChecker
    {
        bool? IsUpToDatePreliminary(FeatureFileInput featureFileInput, string generatedTestFullPath, UpToDateCheckingMethod upToDateCheckingMethod);
        bool IsUpToDate(FeatureFileInput featureFileInput, string generatedTestFullPath, string generatedTestContent, UpToDateCheckingMethod upToDateCheckingMethod);
    }
}
