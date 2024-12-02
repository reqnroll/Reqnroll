using System;

namespace Reqnroll.Generator.Interfaces
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class TestGeneratorFactoryAttribute : Attribute
    {
        public string Type { get; private set; }

        public TestGeneratorFactoryAttribute(string type)
        {
            Type = type;
        }
    }
}