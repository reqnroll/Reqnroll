#nullable enable

using OpenTelemetry.Resources;
using Reqnroll.Analytics.UserId;

namespace Reqnroll.Diagnostics;

/// <summary>
/// Detects the user executing the current process.
/// </summary>
/// <remarks>
/// <para>This detector provides the following attributes:</para>
/// <list type="table">
///   <item>
///     <term>user.id</term>
///     <description>The unique identifier for the current user.</description>
///   </item>
/// </list>
/// </remarks>
/// <param name="userIdStore">The store to fetch the user ID from.</param>
public class UserDetector(IUserUniqueIdStore userIdStore) : IResourceDetector
{
    public UserDetector() : this(new FileUserIdStore(new FileService(), new DirectoryService()))
    {
    }

    /// <summary>
    /// Detects the user resource.
    /// </summary>
    /// <returns>A <see cref="Resource"/> representing the user executing Reqnroll.</returns>
    public Resource Detect()
    {
        var userId = userIdStore.GetUserId();

        return new Resource(
        [
            new ("user.id", userId)
        ]);
    }
}
