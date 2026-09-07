using DigitalArs.Api.Extensions;
using DigitalArs.Application.DTOs.Cards;
using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

/// <summary>
/// Endpoints para la gestión y solicitud de tarjetas (HU-35).
/// </summary>
[ApiController]
[Route("api/cards")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardsController(ICardService cardService)
    {
        _cardService = cardService;
    }

    /// <summary>
    /// Solicita la emisión de una tarjeta virtual para el usuario autenticado.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Datos de la tarjeta virtual emitida (enmascarada).</returns>
    [HttpPost("virtual")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponse>> RequestVirtualCard(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _cardService.RequestVirtualCardAsync(userId, cancellationToken);
        return CreatedAtAction(nameof(GetMyCards), new { id = result.Id }, result);
    }

    /// <summary>
    /// Obtiene todas las tarjetas activas del usuario autenticado (datos enmascarados).
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de tarjetas del usuario.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<CardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<CardResponse>>> GetMyCards(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _cardService.GetMyCardsAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene los datos sensibles y completos (número completo y CVV) de una tarjeta del usuario autenticado.
    /// </summary>
    /// <param name="id">ID de la tarjeta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Datos completos de la tarjeta.</returns>
    [HttpGet("{id:int}/reveal")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponse>> RevealCard(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _cardService.RevealCardAsync(userId, id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Congela o descongela temporalmente una tarjeta del usuario autenticado.
    /// </summary>
    /// <param name="id">ID de la tarjeta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarjeta con el nuevo estado de congelamiento.</returns>
    [HttpPatch("{id:int}/freeze")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponse>> ToggleFreeze(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _cardService.ToggleFreezeAsync(userId, id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Da de baja definitivamente una tarjeta del usuario autenticado.
    /// </summary>
    /// <param name="id">ID de la tarjeta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateCard(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _cardService.DeactivateCardAsync(userId, id, cancellationToken);
        return NoContent();
    }
}
