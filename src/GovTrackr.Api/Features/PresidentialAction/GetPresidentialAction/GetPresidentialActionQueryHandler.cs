﻿using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Api.Features.PresidentialAction.GetPresidentialAction;

public class GetPresidentialActionQueryHandler(AppDbContext dbContext) : IRequestHandler<GetPresidentialActionQuery,
    Result<GetPresidentialActionResponse>>
{
    public async Task<Result<GetPresidentialActionResponse>> Handle(GetPresidentialActionQuery request,
        CancellationToken cancellationToken)
    {
        var action = await dbContext.PresidentialActionTranslations.AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetPresidentialActionResponse(
                x.Id,
                x.PresidentialAction.SubCategory.Category.Id,
                x.PresidentialAction.SubCategory.Id,
                x.Title,
                x.Content,
                x.PresidentialAction.PublishedAt,
                x.Summary,
                x.PresidentialAction.SourceUrl
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return action is null
            ? Result.Fail(PresidentialActionError.PresidentialActionNotFound(request.Id))
            : Result.Ok(action);
    }
}