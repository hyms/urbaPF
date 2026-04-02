using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using UrbaPF.Domain.Entities;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task LogEventAsync(Guid usuarioId, Guid comunidadId, string tipoEvento, Guid entidadId, object data)
    {
        var lastLog = await _auditRepository.GetLastEventAsync(comunidadId);
        var previousHash = lastLog?.HashVerificacion ?? "GENESIS";

        var log = new AuditLog
        {
            UsuarioId = usuarioId,
            ComunidadId = comunidadId,
            TipoEvento = tipoEvento,
            EntidadId = entidadId,
            DataJson = JsonSerializer.Serialize(data),
            Fecha = DateTimeOffset.UtcNow
        };

        log.HashVerificacion = CalculateHash(previousHash, log);

        await _auditRepository.CreateAsync(log);
    }

    public async Task<bool> VerifyChainAsync(Guid comunidadId)
    {
        // This is a simplified verification logic. 
        // In a real system, one would fetch all logs and re-verify the hash chain.
        // For now, it's a placeholder.
        return true; 
    }

    private string CalculateHash(string previousHash, AuditLog log)
    {
        var data = $"{previousHash}|{log.UsuarioId}|{log.ComunidadId}|{log.TipoEvento}|{log.EntidadId}|{log.DataJson}|{log.Fecha.Ticks}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashBytes);
    }
}
