using PortalWebnovels.Exceptions;

namespace PortalWebnovels.Models;

public abstract class Interacao
{
    public int Id { get; protected set; }
    public int UsuarioId { get; protected set; }
    public DateTime Data { get; protected set; }

    protected Interacao() { }   // Dapper

    protected Interacao(int usuarioId)
    {
        if (usuarioId <= 0)
            throw new DominioException("Usuário inválido.");
        UsuarioId = usuarioId;
        Data = DateTime.Now;
    }

    public abstract void Validar();
}
