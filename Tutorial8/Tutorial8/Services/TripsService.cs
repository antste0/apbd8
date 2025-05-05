using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString;
    
    public TripsService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();
        
        // Query joins Trip with Country_Trip and Country tables
        var query = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                   c.IdCountry, c.Name AS CountryName
            FROM Trip t
            JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
            JOIN Country c ON ct.IdCountry = c.IdCountry
            ORDER BY t.DateFrom DESC";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                var tripDict = new Dictionary<int, TripDTO>();
                
                while (await reader.ReadAsync())
                {
                    var idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    
                    // If trip not already in dictionary, create new TripDTO
                    if (!tripDict.TryGetValue(idTrip, out var trip))
                    {
                        trip = new TripDTO
                        {
                            IdTrip = idTrip,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                            DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                            MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            Countries = new List<CountryDTO>()
                        };
                        tripDict.Add(idTrip, trip);
                        trips.Add(trip);
                    }
                    
                    // Add country information to the trip
                    trip.Countries.Add(new CountryDTO
                    {
                        Name = reader.GetString(reader.GetOrdinal("CountryName"))
                    });
                }
            }
        }
        
        return trips;
    }

    public async Task<bool> TripExists(int idTrip)
    {
        var query = "SELECT 1 FROM Trip WHERE IdTrip = @IdTrip";
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@IdTrip", idTrip);
            await connection.OpenAsync();
            return await command.ExecuteScalarAsync() != null;
        }
    }

    public async Task<bool> ClientExists(int idClient)
    {
        var query = "SELECT 1 FROM Client WHERE IdClient = @IdClient";
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@IdClient", idClient);
            await connection.OpenAsync();
            return await command.ExecuteScalarAsync() != null;
        }
    }

    public async Task<bool> ClientAlreadyAssigned(int idClient, int idTrip)
    {
        var query = "SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@IdClient", idClient);
            command.Parameters.AddWithValue("@IdTrip", idTrip);
            await connection.OpenAsync();
            return await command.ExecuteScalarAsync() != null;
        }
    }

    public async Task<int> GetTripParticipantsCount(int idTrip)
    {
        var query = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@IdTrip", idTrip);
            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
    }

    public async Task<int> GetTripMaxPeople(int idTrip)
    {
        var query = "SELECT MaxPeople FROM Trip WHERE IdTrip = @IdTrip";
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@IdTrip", idTrip);
            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
    }

    public async Task AssignClientToTrip(int idClient, int idTrip, DateTime? paymentDate)
    {
        var query = @"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
            VALUES (@IdClient, @IdTrip, GETDATE(), @PaymentDate)";
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@IdClient", idClient);
            command.Parameters.AddWithValue("@IdTrip", idTrip);
            command.Parameters.AddWithValue("@PaymentDate", paymentDate ?? (object)DBNull.Value);
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task RemoveClientFromTrip(int idClient, int idTrip)
    {
        var query = "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
        
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@IdClient", idClient);
            command.Parameters.AddWithValue("@IdTrip", idTrip);
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}