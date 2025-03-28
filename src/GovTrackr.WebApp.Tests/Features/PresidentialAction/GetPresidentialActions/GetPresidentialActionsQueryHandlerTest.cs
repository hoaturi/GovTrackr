using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovTrackr.Application.Domain.Common;
using GovTrackr.Application.Domain.PresidentialAction;
using GovTrackr.Application.Features.PresidentialAction.GetPresidentialActions;
using Shouldly;
using Xunit;

namespace GovTrackr.WebApp.Tests.Features.PresidentialAction.GetPresidentialActions;

public class GetPresidentialActionsQueryHandlerTests : IClassFixture<DatabaseFixture>
{
    // Test data counts for verification
    private const int ExecutiveOrderCount = 1;
    private const int ProclamationCount = 1;
    private const int MemorandaCount = 1;
    private const int TotalActionCount = ExecutiveOrderCount + ProclamationCount + MemorandaCount;
    private readonly DateTime _dateFrom = DateTime.SpecifyKind(new DateTime(2023, 1, 1), DateTimeKind.Utc);
    private readonly DateTime _dateTo = DateTime.SpecifyKind(new DateTime(2023, 12, 31), DateTimeKind.Utc);
    private readonly DatabaseFixture _fixture;
    private readonly GetPresidentialActionsQueryHandler _handler;

    public GetPresidentialActionsQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _handler = new GetPresidentialActionsQueryHandler(_fixture.DbContext);

        // Initialize test data once for all tests
        InitializeTestDataAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeTestDataAsync()
    {
        var testDataHelper = new TestDataHelper(_fixture.DbContext);

        // Create test data for each classification type using the same date range
        await testDataHelper.CreatePresidentialActionsWithDateAsync(ExecutiveOrderCount,
            DocumentSubCategoryType.ExecutiveOrder, _dateFrom, _dateTo);

        await testDataHelper.CreatePresidentialActionsWithDateAsync(ProclamationCount,
            DocumentSubCategoryType.Proclamation, _dateFrom, _dateTo);

        await testDataHelper.CreatePresidentialActionsWithDateAsync(MemorandaCount, DocumentSubCategoryType.Memoranda,
            _dateFrom, _dateTo);
    }

    [Theory]
    [InlineData(null, null, null, TotalActionCount)]
    [InlineData("executive-order", null, null, ExecutiveOrderCount)]
    [InlineData("proclamation", null, null, ProclamationCount)]
    [InlineData("memoranda", null, null, MemorandaCount)]
    public async Task Handle_WithCategoryFilter_ReturnsMatchingActions(
        string category, DateTime? fromDate, DateTime? toDate, int expectedCount)
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
        // Arrange - Get both page 1 and page 2
        var query1 = new GetPresidentialActionsQuery(null, null, null, 1);
        var query2 = new GetPresidentialActionsQuery(null, null, null, 2);

        // Act
        var result1 = await _handler.Handle(query1, CancellationToken.None);
        var result2 = await _handler.Handle(query2, CancellationToken.None);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();

        // Verify page information is correct
        result1.Value.Page.ShouldBe(1);
        result2.Value.Page.ShouldBe(2);

        // Verify total count is consistent across pages
        result1.Value.TotalCount.ShouldBe(result2.Value.TotalCount);

        // Verify we get results (for page 1, with our test data)
        result1.Value.Items.Count.ShouldBeGreaterThan(0);

        // No items should appear on both pages
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