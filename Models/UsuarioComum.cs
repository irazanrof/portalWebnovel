using PortalWebnovels.Exceptions;

namespace PortalWebnovels.Models;

public class UsuarioComum : Usuario
{
    public bool PublicacaoRevogada { get; private set; }
    public string? MotivoRevogacao { get; private set; }

    public override string Tipo => "Comum";

    protected UsuarioComum() { }   // Dapper

    public UsuarioComum(string nome, string email, string senha) : base(nome, email, senha) { }

    public override bool PodePublicar() => !PublicacaoRevogada;

    // Usuário comum só apaga o próprio comentário ou comentários feitos na sua obra.
    public override bool PodeExcluirComentario(Comentario comentario, Webnovel obra)
        => comentario.UsuarioId == Id || obra.AutorId == Id;

    // 'internal': só o código do próprio projeto (o Administrador) consegue chamar.
    internal void RevogarPublicacao(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new DominioException("Informe o motivo da revogação.");
        PublicacaoRevogada = true;
        MotivoRevogacao = motivo.Trim();
    }

    internal void RestaurarPublicacao()
    {
        PublicacaoRevogada = false;
        MotivoRevogacao = null;
    }
}
