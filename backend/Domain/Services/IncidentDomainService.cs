using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Domain.Services;

public class IncidentDomainService
{
    public int CalculatePriority(int incidentType, string? description)
    {
        if (string.IsNullOrEmpty(description))
            return (int)IncidentPriority.Medium;

        var lowerDesc = description.ToLowerInvariant();

        if (incidentType == (int)IncidentType.Security)
        {
            if (lowerDesc.Contains("robo") || lowerDesc.Contains("asalto") || lowerDesc.Contains("emergencia"))
                return (int)IncidentPriority.Urgent;
            if (lowerDesc.Contains("sospechoso") || lowerDesc.Contains("intento"))
                return (int)IncidentPriority.High;
        }

        if (incidentType == (int)IncidentType.Maintenance)
        {
            if (lowerDesc.Contains("fuga") || lowerDesc.Contains("cortocircuito") || lowerDesc.Contains("gas"))
                return (int)IncidentPriority.Urgent;
            if (lowerDesc.Contains("agua") || lowerDesc.Contains("grieta"))
                return (int)IncidentPriority.High;
        }

        if (lowerDesc.Contains("urgente") || lowerDesc.Contains("inmediato"))
            return (int)IncidentPriority.Urgent;
        if (lowerDesc.Contains("importante") || lowerDesc.Contains("pronto"))
            return (int)IncidentPriority.High;

        return (int)IncidentPriority.Medium;
    }

    public bool CanChangeStatus(int currentStatus, int newStatus, UserRole userRole)
    {
        if (userRole >= UserRole.Administrator)
            return true;

        if (newStatus == (int)IncidentStatus.Reported)
            return false;

        if (newStatus == (int)IncidentStatus.Cancelled)
            return userRole >= UserRole.Manager;

        if (currentStatus == (int)IncidentStatus.Resolved && newStatus == (int)IncidentStatus.Closed)
            return true;

        if (newStatus == (int)IncidentStatus.InProgress || newStatus == (int)IncidentStatus.Pending)
            return userRole >= UserRole.Manager;

        return false;
    }

    public bool CanClose(int currentStatus, UserRole userRole, bool isReporter)
    {
        if (userRole >= UserRole.Administrator)
            return true;

        if (currentStatus != (int)IncidentStatus.Resolved)
            return false;

        return isReporter || userRole >= UserRole.Manager;
    }

    public bool CanEdit(Incident incident, UserRole userRole, bool isReporter)
    {
        if (incident.Status >= (int)IncidentStatus.Resolved)
            return false;

        if (userRole >= UserRole.Administrator)
            return true;

        return isReporter && incident.Status == (int)IncidentStatus.Reported;
    }

    public List<IncidentMedia> ParseMedia(string? mediaJson)
    {
        if (string.IsNullOrEmpty(mediaJson))
            return new List<IncidentMedia>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<IncidentMedia>>(mediaJson) 
                   ?? new List<IncidentMedia>();
        }
        catch
        {
            return new List<IncidentMedia>();
        }
    }

    public string SerializeMedia(List<IncidentMedia> media)
    {
        return System.Text.Json.JsonSerializer.Serialize(media);
    }
}
