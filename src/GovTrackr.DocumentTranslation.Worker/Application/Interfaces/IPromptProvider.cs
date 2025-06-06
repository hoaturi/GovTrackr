﻿using Shared.Domain.Common;

namespace GovTrackr.DocumentTranslation.Worker.Application.Interfaces;

public interface IPromptProvider
{
    string GetPrompt(DocumentCategoryType category);
}