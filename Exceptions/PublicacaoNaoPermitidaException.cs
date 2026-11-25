namespace PortalWebnovels.Exceptions;

/// Usuário sem permissão de publicar.
public class PublicacaoNaoPermitidaException : DominioException
{
    public PublicacaoNaoPermitidaException(string mensagem) : base(mensagem) { }
}
