using System;

namespace Reqnroll.RuntimeTests
{
    public class TestEnvironmentHelper
    {
        public static bool IsBeingRunByNCrunch()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvironmentVariableNames.NCrunch));
        }
    }
}