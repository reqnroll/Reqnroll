using Reqnroll.Configuration;

namespace Reqnroll.Generator.Configuration
{
    public class ReqnrollProjectConfiguration
    {
        public ReqnrollConfiguration ReqnrollConfiguration { get; set; }

        public ReqnrollProjectConfiguration()
        {
            ReqnrollConfiguration = ConfigurationLoader.GetDefault(); // load defaults
        }

        protected bool Equals(ReqnrollProjectConfiguration other)
        {
            return Equals(ReqnrollConfiguration, other.ReqnrollConfiguration);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReqnrollProjectConfiguration) obj);
        }

        public override int GetHashCode()
        {
            return (ReqnrollConfiguration != null ? ReqnrollConfiguration.GetHashCode() : 0);
        }
    }
}