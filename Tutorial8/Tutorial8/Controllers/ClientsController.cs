using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[Route("api/clients")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;
    private readonly ITripsService _tripsService;
    
    public ClientsController(IClientsService clientsService, ITripsService tripsService)
    {
        _clientsService = clientsService;
        _tripsService = tripsService;
    }
    
    [HttpGet("{idClient}/trips")]
    public async Task<IActionResult> GetClientTrips(int idClient)
    {
        if (!await _tripsService.ClientExists(idClient))
        {
            return NotFound($"Client with ID {idClient} not found");
        }
        
        var trips = await _clientsService.GetClientTrips(idClient);
        return Ok(trips);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientDTO clientDto)
    {
        try
        {
            var idClient = await _clientsService.CreateClient(clientDto);
            return CreatedAtAction(nameof(GetClientTrips), new { idClient }, null);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
    
    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        // Implementation if needed
        return NoContent();
    }
    
    [HttpPost("{idClient}/trips/{idTrip}")]
    public async Task<IActionResult> AssignClientToTrip(
        int idClient, 
        int idTrip,
        [FromBody] AssignClientToTripDTO dto)
    {
        if (!await _tripsService.ClientExists(idClient))
        {
            return NotFound($"Client with ID {idClient} not found");
        }
        
        if (!await _tripsService.TripExists(idTrip))
        {
            return NotFound($"Trip with ID {idTrip} not found");
        }
        
        if (await _tripsService.ClientAlreadyAssigned(idClient, idTrip))
        {
            return BadRequest("Client is already assigned to this trip");
        }
        
        var currentParticipants = await _tripsService.GetTripParticipantsCount(idTrip);
        var maxParticipants = await _tripsService.GetTripMaxPeople(idTrip);
        
        if (currentParticipants >= maxParticipants)
        {
            return BadRequest("Trip has reached maximum participants");
        }
        
        await _tripsService.AssignClientToTrip(idClient, idTrip, dto.PaymentDate);
        return Ok();
    }
    
    [HttpDelete("{idClient}/trips/{idTrip}")]
    public async Task<IActionResult> RemoveClientFromTrip(int idClient, int idTrip)
    {
        if (!await _tripsService.ClientExists(idClient))
        {
            return NotFound($"Client with ID {idClient} not found");
        }
        
        if (!await _tripsService.TripExists(idTrip))
        {
            return NotFound($"Trip with ID {idTrip} not found");
        }
        
        if (!await _tripsService.ClientAlreadyAssigned(idClient, idTrip))
        {
            return BadRequest("Client is not assigned to this trip");
        }
        
        await _tripsService.RemoveClientFromTrip(idClient, idTrip);
        return NoContent();
    }
}