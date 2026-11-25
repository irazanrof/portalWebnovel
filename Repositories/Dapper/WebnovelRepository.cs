using Dapper;
using Microsoft.Data.Sqlite;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Repositories.Dapper;

public class WebnovelRepository : IWebnovelRepository
{
    private const int SqliteConstraint = 19;

    private readonly string _connectionString;

    public WebnovelRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private SqliteConnection ObterConexao() => new SqliteConnection(_connectionString);

    // CREATE ------------------------------------------------------------------ 
    public int Adicionar(Webnovel entidade)
    {
        using var connection = ObterConexao();
        connection.Open();
        using var transacao = connection.BeginTransaction();

        try
        {
            const string sqlObra = @"
                INSERT INTO Webnovel (Titulo, Sinopse, Status, AutorId, DataCriacao)
                VALUES (@Titulo, @Sinopse, @Status, @AutorId, @DataCriacao);
                SELECT last_insert_rowid();";

            int id = connection.ExecuteScalar<int>(sqlObra, new
            {
                entidade.Titulo,
                entidade.Sinopse,
                Status = (int)entidade.Status,
                entidade.AutorId,
                DataCriacao = Fmt.Data(entidade.DataCriacao)
            }, transacao);

            foreach (var genero in entidade.Generos)
            {
                connection.Execute(
                    "INSERT INTO WebnovelGenero (WebnovelId, GeneroId) VALUES (@WebnovelId, @GeneroId);",
                    new { WebnovelId = id, GeneroId = genero.Id }, transacao);
            }

            foreach (var capitulo in entidade.Capitulos)
                InserirCapitulo(connection, transacao, id, capitulo);

            transacao.Commit();
            return id;
        }
        catch
        {
            transacao.Rollback();
            throw;   // preserva o rastro original (R4)
        }
    }

    // UPDATE ------------------------------------------------------------------ 
    public void Atualizar(Webnovel entidade)
    {
        using var connection = ObterConexao();
        connection.Open();
        using var transacao = connection.BeginTransaction();

        try
        {
            connection.Execute(
                "UPDATE Webnovel SET Titulo = @Titulo, Sinopse = @Sinopse, Status = @Status WHERE Id = @Id;",
                new { entidade.Titulo, entidade.Sinopse, Status = (int)entidade.Status, entidade.Id }, transacao);

            connection.Execute("DELETE FROM WebnovelGenero WHERE WebnovelId = @Id;", new { entidade.Id }, transacao);
            foreach (var genero in entidade.Generos)
            {
                connection.Execute(
                    "INSERT INTO WebnovelGenero (WebnovelId, GeneroId) VALUES (@WebnovelId, @GeneroId);",
                    new { WebnovelId = entidade.Id, GeneroId = genero.Id }, transacao);
            }

            transacao.Commit();
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    // DELETE ------------------------------------------------------------------ 
    public void Remover(int id)
    {
        using var connection = ObterConexao();

        connection.Execute("DELETE FROM Webnovel WHERE Id = @Id;", new { Id = id });
    }

    // READ ------------------------------------------------------------------ 
    public Webnovel? ObterPorId(int id)
    {
        using var connection = ObterConexao();

        var obra = connection.QueryFirstOrDefault<Webnovel>(
            "SELECT Id, Titulo, Sinopse, Status, AutorId, DataCriacao FROM Webnovel WHERE Id = @Id;",
            new { Id = id });

        if (obra == null)
            return null;

        var generos = connection.Query<Genero>(@"
            SELECT g.Id, g.Nome
              FROM Genero g
              JOIN WebnovelGenero wg ON wg.GeneroId = g.Id
             WHERE wg.WebnovelId = @Id
             ORDER BY g.Nome;", new { Id = id });

        var capitulos = connection.Query<Capitulo>(@"
            SELECT Id, WebnovelId, Numero, Titulo, Conteudo, DataPublicacao, DataModificacao
              FROM Capitulo
             WHERE WebnovelId = @Id
             ORDER BY Numero;", new { Id = id });

        var avaliacoes = connection.Query<Avaliacao>(@"
            SELECT Id, UsuarioId, Nota, Texto, Data
              FROM Avaliacao
             WHERE WebnovelId = @Id
             ORDER BY Data DESC;", new { Id = id });

        obra.CarregarGeneros(generos);
        obra.CarregarCapitulos(capitulos);
        obra.CarregarAvaliacoes(avaliacoes);
        return obra;
    }

    public IEnumerable<Webnovel> ObterTodos()
    {
        List<int> ids;
        using (var connection = ObterConexao())
        {
            ids = connection.Query<int>("SELECT Id FROM Webnovel ORDER BY Titulo;").ToList();
        }

        var obras = new List<Webnovel>();
        foreach (int id in ids)
        {
            var obra = ObterPorId(id);
            if (obra != null)
                obras.Add(obra);
        }
        return obras;
    }

    public Webnovel? ObterPorCapituloId(int capituloId)
    {
        int? obraId;
        using (var connection = ObterConexao())
        {
            obraId = connection.QueryFirstOrDefault<int?>(
                "SELECT WebnovelId FROM Capitulo WHERE Id = @Id;", new { Id = capituloId });
        }

        if (obraId == null)
            return null;
        return ObterPorId(obraId.Value);
    }

    public IEnumerable<WebnovelResumoViewModel> ObterResumos(string? busca, int? generoId, int? autorId)
    {
        using var connection = ObterConexao();

        const string sql = @"
            SELECT w.Id, w.Titulo, w.Sinopse, w.Status, w.AutorId,
                   u.Nome AS AutorNome,
                   COALESCE((SELECT GROUP_CONCAT(g.Nome, ', ')
                               FROM WebnovelGenero wg
                               JOIN Genero g ON g.Id = wg.GeneroId
                              WHERE wg.WebnovelId = w.Id), '') AS Generos,
                   (SELECT COUNT(*) FROM Capitulo c WHERE c.WebnovelId = w.Id) AS TotalCapitulos,
                   (SELECT AVG(a.Nota) FROM Avaliacao a WHERE a.WebnovelId = w.Id) AS Media,
                   (SELECT COUNT(*) FROM Avaliacao a WHERE a.WebnovelId = w.Id) AS TotalAvaliacoes
              FROM Webnovel w
              JOIN Usuario u ON u.Id = w.AutorId
             WHERE (@Busca IS NULL OR w.Titulo LIKE '%' || @Busca || '%')
               AND (@GeneroId IS NULL OR EXISTS (SELECT 1 FROM WebnovelGenero x
                                                  WHERE x.WebnovelId = w.Id AND x.GeneroId = @GeneroId))
               AND (@AutorId IS NULL OR w.AutorId = @AutorId)
             ORDER BY w.Titulo;";

        string? buscaLimpa = string.IsNullOrWhiteSpace(busca) ? null : busca.Trim();

        return connection.Query<WebnovelResumoViewModel>(sql,
            new { Busca = buscaLimpa, GeneroId = generoId, AutorId = autorId }).ToList();
    }

    // CAPÍTULOS ------------------------------------------------------------------ 
    public int AdicionarCapitulo(int webnovelId, Capitulo capitulo)
    {
        using var connection = ObterConexao();
        try
        {
            return InserirCapitulo(connection, null, webnovelId, capitulo);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SqliteConstraint)
        {
            throw new RegistroDuplicadoException("Já existe um capítulo com esse número nesta obra.");
        }
    }

    public void AtualizarCapitulo(Capitulo capitulo)
    {
        using var connection = ObterConexao();

        connection.Execute(@"
            UPDATE Capitulo
               SET Titulo = @Titulo, Conteudo = @Conteudo, DataModificacao = @DataModificacao
             WHERE Id = @Id;",
            new { capitulo.Titulo, capitulo.Conteudo, DataModificacao = Fmt.Data(capitulo.DataModificacao), capitulo.Id });
    }

    public void RemoverCapitulo(int capituloId, IEnumerable<Capitulo> renumerados)
    {
        using var connection = ObterConexao();
        connection.Open();
        using var transacao = connection.BeginTransaction();

        try
        {
            connection.Execute("DELETE FROM Capitulo WHERE Id = @Id;", new { Id = capituloId }, transacao);

            foreach (var capitulo in renumerados)
            {
                connection.Execute(
                    "UPDATE Capitulo SET Numero = @Numero WHERE Id = @Id;",
                    new { capitulo.Numero, capitulo.Id }, transacao);
            }

            transacao.Commit();
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    private static int InserirCapitulo(SqliteConnection connection, SqliteTransaction? transacao,
                                       int webnovelId, Capitulo capitulo)
    {
        const string sql = @"
            INSERT INTO Capitulo (WebnovelId, Numero, Titulo, Conteudo, DataPublicacao, DataModificacao)
            VALUES (@WebnovelId, @Numero, @Titulo, @Conteudo, @DataPublicacao, @DataModificacao);
            SELECT last_insert_rowid();";

        return connection.ExecuteScalar<int>(sql, new
        {
            WebnovelId = webnovelId,
            capitulo.Numero,
            capitulo.Titulo,
            capitulo.Conteudo,
            DataPublicacao = Fmt.Data(capitulo.DataPublicacao),
            DataModificacao = Fmt.Data(capitulo.DataModificacao)
        }, transacao);
    }

    // AVALIAÇÕES ------------------------------------------------------------------ 
    public int AdicionarAvaliacao(int webnovelId, Avaliacao avaliacao)
    {
        using var connection = ObterConexao();
        try
        {
            return connection.ExecuteScalar<int>(@"
                INSERT INTO Avaliacao (UsuarioId, WebnovelId, Nota, Texto, Data)
                VALUES (@UsuarioId, @WebnovelId, @Nota, @Texto, @Data);
                SELECT last_insert_rowid();",
                new
                {
                    avaliacao.UsuarioId,
                    WebnovelId = webnovelId,
                    avaliacao.Nota,
                    avaliacao.Texto,
                    Data = Fmt.Data(avaliacao.Data)
                });
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SqliteConstraint)
        {
            throw new AvaliacaoDuplicadaException("Você já avaliou esta webnovel.");
        }
    }

    public void AtualizarAvaliacao(Avaliacao avaliacao)
    {
        using var connection = ObterConexao();

        connection.Execute("UPDATE Avaliacao SET Nota = @Nota, Texto = @Texto WHERE Id = @Id;",
            new { avaliacao.Nota, avaliacao.Texto, avaliacao.Id });
    }
}
