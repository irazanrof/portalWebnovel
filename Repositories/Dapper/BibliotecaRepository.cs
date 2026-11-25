using Dapper;
using Microsoft.Data.Sqlite;
using PortalWebnovels.Models;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Repositories.Dapper;

public class BibliotecaRepository : IBibliotecaRepository
{
    private readonly string _connectionString;

    public BibliotecaRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private SqliteConnection ObterConexao() => new SqliteConnection(_connectionString);

    public Biblioteca ObterDoUsuario(int usuarioId)
    {
        using var connection = ObterConexao();

        // Lê só os dados básicos das obras (sem capítulos): a lista é apenas para exibir.
        var obras = connection.Query<Webnovel>(@"
            SELECT w.Id, w.Titulo, w.Sinopse, w.Status, w.AutorId, w.DataCriacao
              FROM Webnovel w
              JOIN BibliotecaItem i ON i.WebnovelId = w.Id
             WHERE i.UsuarioId = @UsuarioId
             ORDER BY i.DataAdicao DESC;", new { UsuarioId = usuarioId });

        var lista = new Biblioteca(usuarioId);
        lista.Carregar(obras);
        return lista;
    }

    public void Adicionar(int usuarioId, int webnovelId)
    {
        using var connection = ObterConexao();

        connection.Execute(@"
            INSERT INTO BibliotecaItem (UsuarioId, WebnovelId, DataAdicao)
            VALUES (@UsuarioId, @WebnovelId, @Data);",
            new { UsuarioId = usuarioId, WebnovelId = webnovelId, Data = Fmt.Data(DateTime.Now) });
    }

    public void Remover(int usuarioId, int webnovelId)
    {
        using var connection = ObterConexao();

        connection.Execute("DELETE FROM BibliotecaItem WHERE UsuarioId = @UsuarioId AND WebnovelId = @WebnovelId;",
            new { UsuarioId = usuarioId, WebnovelId = webnovelId });
    }
}
