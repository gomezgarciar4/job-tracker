using System.ComponentModel.DataAnnotations;
using JobTracker.Api.Models;

namespace JobTracker.Api.Dtos;

public record UpdateOfferDto(
    [Required, StringLength(150)] string Company,
    [Required, StringLength(150)] string Position,
    [Url, StringLength(500)] string? Url,
    string? Notes,
    OfferStatus Status);

