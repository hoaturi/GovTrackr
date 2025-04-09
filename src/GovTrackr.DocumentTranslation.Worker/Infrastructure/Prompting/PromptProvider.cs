using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;
using Shared.Domain.Common;

namespace GovTrackr.DocumentTranslation.Worker.Infrastructure.Prompting;

internal class PromptProvider : IPromptProvider
{
    private readonly string _presidentialActionPrompt;

    public PromptProvider()
    {
        var presidentialActionPromptPath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Prompting",
            "Prompts",
            "PresidentialActionTranslationPrompt.xml");

        Console.WriteLine(presidentialActionPromptPath);

        if (!File.Exists(presidentialActionPromptPath))
            throw new FileNotFoundException("Prompt file not found", presidentialActionPromptPath);

        _presidentialActionPrompt = File.ReadAllText(presidentialActionPromptPath);
    }

    public string GetPrompt(DocumentCategoryType category)
    {
        return category switch
        {
            DocumentCategoryType.PresidentialAction => _presidentialActionPrompt,
            _ => throw new NotImplementedException($"Prompt for {category} is not implemented.")
        };
    }
}