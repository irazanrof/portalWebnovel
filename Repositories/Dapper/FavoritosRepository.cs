using Dapper;
using Microsoft.Data.Sqlite;
using PortalWebnovels.Models;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Repositories.Dapper;

public class FavoritosRepository : IFavoritosRepository
{
    private readonly string _connectionString;

    public FavoritosRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private SqliteConnection ObterConexao() => new SqliteConnection(_connectionString);

    public Favoritos ObterDoUsuario(int usuarioId)
    {
        using var connection = ObterConexao();

        // Lê só os dados básicos das obras (sem capítulos): a lista é apenas para exibir.
        var obras = connection.Query<Webnovel>(@"
            SELECT w.Id, w.Titulo, w.Sinopse, w.Status, w.AutorId, w.DataCriacao
              FROM Webnovel w
              JOIN Favorito i ON i.WebnovelId = w.Id
             WHERE i.UsuarioId = @UsuarioId
             ORDER BY i.DataFavoritado DESC;", new { UsuarioId = usuarioId });

        var lista = new Favoritos(usuarioId);
        lista.Carregar(obras);
        return lista;
    }

    public void Adicionar(int usuarioId, int webnovelId)
    {
        using var connection = ObterConexao();

        connection.Execute(@"
            INSERT INTO Favorito (UsuarioId, WebnovelId, DataFavoritado)
            VALUES (@UsuarioId, @WebnovelId, @Data);",
            new { UsuarioId = usuarioId, WebnovelId = webnovelId, Data = Fmt.Data(DateTime.Now) });
    }

    public void Remover(int usuarioId, int webnovelId)
    {
        using var connection = ObterConexao();

        connection.Execute("DELETE FROM Favorito WHERE UsuarioId = @UsuarioId AND WebnovelId = @WebnovelId;",
            new { UsuarioId = usuarioId, WebnovelId = webnovelId });
    }
}
