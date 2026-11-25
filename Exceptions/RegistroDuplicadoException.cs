namespace PortalWebnovels.Exceptions;

public class RegistroDuplicadoException : DominioException
{
    public RegistroDuplicadoException(string mensagem) : base(mensagem) { }
}
