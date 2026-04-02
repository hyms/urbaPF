using System.Threading.Tasks;
using UrbaPF.Domain.Entities;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IAuditRepository
{
    Task CreateAsync(AuditLog auditLog);
    Task<AuditLog?> GetLastEventAsync(Guid comunidadId);
}
