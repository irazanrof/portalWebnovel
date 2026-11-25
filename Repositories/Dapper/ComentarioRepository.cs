using Dapper;
using Microsoft.Data.Sqlite;
using PortalWebnovels.Models;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Repositories.Dapper;

public class ComentarioRepository : IComentarioRepository
{
    private const string Colunas = "Id, UsuarioId, CapituloId, Texto, Data, DataEdicao";

    private readonly string _connectionString;

    public ComentarioRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private SqliteConnection ObterConexao() => new SqliteConnection(_connectionString);

    public int Adicionar(Comentario entidade)
    {
        using var connection = ObterConexao();

        const string sql = @"
            INSERT INTO Comentario (UsuarioId, CapituloId, Texto, Data, DataEdicao)
            VALUES (@UsuarioId, @CapituloId, @Texto, @Data, NULL);
            SELECT last_insert_rowid();";

        return connection.ExecuteScalar<int>(sql, new
        {
            entidade.UsuarioId,
            entidade.CapituloId,
            entidade.Texto,
            Data = Fmt.Data(entidade.Data)
        });
    }

    public void Atualizar(Comentario entidade)
    {
        using var connection = ObterConexao();

        connection.Execute("UPDATE Comentario SET Texto = @Texto, DataEdicao = @DataEdicao WHERE Id = @Id;",
            new { entidade.Texto, DataEdicao = Fmt.Data(entidade.DataEdicao), entidade.Id });
    }

    public void Remover(int id)
    {
        using var connection = ObterConexao();

        connection.Execute("DELETE FROM Comentario WHERE Id = @Id;", new { Id = id });
    }

    public Comentario? ObterPorId(int id)
    {
        using var connection = ObterConexao();

        return connection.QueryFirstOrDefault<Comentario>(
            "SELECT " + Colunas + " FROM Comentario WHERE Id = @Id;", new { Id = id });
    }

    public IEnumerable<Comentario> ObterTodos()
    {
        using var connection = ObterConexao();

        return connection.Query<Comentario>("SELECT " + Colunas + " FROM Comentario ORDER BY Data;").ToList();
    }

    public IEnumerable<Comentario> ObterPorCapitulo(int capituloId)
    {
        using var connection = ObterConexao();

        return connection.Query<Comentario>(
            "SELECT " + Colunas + " FROM Comentario WHERE CapituloId = @CapituloId ORDER BY Data;",
            new { CapituloId = capituloId }).ToList();
    }
}
