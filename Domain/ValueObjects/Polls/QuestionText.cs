using System;

namespace Domain.ValueObjects.Polls;

public sealed record QuestionText
{
    public string Value { get; }

    private QuestionText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("La pregunta no puede estar vacía.");

        Value = value;
    }

    public static QuestionText Create(string value) => new(value);
}
