using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientTripDTO>> GetClientTrips(int idClient);
    Task<int> CreateClient(ClientDTO clientDto);
    Task<bool> ClientExists(string pesel);
}