using System.Net;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using GovTrackr.Digest.Functions.Application.Interfaces;
using GovTrackr.Digest.Functions.Configurations.Options;
using Microsoft.Extensions.Options;

namespace GovTrackr.Digest.Functions.Infrastructure.Email;

public class EmailService(AmazonSimpleEmailServiceV2Client sesClient, IOptions<EmailOptions> emailOptions)
    : IEmailService
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string htmlBody,
        CancellationToken cancellationToken)
    {
        var request = CreateEmailRequest(recipientEmail, subject, htmlBody);
        var response = await sesClient.SendEmailAsync(request, cancellationToken);

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    private SendEmailRequest CreateEmailRequest(string recipientEmail, string subject, string htmlBody)
    {
        return new SendEmailRequest
        {
            FromEmailAddress = _emailOptions.SenderEmail,
            Destination = new Destination
            {
                ToAddresses = [recipientEmail]
            },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = subject },
                    Body = new Body
                    {
                        Html = new Content { Data = htmlBody }
                    }
                }
            }
        };
    }
}