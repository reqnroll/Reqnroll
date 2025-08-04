using System;
using System.Security.Cryptography;

namespace Reqnroll.Generator.Generation;

public class UUIDv5
{
    public static Guid Create(Guid namespaceId, string name)
    {
        // Convert namespace and name to byte arrays
        byte[] namespaceBytes = namespaceId.ToByteArray();
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(name);

        // Concatenate the byte arrays
        byte[] data = new byte[namespaceBytes.Length + nameBytes.Length];
        Array.Copy(namespaceBytes, data, namespaceBytes.Length);
        Array.Copy(nameBytes, 0, data, namespaceBytes.Length, nameBytes.Length);

        // Calculate the SHA-1 hash
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hashBytes = sha1.ComputeHash(data);

            // Construct the UUID from the hash
            hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x50); // Set version to 5
            hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80); // Set variant to IETF 1.0

            // Use the first 16 bytes of the hash to create a GUID
            byte[] hashBytesToUse = new byte[16];
            Array.Copy(hashBytes, hashBytesToUse, 16);

            return new Guid(hashBytesToUse);
        }
    }
}