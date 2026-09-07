using DigitalArs.Application.DTOs.Cards;

namespace DigitalArs.Application.Interfaces;

public interface ICardService
{
    /// <summary>Solicita una tarjeta virtual para el usuario.</summary>
    Task<CardResponse> RequestVirtualCardAsync(int userId, CancellationToken ct = default);

    /// <summary>Lista las tarjetas del usuario con datos enmascarados.</summary>
    Task<IReadOnlyList<CardResponse>> GetMyCardsAsync(int userId, CancellationToken ct = default);

    /// <summary>Revela los datos completos de una tarjeta específica del usuario.</summary>
    Task<CardResponse> RevealCardAsync(int userId, int cardId, CancellationToken ct = default);

    /// <summary>Congela o descongela una tarjeta del usuario.</summary>
    Task<CardResponse> ToggleFreezeAsync(int userId, int cardId, CancellationToken ct = default);

    /// <summary>Da de baja definitiva una tarjeta del usuario.</summary>
    Task DeactivateCardAsync(int userId, int cardId, CancellationToken ct = default);
}
