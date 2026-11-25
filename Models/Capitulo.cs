using PortalWebnovels.Exceptions;

namespace PortalWebnovels.Models;

public class Capitulo
{
    public int Id { get; private set; }
    public int WebnovelId { get; private set; }
    public int Numero { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Conteudo { get; private set; } = string.Empty;
    public DateTime DataPublicacao { get; private set; }
    public DateTime? DataModificacao { get; private set; }

    protected Capitulo() { }   // Dapper

    internal Capitulo(int numero, string titulo, string conteudo)
    {
        if (numero < 0)
            throw new DominioException("O número do capítulo não pode ser negativo.");
        Numero = numero;
        Titulo = ValidarTitulo(titulo);
        Conteudo = ValidarConteudo(conteudo);
        DataPublicacao = DateTime.Now;   // gerada pelo sistema
    }

    public void Editar(string titulo, string conteudo)
    {
        string novoTitulo = ValidarTitulo(titulo);
        string novoConteudo = ValidarConteudo(conteudo);
        Titulo = novoTitulo;
        Conteudo = novoConteudo;
        DataModificacao = DateTime.Now; 
    }

    // só a Webnovel renumera capítulos (ao excluir um, os seguintes descem uma posição).
    internal void DecrementarNumero()
    {
        if (Numero <= 0)
            throw new DominioException("O número do capítulo não pode ser negativo.");
        Numero--;
    }

    private static string ValidarTitulo(string? titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DominioException("O capítulo precisa de um título.");
        titulo = titulo.Trim();
        if (titulo.Length > 150)
            throw new DominioException("O título do capítulo pode ter no máximo 150 caracteres.");
        return titulo;
    }

    private static string ValidarConteudo(string? conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new DominioException("O capítulo precisa de um corpo de texto.");
        return conteudo.Trim();
    }
}
