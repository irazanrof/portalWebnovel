using Dapper;
using Microsoft.Data.Sqlite;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Models;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Repositories.Dapper;

public class UsuarioNomeDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class UsuarioRepository : IUsuarioRepository
{
    private const string Colunas = "Id, Nome, Email, SenhaHash, PublicacaoRevogada, MotivoRevogacao, DataCadastro";

    private readonly string _connectionString;

    public UsuarioRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private SqliteConnection ObterConexao() => new SqliteConnection(_connectionString);

    public int Adicionar(Usuario entidade)
    {
        using var connection = ObterConexao();

        const string sql = @"
            INSERT INTO Usuario (Nome, Email, SenhaHash, Tipo, PublicacaoRevogada, MotivoRevogacao, DataCadastro)
            VALUES (@Nome, @Email, @SenhaHash, @Tipo, @PublicacaoRevogada, @MotivoRevogacao, @DataCadastro);
            SELECT last_insert_rowid();";

        var comum = entidade as UsuarioComum;
        return connection.ExecuteScalar<int>(sql, new
        {
            entidade.Nome,
            entidade.Email,
            entidade.SenhaHash,
            entidade.Tipo,
            PublicacaoRevogada = comum != null && comum.PublicacaoRevogada ? 1 : 0,
            MotivoRevogacao = comum?.MotivoRevogacao,
            DataCadastro = Fmt.Data(entidade.DataCadastro)
        });
    }

    public void Atualizar(Usuario entidade)
    {
        using var connection = ObterConexao();

        const string sql = @"
            UPDATE Usuario
               SET Nome = @Nome, PublicacaoRevogada = @PublicacaoRevogada, MotivoRevogacao = @MotivoRevogacao
             WHERE Id = @Id;";

        var comum = entidade as UsuarioComum;
        connection.Execute(sql, new
        {
            entidade.Nome,
            PublicacaoRevogada = comum != null && comum.PublicacaoRevogada ? 1 : 0,
            MotivoRevogacao = comum?.MotivoRevogacao,
            entidade.Id
        });
    }

    public void Remover(int id)
    {
        using var connection = ObterConexao();
        try
        {
            connection.Execute("DELETE FROM Usuario WHERE Id = @Id;", new { Id = id });
        }
        catch (SqliteException)
        {
            throw new DominioException("Não é possível excluir um usuário que possui obras, avaliações ou comentários.");
        }
    }

    public Usuario? ObterPorId(int id)
    {
        using var connection = ObterConexao();

        var tipo = connection.QueryFirstOrDefault<string>("SELECT Tipo FROM Usuario WHERE Id = @Id;", new { Id = id });
        if (tipo == null)
            return null;

        string sql = "SELECT " + Colunas + " FROM Usuario WHERE Id = @Id;";
        if (tipo == "Administrador")
            return connection.QueryFirstOrDefault<Administrador>(sql, new { Id = id });

        return connection.QueryFirstOrDefault<UsuarioComum>(sql, new { Id = id });
    }

    public IEnumerable<Usuario> ObterTodos()
    {
        using var connection = ObterConexao();

        var resultado = new List<Usuario>();
        resultado.AddRange(connection.Query<UsuarioComum>(
            "SELECT " + Colunas + " FROM Usuario WHERE Tipo = 'Comum';"));
        resultado.AddRange(connection.Query<Administrador>(
            "SELECT " + Colunas + " FROM Usuario WHERE Tipo = 'Administrador';"));

        return resultado.OrderBy(u => u.Nome).ToList();
    }

    public IEnumerable<Usuario> Buscar(string? termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return ObterTodos();

        string limpo = termo.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        string padrao = "%" + limpo + "%";

        using var connection = ObterConexao();

        const string where = @" WHERE Tipo = @Tipo
                                  AND (Nome LIKE @Padrao ESCAPE '\' OR Email LIKE @Padrao ESCAPE '\');";

        var resultado = new List<Usuario>();
        resultado.AddRange(connection.Query<UsuarioComum>(
            "SELECT " + Colunas + " FROM Usuario" + where, new { Tipo = "Comum", Padrao = padrao }));
        resultado.AddRange(connection.Query<Administrador>(
            "SELECT " + Colunas + " FROM Usuario" + where, new { Tipo = "Administrador", Padrao = padrao }));

        return resultado.OrderBy(u => u.Nome).ToList();
    }

    public Usuario? ObterPorEmail(string email)
    {
        int? id;
        using (var connection = ObterConexao())
        {
            id = connection.QueryFirstOrDefault<int?>(
                "SELECT Id FROM Usuario WHERE LOWER(Email) = LOWER(@Email);",
                new { Email = email.Trim() });
        }

        if (id == null)
            return null;
        return ObterPorId(id.Value);
    }

    public bool EmailEmUso(string email)
    {
        using var connection = ObterConexao();

        return connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Usuario WHERE LOWER(Email) = LOWER(@Email);",
            new { Email = email.Trim() }) > 0;
    }

    public IDictionary<int, string> ObterNomes()
    {
        using var connection = ObterConexao();

        return connection.Query<UsuarioNomeDto>("SELECT Id, Nome FROM Usuario;")
                         .ToDictionary(u => u.Id, u => u.Nome);
    }
}
