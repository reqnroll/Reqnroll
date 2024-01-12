namespace Reqnroll.Tracing
{
    /// <summary>
    /// Marker interface for trace listener that do not need queued execution
    /// </summary>
    public interface IThreadSafeTraceListener : ITraceListener
    {
    }
}