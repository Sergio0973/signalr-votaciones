using System;
using Domain.ValueObjects.Polls;

namespace Domain.Entities;

public class Vote
{
    public VoterId VoterId { get; private set; } = default!;
    public PollOptionId OptionId { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private Vote() { }

    public Vote(VoterId voterId, PollOptionId optionId)
    {
        VoterId = voterId;
        OptionId = optionId;
        CreatedAt = DateTime.UtcNow;
    }
}
