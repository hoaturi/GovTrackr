using FirebaseAdmin.Messaging;
using GovTrackr.Notification.Functions.Application.Dtos;
using GovTrackr.Notification.Functions.Application.Interfaces;

namespace GovTrackr.Notification.Functions.Infrastructure.Firebase;

public class FcmService(FirebaseMessaging firebaseMessaging) : IPushService
{
    public async Task NotifyAsync(NotificationDto dto, CancellationToken cancellationToken)
    {
        var message = new Message
        {
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = dto.Title,
                Body = dto.Message
            }
        };

        try
        {
            var result = await firebaseMessaging.SendAsync(message, cancellationToken);
            // You can optionally log the message ID
            Console.WriteLine($"Firebase topic notification sent. Message ID: {result}");
        }
        catch (FirebaseMessagingException ex)
        {
            Console.WriteLine($"Error sending Firebase topic notification: {ex.Message}");
            throw; // Rethrow or handle as needed
        }
    }
}