using UrbaPF.Domain.Entities;

namespace UrbaPF.Domain.Services;

public class AlertDomainService
{
    public bool ShouldRequireApproval(int creatorReputationLevel)
    {
        return creatorReputationLevel < 4;
    }

    public bool CanApproveAlert(int userRole)
    {
        return userRole >= 3;
    }

    public bool CanAcknowledge(int userRole)
    {
        return userRole >= 3;
    }

    public bool CanResolve(int userRole, bool isCreator)
    {
        return userRole >= 3 || isCreator;
    }

    public bool CanDelete(int userRole, bool isCreator)
    {
        if (userRole >= 4) return true;
        return isCreator && userRole >= 3;
    }

    public bool CanResendNotification(int userRole, bool isCreator, int alertStatus)
    {
        if (userRole >= 3) return true;
        return isCreator && alertStatus >= 4;
    }

    public bool IsAlertActive(int status)
    {
        return status >= 1 && status <= 3;
    }

    public string GetAlertTypeName(int type)
    {
        return type switch
        {
            1 => "Emergencia",
            2 => "Robo",
            3 => "Incendio",
            4 => "Médica",
            5 => "Otro",
            _ => "Desconocido"
        };
    }

    public string GetStatusName(int status)
    {
        return status switch
        {
            1 => "Pendiente",
            2 => "Aprobada",
            3 => "Notificada",
            4 => "En Proceso",
            5 => "Resuelta",
            6 => "Cancelada",
            _ => "Desconocido"
        };
    }
}
