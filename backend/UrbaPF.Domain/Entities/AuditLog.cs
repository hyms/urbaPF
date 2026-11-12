using System;

namespace UrbaPF.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public Guid Uuid { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid ComunidadId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public Guid EntidadId { get; set; }
    public string DataJson { get; set; } = string.Empty;
    public string? HashVerificacion { get; set; }
}
