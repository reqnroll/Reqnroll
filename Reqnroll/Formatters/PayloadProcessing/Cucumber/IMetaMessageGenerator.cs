using Io.Cucumber.Messages.Types;

namespace Reqnroll.Formatters.PayloadProcessing.Cucumber;

public interface IMetaMessageGenerator
{
    Meta GenerateMetaMessage();
}