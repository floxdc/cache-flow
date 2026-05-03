using System;
using System.Threading;
using System.Threading.Tasks;

namespace FloxDc.CacheFlow.Infrastructure;


internal sealed class TaskEntry<T> : ITaskEntry
{
    public TaskEntry(TaskCompletionSource<T> taskSource, CancellationTokenSource cancellationSource)
    {
        TaskSource = taskSource;
        CancellationSource = cancellationSource;
        CreatedAt = DateTimeOffset.UtcNow;
    }


    public TaskCompletionSource<T> TaskSource { get; }


    public CancellationTokenSource CancellationSource { get; }


    public DateTimeOffset CreatedAt { get; }
}
