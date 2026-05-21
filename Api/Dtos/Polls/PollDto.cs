using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Dtos.Polls;

public sealed class PollOptionDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = default!;
    public int VoteCount { get; init; }
}

public sealed class PollDto
{
    public Guid Id { get; init; }
    public string Question { get; init; } = default!;
    public IReadOnlyList<PollOptionDto> Options { get; init; } = default!;
    public bool IsActive { get; init; }
    public int TotalVotes { get; init; }
}
