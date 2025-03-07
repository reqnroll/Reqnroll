internal static class Hash
{
    private const int Prime1 = 79754099;
    private const int Prime2 = 60214523;

    public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        var hash = Prime1;

        hash = (Prime2 * hash) + value1?.GetHashCode() ?? 0;
        hash = (Prime2 * hash) + value2?.GetHashCode() ?? 0;
        hash = (Prime2 * hash) + value3?.GetHashCode() ?? 0;

        return hash;
    }
}
