using System;

namespace Api.Dtos.Polls;

public sealed class VoteDto
{
    public string VoterId { get; init; } = default!;
    public Guid OptionId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CreateVoteRequest
{
    public string VoterId { get; init; } = default!;
    public Guid OptionId { get; init; }
}
