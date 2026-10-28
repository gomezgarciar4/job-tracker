using JobTracker.Api.Data;
using JobTracker.Api.Dtos;
using JobTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/offers")]
public class OffersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OfferDto>>> GetAll(
        [FromQuery] OfferStatus? status,
        CancellationToken ct)
    {
        var query = db.Offers.AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var offers = await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => ToDto(o))
            .ToListAsync(ct);

        return Ok(offers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OfferDto>> GetById(int id, CancellationToken ct)
    {
        var offer = await db.Offers.FindAsync([id], ct);

        if (offer is null)
            return NotFound();

        return Ok(ToDto(offer));
    }

    [HttpPost]
    public async Task<ActionResult<OfferDto>> Create(CreateOfferDto dto, CancellationToken ct)
    {
        var offer = new Offer
        {
            Company = dto.Company,
            Position = dto.Position,
            Url = dto.Url,
            Notes = dto.Notes
        };

        db.Offers.Add(offer);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = offer.Id }, ToDto(offer));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateOfferDto dto, CancellationToken ct)
    {
        var offer = await db.Offers.FindAsync([id], ct);

        if (offer is null)
            return NotFound();

        offer.Company = dto.Company;
        offer.Position = dto.Position;
        offer.Url = dto.Url;
        offer.Notes = dto.Notes;
        offer.Status = dto.Status;

        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto, CancellationToken ct)
    {
        var offer = await db.Offers.FindAsync([id], ct);

        if (offer is null)
            return NotFound();

        offer.Status = dto.Status;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var offer = await db.Offers.FindAsync([id], ct);

        if (offer is null)
            return NotFound();

        db.Offers.Remove(offer);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static OfferDto ToDto(Offer o) => new(
        o.Id,
        o.Company,
        o.Position,
        o.Url,
        o.Notes,
        o.Status,
        o.CreatedAt);
}

