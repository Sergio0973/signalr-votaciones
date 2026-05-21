using System;

namespace Domain.ValueObjects.Polls;

public sealed record PollId(Guid Value)
{
    public static PollId Create(Guid value) => new(value);
    public static PollId New() => new(Guid.NewGuid());
}
