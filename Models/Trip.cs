namespace TravelApi.Models;

public record Trip(
    Guid Id,
    string Name,
    string Destination,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Description,
    decimal? Budget);

public record TripRequest(
    string Name,
    string Destination,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Description,
    decimal? Budget);
