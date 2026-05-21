using System;
using Domain.ValueObjects.Polls;

namespace Domain.Entities;

public class PollOption
{
    public PollOptionId Id { get; private set; } = default!;
    public OptionText Text { get; private set; } = default!;

    private PollOption() { }

    public PollOption(PollOptionId id, OptionText text)
    {
        Id = id;
        Text = text;
    }

    public void Update(OptionText text)
    {
        Text = text;
    }
}
