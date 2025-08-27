namespace Reqnroll.Tools.MsBuild.Generation
{
    public class FeatureFileCodeBehindGeneratorResult
    {
        public string CodeBehindRelativePath { get; }
        public string EmbeddedMessagesStoragePath { get; }
        public string EmbeddedMessagesResourceName { get; }

        public FeatureFileCodeBehindGeneratorResult(string codeBehindRelativePath, string embeddedMessagesStoragePath, string embeddedMessagesResourceName)
        {
            CodeBehindRelativePath = codeBehindRelativePath;
            EmbeddedMessagesStoragePath = embeddedMessagesStoragePath;
            EmbeddedMessagesResourceName = embeddedMessagesResourceName;
        }
    }
}
