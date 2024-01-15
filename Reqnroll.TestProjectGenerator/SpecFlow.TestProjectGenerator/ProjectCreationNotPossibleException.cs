using System;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class ProjectCreationNotPossibleException : Exception
    {
        public ProjectCreationNotPossibleException()
        {
        }

        public ProjectCreationNotPossibleException(string message) : base(message)
        {
        }

        public ProjectCreationNotPossibleException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}