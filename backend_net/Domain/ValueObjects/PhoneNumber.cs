using System.Text.RegularExpressions;

namespace backend_net.Domain.ValueObjects;

/// <summary>
/// Value Object representing a phone number
/// </summary>
public sealed class PhoneNumber : IEquatable<PhoneNumber>
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be null or empty", nameof(value));

        var cleaned = CleanPhoneNumber(value);
        
        if (!PhoneRegex.IsMatch(cleaned))
            throw new ArgumentException($"Invalid phone number format: {value}", nameof(value));

        Value = cleaned;
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        return new PhoneNumber(phoneNumber);
    }

    public static bool TryCreate(string phoneNumber, out PhoneNumber? result)
    {
        result = null;
        try
        {
            result = new PhoneNumber(phoneNumber);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string CleanPhoneNumber(string phoneNumber)
    {
        // Remove all non-digit characters except +
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // If it doesn't start with +, add it
        if (!cleaned.StartsWith("+"))
        {
            if (cleaned.StartsWith("0"))
            {
                cleaned = "+91" + cleaned.Substring(1);
            }
            else if (cleaned.Length == 10)
            {
                cleaned = "+91" + cleaned;
            }
            else
            {
                cleaned = "+" + cleaned;
            }
        }

        return cleaned;
    }

    public string ToWhatsAppFormat() => $"whatsapp:{Value}";

    public bool Equals(PhoneNumber? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => obj is PhoneNumber other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PhoneNumber? left, PhoneNumber? right) => Equals(left, right);
    public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !Equals(left, right);

    public override string ToString() => Value;
}

