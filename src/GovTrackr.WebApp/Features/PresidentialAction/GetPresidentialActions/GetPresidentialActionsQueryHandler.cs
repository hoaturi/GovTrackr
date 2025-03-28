using FluentResults;
using GovTrackr.Application.Infrastructure.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GovTrackr.Application.Features.PresidentialAction.GetPresidentialActions;

public class GetPresidentialActionsQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetPresidentialActionsQuery, Result<GetPresidentialActionsResponse>>
{
    public async Task<Result<GetPresidentialActionsResponse>> Handle(GetPresidentialActionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PresidentialActionTranslations
            .AsNoTracking()
            .AsQueryable();

        if (request.Category is not null)
            query = query.Where(t => t.PresidentialAction.Classification.Slug == request.Category);

        if (request.FromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(request.FromDate.Value, DateTimeKind.Utc);
            query = query.Where(a => a.PresidentialAction.PublishedAt >= fromDateUtc);
        }

        if (request.ToDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(request.ToDate.Value, DateTimeKind.Utc);
            query = query.Where(a => a.PresidentialAction.PublishedAt <= toDateUtc);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var translatedActions = await query
            .OrderByDescending(t => t.PresidentialAction.PublishedAt)
            .Select(t => new GetPresidentialActionsItem(
                t.Id,
                t.Title,
                t.Summary,
                t.PresidentialAction.Title,
                t.PresidentialAction.SourceUrl,
                t.PresidentialAction.PublishedAt,
                t.PresidentialAction.Classification
            ))
            .Skip((request.Page - 1) * 20)
            .ToListAsync(cancellationToken);

        return Result.Ok(new GetPresidentialActionsResponse(translatedActions, totalCount, request.Page));
    }
}