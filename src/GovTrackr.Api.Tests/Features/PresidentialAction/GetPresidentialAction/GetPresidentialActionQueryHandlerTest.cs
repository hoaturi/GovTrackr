using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovTrackr.Application.Features.PresidentialAction.GetPresidentialAction;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Domain.Common;
using Shared.Infrastructure.Persistence.Context;
using Shared.Models.Errors;
using Xunit;

namespace GovTrackr.Api.Tests.Features.PresidentialAction.GetPresidentialAction;

[Collection("Database")]
public class GetPresidentialActionQueryHandlerTest : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly GetPresidentialActionQueryHandler _handler;
    private readonly IDbContextTransaction _transaction;

    public GetPresidentialActionQueryHandlerTest(DatabaseFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _transaction = _dbContext.Database.BeginTransaction();
        _handler = new GetPresidentialActionQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _transaction.Rollback();
        _transaction.Dispose();
    }

    [Fact]
    public async Task Handle_ReturnsAction_WhenActionExists()
    {
        // Arrange
        var testDataHelper = new TestDataHelper(_dbContext);
        var translationId = Guid.NewGuid();

        await testDataHelper.CreatePresidentialActionWithTranslationAsync(
            translationId,
            DocumentSubCategoryType.ExecutiveOrder,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow);

        var request = new GetPresidentialActionQuery(translationId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(translationId, result.Value.Id);
    }

    [Fact]
    public async Task Handle_ReturnsNotFoundError_WhenActionDoesNotExist()
    {
        // Arrange
        var request = new GetPresidentialActionQuery(Guid.NewGuid());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<NotFoundError>(result.Errors.First());
    }
}