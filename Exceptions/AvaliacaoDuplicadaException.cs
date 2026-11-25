namespace PortalWebnovels.Exceptions;

/// O usuário já avaliou esta webnovel (só vale uma avaliação por usuário e obra).
public class AvaliacaoDuplicadaException : RegistroDuplicadoException
{
    public AvaliacaoDuplicadaException(string mensagem) : base(mensagem) { }
}
