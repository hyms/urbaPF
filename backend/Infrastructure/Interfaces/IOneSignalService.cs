namespace UrbaPF.Infrastructure.Interfaces;

public interface IOneSignalService
{
    Task SendToSegmentAsync(string heading, string message, string segment);
    Task SendToUserAsync(string heading, string message, string playerId);
    Task SendToUsersAsync(string heading, string message, IEnumerable<string> playerIds);
}
