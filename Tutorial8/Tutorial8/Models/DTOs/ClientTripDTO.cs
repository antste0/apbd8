namespace Tutorial8.Models.DTOs;

public class ClientTripDTO
{
    public int IdTrip { get; set; }
    public string Name { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
}