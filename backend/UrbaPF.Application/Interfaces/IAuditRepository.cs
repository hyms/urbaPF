using System.Threading.Tasks;
using UrbaPF.Domain.Entities;

namespace UrbaPF.Application.Interfaces;

public interface IAuditRepository
{
    Task CreateAsync(AuditLog auditLog);
    Task<AuditLog?> GetLastEventAsync(Guid comunidadId);
}
