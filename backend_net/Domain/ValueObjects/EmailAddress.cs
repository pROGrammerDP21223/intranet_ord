using System.Text.RegularExpressions;

namespace backend_net.Domain.ValueObjects;

/// <summary>
/// Value Object representing an email address
/// Ensures email is always in valid format
/// </summary>
public sealed class EmailAddress : IEquatable<EmailAddress>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address cannot be null or empty", nameof(value));

        var trimmedValue = value.Trim().ToLowerInvariant();
        
        if (!EmailRegex.IsMatch(trimmedValue))
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));

        Value = trimmedValue;
    }

    public static EmailAddress Create(string email)
    {
        return new EmailAddress(email);
    }

    public static bool TryCreate(string email, out EmailAddress? emailAddress)
    {
        emailAddress = null;
        try
        {
            emailAddress = new EmailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Equals(EmailAddress? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => obj is EmailAddress other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(EmailAddress? left, EmailAddress? right) => Equals(left, right);
    public static bool operator !=(EmailAddress? left, EmailAddress? right) => !Equals(left, right);

    public override string ToString() => Value;
}

