using TravelApi.Models;

namespace TravelApi.Interfaces;

public interface ITripService
{
    IReadOnlyList<Trip> GetAll();
    Trip? Get(Guid id);
    Trip Add(TripRequest request);
    Trip? Update(Guid id, TripRequest request);
    bool Delete(Guid id);
}
