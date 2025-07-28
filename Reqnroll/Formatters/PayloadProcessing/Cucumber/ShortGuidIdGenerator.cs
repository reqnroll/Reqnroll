using Gherkin.CucumberMessages;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Reqnroll.Formatters.PayloadProcessing.Cucumber;

internal class ShortGuidIdGenerator : IIdGenerator, IDisposable
{
    private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
    private bool disposed = false;

    public string GetNewId()
    {
        byte[] bytes = new byte[16];
        rng.GetBytes(bytes);
        string compactBase64 = Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        return compactBase64;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            rng.Dispose();
            disposed = true;
        }
    }
}
