using System;

namespace Domain.ValueObjects.Polls;

public sealed record OptionText
{
    public string Value { get; }

    private OptionText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("La opción no puede estar vacía.");

        Value = value;
    }

    public static OptionText Create(string value) => new(value);
}
