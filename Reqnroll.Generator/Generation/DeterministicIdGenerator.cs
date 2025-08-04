using Gherkin.CucumberMessages;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Reqnroll.Generator.Generation;

public class DeterministicIdGenerator : IIdGenerator
{
    private readonly Guid _namespaceGuid;
    private int _counter;

    public DeterministicIdGenerator(string seed)
    {
        // Hash the seed string to derive a namespace GUID
        using (var sha1 = SHA1.Create())
        {
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(seed));
            hash[6] = (byte)((hash[6] & 0x0F) | 0x50); // Set version to 5
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // Set variant to IETF 1.0

            // Use the first 16 bytes of the hash to create a GUID
            byte[] hashBytesToUse = new byte[16];
            Array.Copy(hash, hashBytesToUse, 16);

            _namespaceGuid = new Guid(hashBytesToUse);
        }
        _counter = 0;
    }

    public string GetNewId()
    {
        // Use the current counter as the "name" to make the sequence deterministic
        var name = _counter.ToString();
        _counter++;

        // Generate deterministic GUID
        var guid = UUIDv5.Create(_namespaceGuid, name);
        return guid.ToString("N"); // "N" = 32 digits, no hyphens
    }
}

