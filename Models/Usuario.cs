using PortalWebnovels.Exceptions;

namespace PortalWebnovels.Models;

public abstract class Usuario
{
    public int Id { get; protected set; }
    public string Nome { get; protected set; } = string.Empty;
    public string Email { get; protected set; } = string.Empty;
    public DateTime DataCadastro { get; protected set; }

    public string SenhaHash { get; protected set; } = string.Empty;

    public abstract string Tipo { get; }

    protected Usuario() { }   // usado pelo Dapper

    protected Usuario(string nome, string email, string senha)
    {
        Nome = ValidarNome(nome);
        Email = ValidarEmail(email);
        SenhaHash = PasswordHasher.Gerar(ValidarSenha(senha));
        DataCadastro = DateTime.Now;
    }

    public bool VerificarSenha(string? senha)
        => !string.IsNullOrEmpty(senha) && PasswordHasher.Verificar(senha, SenhaHash);

    public abstract bool PodePublicar();
    public abstract bool PodeExcluirComentario(Comentario comentario, Webnovel obra);

    public void AtualizarPerfil(string nome)
    {
        Nome = ValidarNome(nome);
    }

    protected static string ValidarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome é obrigatório.");
        nome = nome.Trim();
        if (nome.Length < 2 || nome.Length > 100)
            throw new DominioException("O nome deve ter entre 2 e 100 caracteres.");
        return nome;
    }

    protected static string ValidarSenha(string? senha)
    {
        if (string.IsNullOrEmpty(senha))
            throw new DominioException("A senha é obrigatória.");
        if (senha.Length < 6 || senha.Length > 100)
            throw new DominioException("A senha deve ter entre 6 e 100 caracteres.");
        return senha;
    }

    protected static string ValidarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DominioException("O e-mail é obrigatório.");
        email = email.Trim().ToLowerInvariant();
        try
        {
            var endereco = new System.Net.Mail.MailAddress(email);
            if (endereco.Address != email) throw new FormatException();
        }
        catch (FormatException)
        {
            throw new DominioException("O e-mail informado é inválido.");
        }
        return email;
    }
}
