var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Travel API v1"));
}

app.UseHttpsRedirection();

var trips = new Dictionary<Guid, Trip>();
var tripsLock = new object();
var tripsApi = app.MapGroup("/api/trips");

tripsApi.MapGet("/", () =>
{
    lock (tripsLock)
    {
        return Results.Ok(trips.Values.OrderBy(trip => trip.StartDate).ToArray());
    }
});

tripsApi.MapGet("/{id:guid}", (Guid id) =>
{
    lock (tripsLock)
    {
        return trips.TryGetValue(id, out var trip)
            ? (IResult)Results.Ok(trip)
            : Results.NotFound();
    }
});

tripsApi.MapPost("/", (TripRequest request) =>
{
    var validationError = ValidateTrip(request);
    if (validationError is not null)
    {
        return (IResult)Results.BadRequest(new { error = validationError });
    }

    var trip = new Trip(
        Guid.NewGuid(),
        request.Name.Trim(),
        request.Destination.Trim(),
        request.StartDate,
        request.EndDate,
        request.Description?.Trim(),
        request.Budget);

    lock (tripsLock)
    {
        trips.Add(trip.Id, trip);
    }

    return Results.Created($"/api/trips/{trip.Id}", trip);
});

tripsApi.MapPut("/{id:guid}", (Guid id, TripRequest request) =>
{
    var validationError = ValidateTrip(request);
    if (validationError is not null)
    {
        return (IResult)Results.BadRequest(new { error = validationError });
    }

    var trip = new Trip(
        id,
        request.Name.Trim(),
        request.Destination.Trim(),
        request.StartDate,
        request.EndDate,
        request.Description?.Trim(),
        request.Budget);

    lock (tripsLock)
    {
        if (!trips.ContainsKey(id))
        {
            return Results.NotFound();
        }

        trips[id] = trip;
    }

    return Results.Ok(trip);
});

tripsApi.MapDelete("/{id:guid}", (Guid id) =>
{
    lock (tripsLock)
    {
        return trips.Remove(id) ? Results.NoContent() : Results.NotFound();
    }
});

app.Run();

static string? ValidateTrip(TripRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return "יש להזין שם לטיול.";
    }

    if (string.IsNullOrWhiteSpace(request.Destination))
    {
        return "יש להזין יעד לטיול.";
    }

    if (request.EndDate < request.StartDate)
    {
        return "תאריך הסיום חייב להיות אחרי תאריך ההתחלה.";
    }

    if (request.Budget < 0)
    {
        return "התקציב אינו יכול להיות שלילי.";
    }

    return null;
}

record TripRequest(
    string Name,
    string Destination,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Description,
    decimal? Budget);

record Trip(
    Guid Id,
    string Name,
    string Destination,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Description,
    decimal? Budget);
