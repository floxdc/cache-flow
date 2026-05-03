using System;
using System.Threading.Tasks;

namespace FloxDc.CacheFlow.Infrastructure;


internal sealed class TaskEntry<T> : ITaskEntry
{
    public TaskEntry(TaskCompletionSource<T> taskSource)
    {
        TaskSource = taskSource;
        CreatedAt = DateTimeOffset.UtcNow;
    }


    public TaskCompletionSource<T> TaskSource { get; }


    public DateTimeOffset CreatedAt { get; }
}
