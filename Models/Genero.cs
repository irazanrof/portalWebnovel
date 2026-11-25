using PortalWebnovels.Exceptions;

namespace PortalWebnovels.Models;

public class Genero
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;

    protected Genero() { }

    public Genero(string nome)
    {
        Nome = ValidarNome(nome);
    }

    public void AlterarNome(string novoNome)
    {
        Nome = ValidarNome(novoNome);
    }

    private static string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do gênero é obrigatório.");

        nome = nome.Trim();

        if (nome.Length < 3 || nome.Length > 50)
            throw new DominioException("O nome do gênero deve ter entre 3 e 50 caracteres.");

        return nome;
    }
}
