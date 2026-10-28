using JobTracker.Api.Controllers;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos;
using JobTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Tests;

public class OffersControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly OffersController _controller;

    public OffersControllerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _controller = new OffersController(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<OfferDto> CreateTestOffer(
        string company = "TestCo",
        string position = "Dev",
        OfferStatus status = OfferStatus.Pending)
    {
        var dto = new CreateOfferDto(company, position, null, null);
        var result = await _controller.Create(dto, CancellationToken.None);
        var created = result.Result as CreatedAtActionResult;
        var offer = (OfferDto)created!.Value!;

        if (status != OfferStatus.Pending)
        {
            await _controller.UpdateStatus(offer.Id, new UpdateStatusDto(status), CancellationToken.None);
        }

        return offer;
    }

    [Fact]
    public async Task Create_ReturnsCreatedOffer()
    {
        var dto = new CreateOfferDto("Google", "SWE", "https://careers.google.com", "Notes");

        var result = await _controller.Create(dto, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var offer = Assert.IsType<OfferDto>(created.Value);
        Assert.Equal("Google", offer.Company);
        Assert.Equal("SWE", offer.Position);
        Assert.Equal("https://careers.google.com", offer.Url);
        Assert.Equal(OfferStatus.Pending, offer.Status);
    }

    [Fact]
    public async Task GetAll_ReturnsAllOffers_OrderedByCreatedAtDesc()
    {
        var first = await CreateTestOffer("Alpha", "Dev1");
        var second = await CreateTestOffer("Beta", "Dev2");

        var result = await _controller.GetAll(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var offers = Assert.IsType<List<OfferDto>>(ok.Value);
        Assert.Equal(2, offers.Count);
        Assert.Equal(second.Id, offers[0].Id);
        Assert.Equal(first.Id, offers[1].Id);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsFiltered()
    {
        await CreateTestOffer("Alpha", "Dev1", OfferStatus.Pending);
        await CreateTestOffer("Beta", "Dev2", OfferStatus.Interview);

        var result = await _controller.GetAll(OfferStatus.Interview, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var offers = Assert.IsType<List<OfferDto>>(ok.Value);
        Assert.Single(offers);
        Assert.Equal("Beta", offers[0].Company);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var result = await _controller.GetById(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_Exists_ReturnsOffer()
    {
        var created = await CreateTestOffer();

        var result = await _controller.GetById(created.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var offer = Assert.IsType<OfferDto>(ok.Value);
        Assert.Equal(created.Id, offer.Id);
    }

    [Fact]
    public async Task Update_Exists_ReturnsNoContent()
    {
        var created = await CreateTestOffer();
        var dto = new UpdateOfferDto("Updated", "Senior Dev", null, "Updated notes", OfferStatus.Applied);

        var result = await _controller.Update(created.Id, dto, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        var updated = await _db.Offers.FindAsync(created.Id);
        Assert.Equal("Updated", updated!.Company);
        Assert.Equal(OfferStatus.Applied, updated.Status);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var dto = new UpdateOfferDto("X", "Y", null, null, OfferStatus.Pending);

        var result = await _controller.Update(999, dto, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_Exists_ReturnsNoContent()
    {
        var created = await CreateTestOffer();

        var result = await _controller.UpdateStatus(created.Id, new UpdateStatusDto(OfferStatus.Accepted), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        var updated = await _db.Offers.FindAsync(created.Id);
        Assert.Equal(OfferStatus.Accepted, updated!.Status);
    }

    [Fact]
    public async Task UpdateStatus_NotFound_Returns404()
    {
        var result = await _controller.UpdateStatus(999, new UpdateStatusDto(OfferStatus.Accepted), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Exists_ReturnsNoContent()
    {
        var created = await CreateTestOffer();

        var result = await _controller.Delete(created.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await _db.Offers.FindAsync(created.Id));
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        var result = await _controller.Delete(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}

