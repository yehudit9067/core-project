using Microsoft.AspNetCore.Mvc;
using TravelApi.Interfaces;
using TravelApi.Models;
using TravelApi.Services;

namespace TravelApi.Controllers;

/// <summary>
/// קונטרולר REST עבור טיולים. הנתיב מוגדר ב-[Route], ולכן ה-Map* ב-Program.cs
/// מוחלף ב-app.MapControllers().
/// </summary>
[ApiController]
[Route("api/trips")]
public class TripsController : ControllerBase
{
    private readonly ITripService _service;

    public TripsController(ITripService service) => _service = service;

    /// <summary>GET /api/trips</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<Trip>> GetAll() => Ok(_service.GetAll());

    /// <summary>GET /api/trips/{id}</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Trip> Get(Guid id) =>
        _service.Get(id) is { } trip ? Ok(trip) : NotFound();

    /// <summary>POST /api/trips</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Trip> Create(TripRequest request)
    {
        var validationError = TripValidator.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(new { error = validationError });
        }

        var trip = _service.Add(request);
        return CreatedAtAction(nameof(Get), new { id = trip.Id }, trip);
    }

    /// <summary>PUT /api/trips/{id}</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Trip> Update(Guid id, TripRequest request)
    {
        var validationError = TripValidator.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(new { error = validationError });
        }

        var trip = _service.Update(id, request);
        return trip is null ? NotFound() : Ok(trip);
    }

    /// <summary>DELETE /api/trips/{id}</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        _service.Delete(id) ? NoContent() : NotFound();
}
