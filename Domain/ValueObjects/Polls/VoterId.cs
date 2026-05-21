using System;

namespace Domain.ValueObjects.Polls;

public sealed record VoterId
{
    public string Value { get; }

    private VoterId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El identificador del votante no puede estar vacío.");

        Value = value;
    }

    public static VoterId Create(string value) => new(value);
}
