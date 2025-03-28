using FluentResults;
using GovTrackr.Application.Infrastructure.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GovTrackr.Application.Features.PresidentialAction.GetPresidentialActions;

public class GetPresidentialActionsQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetPresidentialActionsQuery, Result<GetPresidentialActionsResponse>>
{
    private const int PageSize = 20;

    public async Task<Result<GetPresidentialActionsResponse>> Handle(GetPresidentialActionsQuery request,
        CancellationToken cancellationToken)
    {
        var baseQuery = dbContext.PresidentialActionTranslations
            .AsNoTracking();

        if (request.Category is not null)
            baseQuery = baseQuery.Where(a => a.PresidentialAction.SubCategory.Slug == request.Category);

        if (request.FromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(request.FromDate.Value, DateTimeKind.Utc);
            baseQuery = baseQuery.Where(a => a.PresidentialAction.PublishedAt >= fromDateUtc);
        }

        if (request.ToDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(request.ToDate.Value, DateTimeKind.Utc);
            baseQuery = baseQuery.Where(a => a.PresidentialAction.PublishedAt <= toDateUtc);
        }

        var orderedQuery = baseQuery.OrderByDescending(t => t.PresidentialAction.PublishedAt);

        var totalCount = await orderedQuery.CountAsync(cancellationToken);

        var translatedActions = await orderedQuery
            .Skip((request.Page - 1) * PageSize)
            .Take(PageSize)
            .Select(t => new GetPresidentialActionsItem(
                t.Id,
                t.Title,
                t.Summary,
                t.PresidentialAction.Title,
                t.PresidentialAction.SourceUrl,
                t.PresidentialAction.PublishedAt,
                t.PresidentialAction.SubCategory
            ))
            .ToListAsync(cancellationToken);

        return Result.Ok(new GetPresidentialActionsResponse(translatedActions, totalCount, request.Page));
    }
}