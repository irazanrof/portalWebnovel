using PortalWebnovels.Exceptions;

namespace PortalWebnovels.Models;

public class HistoricoLeitura
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public int WebnovelId { get; private set; }
    public int CapituloId { get; private set; }
    public DateTime DataLeitura { get; private set; }

    protected HistoricoLeitura() { }   // Dapper

    public HistoricoLeitura(int usuarioId, Webnovel obra, Capitulo capitulo)
    {
        if (usuarioId <= 0) throw new DominioException("Usuário inválido.");
        Validar(obra, capitulo);
        UsuarioId = usuarioId;
        WebnovelId = obra.Id;
        CapituloId = capitulo.Id;
        DataLeitura = DateTime.Now;
    }

    public void AtualizarLeitura(Webnovel obra, Capitulo capitulo)
    {
        Validar(obra, capitulo);
        if (obra.Id != WebnovelId)
            throw new DominioException("Este histórico pertence a outra obra.");
        CapituloId = capitulo.Id;
        DataLeitura = DateTime.Now;
    }

    private static void Validar(Webnovel obra, Capitulo capitulo)
    {
        if (obra == null || capitulo == null)
            throw new DominioException("Obra e capítulo são obrigatórios.");
        if (capitulo.WebnovelId != obra.Id)
            throw new DominioException("O capítulo lido não pertence a esta webnovel.");
    }
}
