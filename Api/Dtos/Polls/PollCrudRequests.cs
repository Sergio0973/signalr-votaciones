using System;
using System.Collections.Generic;

namespace Api.Dtos.Polls;

public sealed class UpdatePollRequest
{
    public string Question { get; init; } = default!;
    public bool IsActive { get; init; }
}

public sealed class CreatePollOptionRequest
{
    public string Text { get; init; } = default!;
}

public sealed class UpdatePollOptionRequest
{
    public string Text { get; init; } = default!;
}
