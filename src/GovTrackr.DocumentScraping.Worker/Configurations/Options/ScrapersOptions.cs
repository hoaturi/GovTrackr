﻿using System.ComponentModel.DataAnnotations;

namespace GovTrackr.DocumentScraping.Worker.Configurations.Options;

internal class ScrapersOptions
{
    internal const string SectionName = "Scrapers";

    [Required] public int MaxConcurrentPages { get; init; } = 5;
}