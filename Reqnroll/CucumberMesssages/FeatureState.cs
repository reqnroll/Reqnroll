namespace Reqnroll.CucumberMesssages
{
    internal class FeatureState
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } //This will be false if the feature could not be pickled
    }
}