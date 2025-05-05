using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString;
    
    public ClientsService(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<List<ClientTripDTO>> GetClientTrips(int idClient)
    {
        var trips = new List<ClientTripDTO>();
        var query = @"
            SELECT t.IdTrip, t.Name, ct.RegisteredAt, ct.PaymentDate
            FROM Client_Trip ct
            JOIN Trip t ON ct.IdTrip = t.IdTrip
            WHERE ct.IdClient = @IdClient
            ORDER BY t.DateFrom DESC";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdClient", idClient);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        trips.Add(new ClientTripDTO
                        {
                            IdTrip = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            RegisteredAt = reader.GetDateTime(reader.GetOrdinal("RegisteredAt")),
                            PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate")) 
                                ? null 
                                : reader.GetDateTime(reader.GetOrdinal("PaymentDate"))
                        });
                    }
                }
            }
        }
        
        return trips;
    }

    public async Task<int> CreateClient(ClientDTO clientDto)
    {
        if (string.IsNullOrWhiteSpace(clientDto.FirstName) ||
            string.IsNullOrWhiteSpace(clientDto.LastName) ||
            string.IsNullOrWhiteSpace(clientDto.Email) ||
            string.IsNullOrWhiteSpace(clientDto.Pesel))
        {
            throw new ArgumentException("First name, last name, email and PESEL are required");
        }

        if (await ClientExists(clientDto.Pesel))
        {
            throw new InvalidOperationException("Client with this PESEL already exists");
        }

        var query = @"
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            OUTPUT INSERTED.IdClient
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@FirstName", clientDto.FirstName);
            command.Parameters.AddWithValue("@LastName", clientDto.LastName);
            command.Parameters.AddWithValue("@Email", clientDto.Email);
            command.Parameters.AddWithValue("@Telephone", clientDto.Telephone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Pesel", clientDto.Pesel);
            
            await connection.OpenAsync();
            return (int)await command.ExecuteScalarAsync();
        }
    }

    public async Task<bool> ClientExists(string pesel)
    {
        var query = "SELECT 1 FROM Client WHERE Pesel = @Pesel";
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@Pesel", pesel);
            await connection.OpenAsync();
            return await command.ExecuteScalarAsync() != null;
        }
    }
}