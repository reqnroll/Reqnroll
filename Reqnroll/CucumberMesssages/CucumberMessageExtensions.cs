#nullable enable

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;


namespace Reqnroll.CucumberMesssages
{
    public static class CucumberMessageExtensions
    {
        public static async IAsyncEnumerable<T> ReadAllAsync<T>(this ChannelReader<T> reader,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (reader.TryRead(out T? item))
                {
                    yield return item;
                }
            }
        }
    }
}
