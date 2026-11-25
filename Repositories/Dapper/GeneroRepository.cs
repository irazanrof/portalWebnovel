using Dapper;
using Microsoft.Data.Sqlite;
using PortalWebnovels.Models;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Repositories.Dapper;

/// Implementação do repositório de Genero com Dapper + SQLite (R6).
public class GeneroRepository : IGeneroRepository
{
    private readonly string _connectionString;

    public GeneroRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private SqliteConnection ObterConexao() => new SqliteConnection(_connectionString);

    public int Adicionar(Genero entidade)
    {
        using var connection = ObterConexao();

        const string sql = @"
            INSERT INTO Genero (Nome) VALUES (@Nome);
            SELECT last_insert_rowid();";

        return connection.ExecuteScalar<int>(sql, new { entidade.Nome });
    }

    public void Atualizar(Genero entidade)
    {
        using var connection = ObterConexao();

        const string sql = "UPDATE Genero SET Nome = @Nome WHERE Id = @Id;";

        connection.Execute(sql, new { entidade.Nome, entidade.Id });
    }

    public void Remover(int id)
    {
        using var connection = ObterConexao();

        connection.Execute("DELETE FROM Genero WHERE Id = @Id;", new { Id = id });
    }

    public Genero? ObterPorId(int id)
    {
        using var connection = ObterConexao();

        // O Dapper liga cada coluna à propriedade de mesmo nome (Id -> Id, Nome -> Nome).
        return connection.QueryFirstOrDefault<Genero>(
            "SELECT Id, Nome FROM Genero WHERE Id = @Id;",
            new { Id = id });
    }

    public IEnumerable<Genero> ObterTodos()
    {
        using var connection = ObterConexao();

        return connection.Query<Genero>("SELECT Id, Nome FROM Genero ORDER BY Nome ASC;");
    }

    public bool ExisteComNome(string nome, int? ignorarId = null)
    {
        using var connection = ObterConexao();

        const string sql = @"
            SELECT COUNT(*) FROM Genero
            WHERE LOWER(Nome) = LOWER(@Nome)
              AND (@IgnorarId IS NULL OR Id <> @IgnorarId);";

        return connection.ExecuteScalar<int>(sql, new { Nome = nome.Trim(), IgnorarId = ignorarId }) > 0;
    }

    public bool EstaEmUso(int id)
    {
        using var connection = ObterConexao();

        const string sql = "SELECT COUNT(*) FROM WebnovelGenero WHERE GeneroId = @Id;";

        return connection.ExecuteScalar<int>(sql, new { Id = id }) > 0;
    }
}
