using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Domain.Services;

public class AlertDomainService
{
    public bool ShouldRequireApproval(int creatorReputationLevel)
    {
        return creatorReputationLevel < (int)CredibilityLevel.Trusted;
    }

    public bool CanApproveAlert(UserRole userRole)
    {
        return userRole >= UserRole.Manager;
    }

    public bool CanAcknowledge(UserRole userRole)
    {
        return userRole >= UserRole.Manager;
    }

    public bool CanResolve(UserRole userRole, bool isCreator)
    {
        return userRole >= UserRole.Manager || isCreator;
    }

    public bool CanDelete(UserRole userRole, bool isCreator)
    {
        if (userRole >= UserRole.Administrator) return true;
        return isCreator && userRole >= UserRole.Manager;
    }

    public bool CanResendNotification(UserRole userRole, bool isCreator, int alertStatus)
    {
        if (userRole >= UserRole.Manager) return true;
        return isCreator && alertStatus >= (int)AlertStatus.Resolved;
    }

    public bool IsAlertActive(int status)
    {
        return status >= (int)AlertStatus.Pending && status <= (int)AlertStatus.Notified;
    }

    public string GetAlertTypeName(int type)
    {
        return type switch
        {
            (int)AlertType.Emergency => "Emergencia",
            (int)AlertType.Robbery => "Robo",
            (int)AlertType.Fire => "Incendio",
            (int)AlertType.Medical => "Médica",
            (int)AlertType.Other => "Otro",
            _ => "Desconocido"
        };
    }

    public string GetStatusName(int status)
    {
        return status switch
        {
            (int)AlertStatus.Pending => "Pendiente",
            (int)AlertStatus.Approved => "Aprobada",
            (int)AlertStatus.Notified => "Notificada",
            (int)AlertStatus.InProgress => "En Proceso",
            (int)AlertStatus.Resolved => "Resuelta",
            (int)AlertStatus.Cancelled => "Cancelada",
            _ => "Desconocido"
        };
    }
}
