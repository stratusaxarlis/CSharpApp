namespace CSharpApp.Application.Common.Helpers;

public sealed class RequestPerformanceState : IRequestPerformanceState
{
    private int _count;
    public int Increment() =>
        // Increment ExecutionCount in a thread-safe manner.
        Interlocked.Increment(ref _count);
}
