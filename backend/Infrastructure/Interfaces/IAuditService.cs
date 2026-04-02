using System.Threading.Tasks;
using UrbaPF.Domain.Entities;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IAuditService
{
    Task LogEventAsync(Guid usuarioId, Guid comunidadId, string tipoEvento, Guid entidadId, object data);
    Task<bool> VerifyChainAsync(Guid comunidadId);
}
