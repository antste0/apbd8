using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<bool> TripExists(int idTrip);
    Task<bool> ClientExists(int idClient);
    Task<bool> ClientAlreadyAssigned(int idClient, int idTrip);
    Task<int> GetTripParticipantsCount(int idTrip);
    Task<int> GetTripMaxPeople(int idTrip);
    Task AssignClientToTrip(int idClient, int idTrip, DateTime? paymentDate);
    Task RemoveClientFromTrip(int idClient, int idTrip);
}