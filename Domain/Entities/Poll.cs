using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ValueObjects.Polls;

namespace Domain.Entities;

public class Poll
{
    public PollId Id { get; private set; } = default!;
    public QuestionText Question { get; private set; } = default!;
    public List<PollOption> Options { get; private set; } = new();
    public List<Vote> Votes { get; private set; } = new();
    public bool IsActive { get; private set; }

    private Poll() { }

    public Poll(PollId id, QuestionText question, List<OptionText> optionTexts)
    {
        if (optionTexts == null || optionTexts.Count < 2)
            throw new ArgumentException("Debe haber al menos 2 opciones de votación.", nameof(optionTexts));

        Id = id;
        Question = question;
        Options = optionTexts.Select(text => new PollOption(PollOptionId.New(), text)).ToList();
        IsActive = true;
    }

    public bool CastVote(VoterId voterId, PollOptionId optionId)
    {
        if (!IsActive)
            return false;

        if (!Options.Any(o => o.Id == optionId))
            return false;

        Votes.RemoveAll(v => v.VoterId == voterId);
        Votes.Add(new Vote(voterId, optionId));

        return true;
    }

    public void Close()
    {
        IsActive = false;
    }

    public void Update(QuestionText question, bool isActive)
    {
        Question = question;
        IsActive = isActive;
    }
}
