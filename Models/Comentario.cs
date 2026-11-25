using PortalWebnovels.Exceptions;

namespace PortalWebnovels.Models;

public class Comentario : Interacao
{
    public int CapituloId { get; private set; }
    public string Texto { get; private set; } = string.Empty;
    public DateTime? DataEdicao { get; private set; }

    protected Comentario() { }   // Dapper

    public Comentario(int usuarioId, int capituloId, string texto) : base(usuarioId)
    {
        if (capituloId <= 0)
            throw new DominioException("Capítulo inválido.");
        CapituloId = capituloId;
        Texto = ValidarTexto(texto);
    }

    public override void Validar() => ValidarTexto(Texto);

    public void Editar(string texto)
    {
        Texto = ValidarTexto(texto);
        DataEdicao = DateTime.Now;
    }

    private static string ValidarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new DominioException("O comentário não pode ser vazio.");
        texto = texto.Trim();
        if (texto.Length > 1000)
            throw new DominioException("O comentário não pode passar de 1000 caracteres.");
        return texto;
    }
}
