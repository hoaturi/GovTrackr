using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.PresidentialAction;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Api.Features.PresidentialAction.GetPresidentialActions;

public class
    GetPresidentialActionsQueryHandler(AppDbContext dbContext) : IRequestHandler<GetPresidentialActionsQuery,
    Result<GetPresidentialActionsResponse>>
{
    private const int PageSize = 20;

    public async Task<Result<GetPresidentialActionsResponse>> Handle(GetPresidentialActionsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<PresidentialActionTranslation> query = dbContext.PresidentialActionTranslations;

        if (request.Category is not null && request.Category != string.Empty)
            query = query.Where(a => a.PresidentialAction.SubCategory.Slug == request.Category);

        if (request.FromDate.HasValue)
            query = query.Where(a => a.PresidentialAction.PublishedAt >= request.FromDate.Value.ToUniversalTime());

        if (request.ToDate.HasValue)
            query = query.Where(a => a.PresidentialAction.PublishedAt <= request.ToDate.Value.ToUniversalTime());

        var totalCount = await query.CountAsync(cancellationToken);

        var actions = await query
            .OrderByDescending(a => a.PresidentialAction.PublishedAt)
            .Skip((request.Page - 1) * PageSize)
            .Take(PageSize)
            .Select(a => new GetPresidentialActionsItem(
                a.Id,
                a.Title,
                a.Summary,
                a.PresidentialAction.Title,
                a.PresidentialAction.SourceUrl,
                a.PresidentialAction.PublishedAt,
                a.PresidentialAction.SubCategory.Name
            ))
            .ToListAsync(cancellationToken);

        return Result.Ok(new GetPresidentialActionsResponse(actions, totalCount, request.Page));
    }
}