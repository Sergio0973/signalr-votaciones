using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Dtos.Polls;

public sealed class CreatePollRequest
{
    public string Question { get; init; } = default!;
    public IReadOnlyList<string> Options { get; init; } = default!;
}
