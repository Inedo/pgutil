namespace Inedo.DependencyScan;

internal static class InternalExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = new List<T>();

        await foreach (var item in source.ConfigureAwait(false).WithCancellation(cancellationToken))
        {
            list.Add(item);
        }

        return list;
    }
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new EnumerableWrapper<T>(source);
    }

    public static async IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> source, IAsyncEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(other);

        await foreach (var item in source.ConfigureAwait(false))
            yield return item;

        await foreach (var item in other.ConfigureAwait(false))
            yield return item;

    }

    private sealed class EnumerableWrapper<T>(IEnumerable<T> source) : IAsyncEnumerable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new EnumeratorWrapper(source.GetEnumerator());

        private sealed class EnumeratorWrapper(IEnumerator<T> enumerator) : IAsyncEnumerator<T>
        {
            public T Current => enumerator.Current;

            public ValueTask DisposeAsync()
            {
                enumerator.Dispose();
                return default;
            }
            public ValueTask<bool> MoveNextAsync() => new(enumerator.MoveNext());
        }
    }
}
