using System.ComponentModel.DataAnnotations;

namespace JobTracker.Api.Dtos;

public record CreateOfferDto(
    [Required, StringLength(150)] string Company,
    [Required, StringLength(150)] string Position,
    [Url, StringLength(500)] string? Url,
    string? Notes);

