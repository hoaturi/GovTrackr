using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovTrackr.Application.Features.PresidentialAction.GetPresidentialActions;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Domain.Common;
using Shared.Infrastructure.Persistence.Context;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace GovTrackr.Api.Tests.Features.PresidentialAction.GetPresidentialActions;

[Collection("Database")]
public class GetPresidentialActionsQueryHandlerTests : IAsyncLifetime
{
    private const int ExecutiveOrderCount = 1;
    private const int ProclamationCount = 1;
    private const int MemorandaCount = 1;
    private const int TotalActionCount = ExecutiveOrderCount + ProclamationCount + MemorandaCount;
    private readonly DateTime _dateFrom = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc);
    private readonly DateTime _dateTo = DateTime.SpecifyKind(new DateTime(2024, 12, 31), DateTimeKind.Utc);
    private readonly AppDbContext _dbContext;
    private readonly GetPresidentialActionsQueryHandler _handler;
    private readonly IDbContextTransaction _transaction;

    public GetPresidentialActionsQueryHandlerTests(DatabaseFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _dbContext = fixture.DbContext;
        _transaction = _dbContext.Database.BeginTransaction();
        _handler = new GetPresidentialActionsQueryHandler(_dbContext);
    }

    public async Task InitializeAsync()
    {
        await InitializeTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
    }

    private async Task InitializeTestDataAsync()
    {
        var testDataHelper = new TestDataHelper(_dbContext);

        await testDataHelper.CreatePresidentialActionsAsync(ExecutiveOrderCount, DocumentSubCategoryType.ExecutiveOrder,
            _dateFrom, _dateTo);

        await testDataHelper.CreatePresidentialActionsAsync(ProclamationCount, DocumentSubCategoryType.Proclamation,
            _dateFrom, _dateTo);

        await testDataHelper.CreatePresidentialActionsAsync(MemorandaCount, DocumentSubCategoryType.Memoranda,
            _dateFrom, _dateTo);
    }

    [Theory]
    [InlineData(null, null, null, TotalActionCount)]
    [InlineData("executive-order", null, null, ExecutiveOrderCount)]
    [InlineData("proclamation", null, null, ProclamationCount)]
    [InlineData("memoranda", null, null, MemorandaCount)]
    public async Task Handle_WithCategoryFilter_ReturnsMatchingActions(string category, DateTime? fromDate,
        DateTime? toDate, int expectedCount)
    {
        // Arrange
        var query = new GetPresidentialActionsQuery(category, fromDate, toDate, 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBeGreaterThanOrEqualTo(expectedCount);

        if (category != null)
            foreach (var item in result.Value.Items)
                item.SubCategory.Slug.ShouldBe(category);
    }

    [Fact]
    public async Task Handle_WithDateRange_ReturnsActionsWithinRange()
    {
        // Arrange
        var query = new GetPresidentialActionsQuery(null, _dateFrom, _dateTo, 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBeGreaterThanOrEqualTo(TotalActionCount);

        foreach (var item in result.Value.Items)
        {
            item.PublishedAt.ShouldBeGreaterThanOrEqualTo(_dateFrom);
            item.PublishedAt.ShouldBeLessThanOrEqualTo(_dateTo);
        }
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var query1 = new GetPresidentialActionsQuery(null, null, null, 1);
        var query2 = new GetPresidentialActionsQuery(null, null, null, 2);

        // Act
        var result1 = await _handler.Handle(query1, CancellationToken.None);
        var result2 = await _handler.Handle(query2, CancellationToken.None);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();

        result1.Value.Page.ShouldBe(1);
        result2.Value.Page.ShouldBe(2);

        result1.Value.TotalCount.ShouldBe(result2.Value.TotalCount);

        result1.Value.Items.Count.ShouldBeGreaterThan(0);

        var page1Ids = result1.Value.Items.Select(item => item.Id).ToHashSet();
        var page2Ids = result2.Value.Items.Select(item => item.Id).ToHashSet();
        page1Ids.Overlaps(page2Ids).ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WithAllFilters_ReturnsFilteredAndPaginatedResults()
    {
        // Arrange
        var query = new GetPresidentialActionsQuery("executive-order", _dateFrom, _dateTo, 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBeGreaterThanOrEqualTo(ExecutiveOrderCount);

        foreach (var item in result.Value.Items)
        {
            item.SubCategory.Slug.ShouldBe("executive-order");
            item.PublishedAt.ShouldBeGreaterThanOrEqualTo(_dateFrom);
            item.PublishedAt.ShouldBeLessThanOrEqualTo(_dateTo);
        }
    }
}