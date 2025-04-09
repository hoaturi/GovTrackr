using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;

namespace GovTrackr.DocumentTranslation.Worker.Infrastructure.Prompting;

internal class PromptProvider : IPromptProvider
{
    public PromptProvider()
    {
        var presidentialActionPromptPath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Prompting",
            "Prompt",
            "PresidentialActionTranslationPrompt.xml");

        if (!File.Exists(presidentialActionPromptPath))
            throw new FileNotFoundException("Prompt file not found", presidentialActionPromptPath);

        PresidentialActionPrompt = File.ReadAllText(presidentialActionPromptPath);
    }

    public string PresidentialActionPrompt { get; }
}