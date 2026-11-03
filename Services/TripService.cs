using System.Text.Json;
using TravelApi.Interfaces;
using TravelApi.Models;

namespace TravelApi.Services;

/// <summary>
/// שירות אחסון הטיולים בקובץ JSON. רשום כ-Singleton, ולכן כל גישה
/// לנתונים מוגנת בנעילה (lock) וכל כתיבה מתבצעת בצורה אטומית.
/// </summary>
public class TripService : ITripService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly string _tempPath;
    private readonly object _lock = new();
    private readonly List<Trip> _trips;

    public TripService(IWebHostEnvironment environment)
    {
        // שם הקובץ חייב להיות זהה בדיוק לשם שעל הדיסק — כולל אותיות גדולות/קטנות.
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "trips.json");
        _tempPath = _filePath + ".tmp";

        _trips = Load(_filePath);
    }

    public IReadOnlyList<Trip> GetAll()
    {
        lock (_lock)
        {
            return _trips.OrderBy(trip => trip.StartDate).ToArray();
        }
    }

    public Trip? Get(Guid id)
    {
        lock (_lock)
        {
            return _trips.FirstOrDefault(trip => trip.Id == id);
        }
    }

    public Trip Add(TripRequest request)
    {
        var trip = ToTrip(Guid.NewGuid(), request);

        lock (_lock)
        {
            _trips.Add(trip);
            Save();
        }

        return trip;
    }

    public Trip? Update(Guid id, TripRequest request)
    {
        var trip = ToTrip(id, request);

        lock (_lock)
        {
            var index = _trips.FindIndex(t => t.Id == id);
            if (index < 0)
            {
                return null;
            }

            _trips[index] = trip;
            Save();
        }

        return trip;
    }

    public bool Delete(Guid id)
    {
        lock (_lock)
        {
            var removed = _trips.RemoveAll(trip => trip.Id == id) > 0;
            if (removed)
            {
                Save();
            }

            return removed;
        }
    }

    private static Trip ToTrip(Guid id, TripRequest request) => new(
        id,
        request.Name.Trim(),
        request.Destination.Trim(),
        request.StartDate,
        request.EndDate,
        request.Description?.Trim(),
        request.Budget);

    private static List<Trip> Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        var json = File.ReadAllText(filePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<Trip>>(json, JsonOptions) ?? [];
    }

    /// <summary>
    /// כתיבה אטומית: קודם לטמפוררי, ואז החלפה. צריך להיקרא בתוך lock.
    /// </summary>
    private void Save()
    {
        var json = JsonSerializer.Serialize(_trips, JsonOptions);

        File.WriteAllText(_tempPath, json);
        File.Move(_tempPath, _filePath, overwrite: true);
    }
}
