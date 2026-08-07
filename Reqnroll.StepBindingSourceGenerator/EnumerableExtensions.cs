namespace Reqnroll.StepBindingSourceGenerator;

internal static class EnumerableExtensions
{
    public static int GetSequenceHashCode<T>(this IEnumerable<T> values)
    {
        unchecked
        {
            var hash = 4037797;

            foreach (var item in values)
            {
                hash *= 1864987 + item?.GetHashCode() ?? 0;
            }

            return hash;
        }
    }
}
