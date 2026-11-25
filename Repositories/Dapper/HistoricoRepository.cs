using Dapper;
using Microsoft.Data.Sqlite;
using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Repositories.Dapper;

public class HistoricoRepository : IHistoricoRepository
{
    private readonly string _connectionString;

    public HistoricoRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private SqliteConnection ObterConexao() => new SqliteConnection(_connectionString);

    public HistoricoLeitura? ObterPorUsuarioEObra(int usuarioId, int webnovelId)
    {
        using var connection = ObterConexao();

        return connection.QueryFirstOrDefault<HistoricoLeitura>(@"
            SELECT Id, UsuarioId, WebnovelId, CapituloId, DataLeitura
              FROM HistoricoLeitura
             WHERE UsuarioId = @UsuarioId AND WebnovelId = @WebnovelId;",
            new { UsuarioId = usuarioId, WebnovelId = webnovelId });
    }

    public void Registrar(HistoricoLeitura historico)
    {
        using var connection = ObterConexao();

        // UPSERT: se já existe (UsuarioId, WebnovelId), só troca o capítulo e a data.
        connection.Execute(@"
            INSERT INTO HistoricoLeitura (UsuarioId, WebnovelId, CapituloId, DataLeitura)
            VALUES (@UsuarioId, @WebnovelId, @CapituloId, @DataLeitura)
            ON CONFLICT (UsuarioId, WebnovelId)
            DO UPDATE SET CapituloId = excluded.CapituloId, DataLeitura = excluded.DataLeitura;",
            new
            {
                historico.UsuarioId,
                historico.WebnovelId,
                historico.CapituloId,
                DataLeitura = Fmt.Data(historico.DataLeitura)
            });
    }

    public IEnumerable<HistoricoItemViewModel> ObterResumoDoUsuario(int usuarioId)
    {
        using var connection = ObterConexao();

        return connection.Query<HistoricoItemViewModel>(@"
            SELECT h.WebnovelId, w.Titulo AS WebnovelTitulo,
                   h.CapituloId, c.Numero AS CapituloNumero, c.Titulo AS CapituloTitulo,
                   h.DataLeitura
              FROM HistoricoLeitura h
              JOIN Webnovel w ON w.Id = h.WebnovelId
              JOIN Capitulo c ON c.Id = h.CapituloId
             WHERE h.UsuarioId = @UsuarioId
             ORDER BY h.DataLeitura DESC;", new { UsuarioId = usuarioId }).ToList();
    }
}
