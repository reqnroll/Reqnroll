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

        public static BuildMetadata Unknown
        {
            get
            {
                return new BuildMetadata(
                     BuildUrl: "UNKNOWN",
                     BuildNumber: "UNKNOWN",
                     Remote: "UNKNOWN",
                     Revision: "UNKNOWN",
                     Branch: "UNKNOWN",
                     Tag: "UNKNOWN")
                {
                    ProductName = ""
                };
            }
        }
    }
}
