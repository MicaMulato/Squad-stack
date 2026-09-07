using DigitalArs.Application.DTOs.Cards;
using DigitalArs.Application.Exceptions;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using DigitalArs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DigitalArs.Infrastructure.Services;

public class CardService : ICardService
{
    private readonly ApplicationDbContext _context;

    public CardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CardResponse> RequestVirtualCardAsync(int userId, CancellationToken ct = default)
    {
        var account = await _context.Accounts
            .Include(a => a.User)
            .Include(a => a.Cards)
            .FirstOrDefaultAsync(a => a.UserId == userId, ct);

        if (account is null)
            throw new NotFoundException("No se encontró una cuenta bancaria asociada a este usuario.");

        // Verificar si ya tiene una tarjeta virtual activa
        var existingVirtual = account.Cards
            .FirstOrDefault(c => c.Type == CardType.Virtual && c.IsActive);

        if (existingVirtual is not null)
            throw new BadRequestException("Ya posees una tarjeta virtual activa.");

        var now = DateTime.UtcNow;
        var holderName = $"{account.User!.FirstName} {account.User.LastName}".ToUpperInvariant();

        var card = new Card
        {
            AccountId = account.Id,
            HolderName = holderName,
            CardNumber = GenerateCardNumber(),
            SecurityCode = GenerateCvv(),
            ExpirationDate = now.AddYears(3),
            Type = CardType.Virtual,
            CreatedAt = now,
            IsActive = true,
            IsFrozen = false
        };

        await _context.Cards.AddAsync(card, ct);
        await _context.SaveChangesAsync(ct);

        return MapToMaskedResponse(card);
    }

    public async Task<IReadOnlyList<CardResponse>> GetMyCardsAsync(int userId, CancellationToken ct = default)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, ct);

        if (account is null)
            return Array.Empty<CardResponse>();

        var cards = await _context.Cards
            .AsNoTracking()
            .Where(c => c.AccountId == account.Id && c.IsActive)
            .OrderBy(c => c.Type)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        return cards.Select(MapToMaskedResponse).ToList();
    }

    public async Task<CardResponse> RevealCardAsync(int userId, int cardId, CancellationToken ct = default)
    {
        var card = await GetUserCardOrThrowAsync(userId, cardId, ct);

        return new CardResponse
        {
            Id = card.Id,
            HolderName = card.HolderName,
            CardNumber = FormatCardNumber(card.CardNumber),
            LastFourDigits = card.CardNumber[^4..],
            SecurityCode = card.SecurityCode,
            ExpirationDate = card.ExpirationDate.ToString("MM/yy"),
            Type = card.Type.ToString(),
            IsActive = card.IsActive,
            IsFrozen = card.IsFrozen,
            CreatedAt = card.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    public async Task<CardResponse> ToggleFreezeAsync(int userId, int cardId, CancellationToken ct = default)
    {
        var card = await GetUserCardOrThrowAsync(userId, cardId, ct);

        if (!card.IsActive)
            throw new BadRequestException("No se puede congelar/descongelar una tarjeta dada de baja.");

        card.IsFrozen = !card.IsFrozen;
        await _context.SaveChangesAsync(ct);

        return MapToMaskedResponse(card);
    }

    public async Task DeactivateCardAsync(int userId, int cardId, CancellationToken ct = default)
    {
        var card = await GetUserCardOrThrowAsync(userId, cardId, ct);

        if (!card.IsActive)
            throw new BadRequestException("La tarjeta ya se encuentra dada de baja.");

        card.IsActive = false;
        card.IsFrozen = false;
        await _context.SaveChangesAsync(ct);
    }

    // ─── Helpers privados ───────────────────────────────────────────

    private async Task<Card> GetUserCardOrThrowAsync(int userId, int cardId, CancellationToken ct)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, ct);

        if (account is null)
            throw new NotFoundException("No se encontró una cuenta bancaria asociada a este usuario.");

        var card = await _context.Cards
            .FirstOrDefaultAsync(c => c.Id == cardId && c.AccountId == account.Id, ct);

        if (card is null)
            throw new NotFoundException($"No se encontró la tarjeta con ID {cardId}.");

        return card;
    }

    private static CardResponse MapToMaskedResponse(Card card)
    {
        return new CardResponse
        {
            Id = card.Id,
            HolderName = card.HolderName,
            CardNumber = MaskCardNumber(card.CardNumber),
            LastFourDigits = card.CardNumber[^4..],
            SecurityCode = "***",
            ExpirationDate = card.ExpirationDate.ToString("MM/yy"),
            Type = card.Type.ToString(),
            IsActive = card.IsActive,
            IsFrozen = card.IsFrozen,
            CreatedAt = card.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    /// <summary>Genera un número de tarjeta Visa de 16 dígitos (4 + 15 random).</summary>
    private static string GenerateCardNumber()
    {
        var rng = RandomNumberGenerator.Create();
        var digits = new char[16];
        digits[0] = '4'; // Prefijo Visa

        var bytes = new byte[15];
        rng.GetBytes(bytes);
        for (var i = 0; i < 15; i++)
            digits[i + 1] = (char)('0' + (bytes[i] % 10));

        return new string(digits);
    }

    /// <summary>Genera un CVV de 3 dígitos.</summary>
    private static string GenerateCvv()
    {
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[3];
        rng.GetBytes(bytes);
        return string.Concat(bytes.Select(b => (char)('0' + (b % 10))));
    }

    /// <summary>Enmascara el número: **** **** **** 1234</summary>
    private static string MaskCardNumber(string cardNumber)
    {
        if (cardNumber.Length < 4) return "****";
        return $"**** **** **** {cardNumber[^4..]}";
    }

    /// <summary>Formatea el número: 4539 1234 5678 9012</summary>
    private static string FormatCardNumber(string cardNumber)
    {
        if (cardNumber.Length != 16) return cardNumber;
        return $"{cardNumber[..4]} {cardNumber[4..8]} {cardNumber[8..12]} {cardNumber[12..]}";
    }
}
