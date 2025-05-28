namespace Reqnroll.Assist
{
    public class InstanceCreationOptions
    {
        public bool VerifyAllColumnsBound { get; set; }
        public bool VerifyCaseInsensitive { get; set; }
        public bool RequireTableToProvideAllConstructorParameters { get; set; }

        internal bool AssumeInstanceIsAlreadyCreated { get; set; }
    }
}
