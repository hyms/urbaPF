using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using UrbaPF.Infrastructure.Repositories;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Infrastructure.Services;

public interface IPushNotificationService
{
    Task<bool> SendNotificationToUserAsync(string userId, string title, string message, Dictionary<string, string>? data = null);
    Task<bool> NotifyEmergencyAsync(string title, string message, Guid condominiumId);
    Task<bool> NotifyIncidentAsync(string title, string message, int priority, Guid condominiumId);
}

public class PushNotificationService : IPushNotificationService
{
    private readonly IUserRepository _userRepository;
    private readonly bool _isFirebaseConfigured;

    public PushNotificationService(
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _isFirebaseConfigured = FirebaseMessaging.DefaultInstance != null;
    }

    public async Task<bool> SendNotificationToUserAsync(string userId, string title, string message, Dictionary<string, string>? data = null)
    {
        if (!_isFirebaseConfigured)
        {
            Console.WriteLine("[Push] Firebase not configured. Notification skipped.");
            return false;
        }

        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null || string.IsNullOrEmpty(user.FcmToken))
            {
                Console.WriteLine($"[Push] User {userId} has no FCM token.");
                return false;
            }

            var notification = new Notification
            {
                Title = title,
                Body = message
            };

            var messageData = new Dictionary<string, string>();
            if (data != null)
            {
                foreach (var kvp in data)
                {
                    messageData[kvp.Key] = kvp.Value;
                }
            }
            messageData["click_action"] = "OPEN_APP";

            var fcmMessage = new Message
            {
                Notification = notification,
                Token = user.FcmToken,
                Data = messageData
            };

            var result = await FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage);
            Console.WriteLine($"[Push] Notification sent to user {userId}: {result}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Push] Error sending to user {userId}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> NotifyEmergencyAsync(string title, string message, Guid condominiumId)
    {
        if (!_isFirebaseConfigured)
        {
            Console.WriteLine("[Push] Firebase not configured. Emergency notification skipped.");
            return false;
        }

        var users = await _userRepository.GetByCondominiumAsync(condominiumId);
        
        var tokens = users
            .Where(u => !string.IsNullOrEmpty(u.FcmToken))
            .Select(u => u.FcmToken!)
            .Distinct()
            .ToList();

        if (tokens.Count == 0)
        {
            Console.WriteLine("[Push] No FCM tokens found for emergency notification.");
            return false;
        }

        try
        {
            var notification = new Notification
            {
                Title = $"🚨 {title}",
                Body = message
            };

            var multicastMessage = new MulticastMessage
            {
                Notification = notification,
                Tokens = tokens,
                Data = new Dictionary<string, string>
                {
                    { "type", "emergency" },
                    { "title", title },
                    { "click_action", "OPEN_APP" }
                }
            };

            var result = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicastMessage);
            Console.WriteLine($"[Push] Emergency notification sent to {result.SuccessCount}/{tokens.Count} devices.");
            return result.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Push] Error sending emergency notification: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> NotifyIncidentAsync(string title, string message, int priority, Guid condominiumId)
    {
        if (priority < 3)
        {
            return false;
        }

        if (!_isFirebaseConfigured)
        {
            Console.WriteLine("[Push] Firebase not configured. Incident notification skipped.");
            return false;
        }

        var users = await _userRepository.GetByCondominiumAsync(condominiumId);
        
        var tokens = users
            .Where(u => !string.IsNullOrEmpty(u.FcmToken) && u.Role >= UserRole.Manager)
            .Select(u => u.FcmToken!)
            .Distinct()
            .ToList();

        if (tokens.Count == 0)
        {
            return false;
        }

        try
        {
            var notification = new Notification
            {
                Title = $"⚠️ {title}",
                Body = message
            };

            var multicastMessage = new MulticastMessage
            {
                Notification = notification,
                Tokens = tokens,
                Data = new Dictionary<string, string>
                {
                    { "type", "incident" },
                    { "priority", priority.ToString() },
                    { "click_action", "OPEN_APP" }
                }
            };

            var result = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicastMessage);
            Console.WriteLine($"[Push] Incident notification sent to {result.SuccessCount}/{tokens.Count} managers.");
            return result.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Push] Error sending incident notification: {ex.Message}");
            return false;
        }
    }
}