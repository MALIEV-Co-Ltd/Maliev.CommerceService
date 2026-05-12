using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Domain.ValueObjects;

/// <summary>
/// Monetary amount with ISO currency code.
/// </summary>
public sealed record Money
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> record.
    /// </summary>
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Gets the decimal amount.
    /// </summary>
    [Range(0, 999999999)]
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets the ISO currency code.
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; init; }
}
