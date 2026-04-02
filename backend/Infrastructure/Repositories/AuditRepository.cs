using Dapper;
using UrbaPF.Domain.Entities;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class AuditRepository : BaseRepository, IAuditRepository
{
    public AuditRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task CreateAsync(AuditLog auditLog)
    {
        var sql = @"
            INSERT INTO logs_eventos (uuid, fecha, usuario_id, comunidad_id, tipo_evento, entidad_id, data_json, hash_verificacion)
            VALUES (gen_random_uuid(), @Fecha, @UsuarioId, @ComunidadId, @TipoEvento, @EntidadId, @DataJson, @HashVerificacion)";
        await ExecuteAsync(sql, auditLog);
    }

    public async Task<AuditLog?> GetLastEventAsync(Guid comunidadId)
    {
        var sql = @"
            SELECT * FROM logs_eventos
            WHERE comunidad_id = @ComunidadId
            ORDER BY id DESC
            LIMIT 1";
        return await QueryFirstOrDefaultAsync<AuditLog>(sql, new { ComunidadId = comunidadId });
    }
}
