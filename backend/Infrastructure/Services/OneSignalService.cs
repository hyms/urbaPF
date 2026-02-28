using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Services;

public class OneSignalService : IOneSignalService
{
    private readonly HttpClient _httpClient;
    private readonly string? _appId;
    private readonly string? _restApiKey;

    public OneSignalService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _appId = configuration["ONESIGNAL_APP_ID"];
        _restApiKey = configuration["ONESIGNAL_REST_API_KEY"];
    }

    public async Task SendToSegmentAsync(string heading, string message, string segment)
    {
        if (string.IsNullOrEmpty(_appId) || string.IsNullOrEmpty(_restApiKey))
            return;

        var request = new
        {
            app_id = _appId,
            headings = new { en = heading },
            contents = new { en = message },
            included_segments = new[] { segment }
        };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_restApiKey}");
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json");

        try
        {
            await _httpClient.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", request);
        }
        catch
        {
        }
    }

    public async Task SendToUserAsync(string heading, string message, string playerId)
    {
        if (string.IsNullOrEmpty(_appId) || string.IsNullOrEmpty(_restApiKey))
            return;

        var request = new
        {
            app_id = _appId,
            headings = new { en = heading },
            contents = new { en = message },
            include_player_ids = new[] { playerId }
        };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_restApiKey}");
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json");

        try
        {
            await _httpClient.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", request);
        }
        catch
        {
        }
    }

    public async Task SendToUsersAsync(string heading, string message, IEnumerable<string> playerIds)
    {
        if (string.IsNullOrEmpty(_appId) || string.IsNullOrEmpty(_restApiKey))
            return;

        var request = new
        {
            app_id = _appId,
            headings = new { en = heading },
            contents = new { en = message },
            include_player_ids = playerIds.ToList()
        };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_restApiKey}");
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json");

        try
        {
            await _httpClient.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", request);
        }
        catch
        {
        }
    }
}
