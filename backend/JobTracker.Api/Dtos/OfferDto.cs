using JobTracker.Api.Models;

namespace JobTracker.Api.Dtos;

public record OfferDto(
    int Id,
    string Company,
    string Position,
    string? Url,
    string? Notes,
    OfferStatus Status,
    DateTime CreatedAt);

