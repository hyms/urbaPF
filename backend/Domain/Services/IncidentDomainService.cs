using UrbaPF.Domain.Entities;

namespace UrbaPF.Domain.Services;

public class IncidentDomainService
{
    public int CalculatePriority(int incidentType, string? description)
    {
        if (string.IsNullOrEmpty(description))
            return 2;

        var lowerDesc = description.ToLowerInvariant();

        if (incidentType == 2)
        {
            if (lowerDesc.Contains("robo") || lowerDesc.Contains("asalto") || lowerDesc.Contains("emergencia"))
                return 4;
            if (lowerDesc.Contains("sospechoso") || lowerDesc.Contains("intento"))
                return 3;
        }

        if (incidentType == 1)
        {
            if (lowerDesc.Contains("fuga") || lowerDesc.Contains("cortocircuito") || lowerDesc.Contains("gas"))
                return 4;
            if (lowerDesc.Contains("agua") || lowerDesc.Contains("grieta"))
                return 3;
        }

        if (lowerDesc.Contains("urgente") || lowerDesc.Contains("inmediato"))
            return 4;
        if (lowerDesc.Contains("importante") || lowerDesc.Contains("pronto"))
            return 3;

        return 2;
    }

    public bool CanChangeStatus(int currentStatus, int newStatus, int userRole)
    {
        if (userRole >= 4)
            return true;

        if (newStatus == 1)
            return false;

        if (newStatus == 6)
            return userRole >= 3;

        if (currentStatus == 4 && newStatus == 5)
            return true;

        if (newStatus == 2 || newStatus == 3)
            return userRole >= 3;

        return false;
    }

    public bool CanClose(int currentStatus, int userRole, bool isReporter)
    {
        if (userRole >= 4)
            return true;

        if (currentStatus != 4)
            return false;

        return isReporter || userRole >= 3;
    }

    public bool CanEdit(Incident incident, int userRole, bool isReporter)
    {
        if (incident.Status >= 4)
            return false;

        if (userRole >= 4)
            return true;

        return isReporter && incident.Status == 1;
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
