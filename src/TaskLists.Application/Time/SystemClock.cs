using TaskLists.Application.Abstractions.Time;

namespace TaskLists.Application.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
