namespace JobTracker.Api.Models;

public enum OfferStatus
{
    Pending,
    Applied,
    Interview,
    Rejected,
    Accepted
}

public class Offer
{
    public int Id { get; set; }
    public required string Company { get; set; }
    public required string Position { get; set; }
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public OfferStatus Status { get; set; } = OfferStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

