namespace PortalWebnovels.Models;

/// Gerencia gêneros, remove qualquer comentário e revoga/restaura a permissão de publicar.
public class Administrador : Usuario
{
    public override string Tipo => "Administrador";

    protected Administrador() { }   // Dapper

    public Administrador(string nome, string email, string senha) : base(nome, email, senha) { }

    public override bool PodePublicar() => true;

    public override bool PodeExcluirComentario(Comentario comentario, Webnovel obra) => true;

    public void RevogarPublicacaoDe(UsuarioComum alvo, string motivo) => alvo.RevogarPublicacao(motivo);

    public void RestaurarPublicacaoDe(UsuarioComum alvo) => alvo.RestaurarPublicacao();
}
