using System;

namespace Domain.ValueObjects.Polls;

public sealed record PollOptionId(Guid Value)
{
    public static PollOptionId Create(Guid value) => new(value);
    public static PollOptionId New() => new(Guid.NewGuid());
}
