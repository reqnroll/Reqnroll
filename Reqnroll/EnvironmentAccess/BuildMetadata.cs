using System.Runtime.CompilerServices;

namespace Reqnroll.EnvironmentAccess
{
    public record BuildMetadata(
            string BuildUrl,
            string BuildNumber,
            string Remote,
            string Revision,
            string Branch,
            string Tag)
    {
        public string ProductName { get; init; }
    }
}
