using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using UrbaPF.Infrastructure.Repositories;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Services;

public interface IPushNotificationService
{
    Task<bool> SendNotificationToUserAsync(string userId, string title, string message, Dictionary<string, string>? data = null);
    Task<bool> SendNotificationToSegmentAsync(string segment, string title, string message, Dictionary<string, string>? data = null);
    Task<bool> NotifyEmergencyAsync(string title, string message, Guid condominiumId);
    Task<bool> NotifyIncidentAsync(string title, string message, int priority, Guid condominiumId);
}

public class PushNotificationService : IPushNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUserRepository _userRepository;
    private readonly string? _oneSignalAppId;
    private readonly string? _oneSignalApiKey;
    private readonly string? _oneSignalRestApiKey;

    public PushNotificationService(
        IHttpClientFactory httpClientFactory,
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _userRepository = userRepository;
        
        _oneSignalAppId = configuration["ONESIGNAL_APP_ID"];
        _oneSignalApiKey = configuration["ONESIGNAL_API_KEY"];
        _oneSignalRestApiKey = configuration["ONESIGNAL_REST_API_KEY"];
    }

    public async Task<bool> SendNotificationToUserAsync(string userId, string title, string message, Dictionary<string, string>? data = null)
    {
        if (string.IsNullOrEmpty(_oneSignalAppId) || string.IsNullOrEmpty(_oneSignalRestApiKey))
        {
            Console.WriteLine("[Push] OneSignal not configured. Notification skipped.");
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

            var payload = new
            {
                app_id = _oneSignalAppId,
                contents = new { en = message },
                headings = new { en = title },
                include_player_ids = new[] { user.FcmToken },
                data = data ?? new Dictionary<string, string>(),
                priority = 10
            };

            return await SendToOneSignal(payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Push] Error sending to user {userId}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendNotificationToSegmentAsync(string segment, string title, string message, Dictionary<string, string>? data = null)
    {
        if (string.IsNullOrEmpty(_oneSignalAppId) || string.IsNullOrEmpty(_oneSignalRestApiKey))
        {
            Console.WriteLine("[Push] OneSignal not configured. Notification skipped.");
            return false;
        }

        try
        {
            var payload = new
            {
                app_id = _oneSignalAppId,
                contents = new { en = message },
                headings = new { en = title },
                included_segments = new[] { segment },
                data = data ?? new Dictionary<string, string>(),
                priority = 10
            };

            return await SendToOneSignal(payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Push] Error sending to segment {segment}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> NotifyEmergencyAsync(string title, string message, Guid condominiumId)
    {
        var users = await _userRepository.GetByCondominiumAsync(condominiumId);
        
        var playerIds = users
            .Where(u => !string.IsNullOrEmpty(u.FcmToken))
            .Select(u => u.FcmToken!)
            .Distinct()
            .ToList();

        if (playerIds.Count == 0)
        {
            Console.WriteLine("[Push] No FCM tokens found for emergency notification.");
            return false;
        }

        if (string.IsNullOrEmpty(_oneSignalAppId) || string.IsNullOrEmpty(_oneSignalRestApiKey))
        {
            Console.WriteLine("[Push] OneSignal not configured. Emergency notification skipped.");
            return false;
        }

        try
        {
            var payload = new
            {
                app_id = _oneSignalAppId,
                contents = new { en = message },
                headings = new { en = $"🚨 {title}" },
                include_player_ids = playerIds,
                data = new Dictionary<string, string>
                {
                    { "type", "emergency" },
                    { "title", title }
                },
                priority = 10,
                ttl = 3600
            };

            return await SendToOneSignal(payload);
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

        var users = await _userRepository.GetByCondominiumAsync(condominiumId);
        
        var playerIds = users
            .Where(u => !string.IsNullOrEmpty(u.FcmToken) && u.Role >= 3)
            .Select(u => u.FcmToken!)
            .Distinct()
            .ToList();

        if (playerIds.Count == 0)
        {
            return false;
        }

        if (string.IsNullOrEmpty(_oneSignalAppId) || string.IsNullOrEmpty(_oneSignalRestApiKey))
        {
            Console.WriteLine("[Push] OneSignal not configured. Incident notification skipped.");
            return false;
        }

        try
        {
            var payload = new
            {
                app_id = _oneSignalAppId,
                contents = new { en = message },
                headings = new { en = $"⚠️ {title}" },
                include_player_ids = playerIds,
                data = new Dictionary<string, string>
                {
                    { "type", "incident" },
                    { "priority", priority.ToString() }
                },
                priority = priority == 4 ? 10 : 5
            };

            return await SendToOneSignal(payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Push] Error sending incident notification: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> SendToOneSignal(object payload)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {_oneSignalRestApiKey}");
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");

            var response = await client.PostAsJsonAsync(
                "https://onesignal.com/api/v1/notifications",
                payload);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Push] Notification sent successfully.");
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[Push] OneSignal error: {errorContent}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Push] Exception: {ex.Message}");
            return false;
        }
    }
}
