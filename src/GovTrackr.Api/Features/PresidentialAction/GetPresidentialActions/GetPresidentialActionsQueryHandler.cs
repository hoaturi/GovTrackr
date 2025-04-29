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

        if (request.CategoryId is not null)
            query = query.Where(a => a.PresidentialAction.SubCategory.Id == request.CategoryId);

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
                a.PresidentialAction.SubCategory.Id
            ))
            .ToListAsync(cancellationToken);

        return Result.Ok(new GetPresidentialActionsResponse(actions, totalCount, request.Page));
    }
}