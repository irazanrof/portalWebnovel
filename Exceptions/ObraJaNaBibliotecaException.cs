namespace PortalWebnovels.Exceptions;

/// A obra já está na lista (Biblioteca ou Favoritos) do usuário.
public class ObraJaNaBibliotecaException : RegistroDuplicadoException
{
    public ObraJaNaBibliotecaException(string mensagem) : base(mensagem) { }
}
