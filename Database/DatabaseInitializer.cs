using Dapper;
using Microsoft.Data.Sqlite;
using PortalWebnovels.Models;

namespace PortalWebnovels.Database;

/// Cria o banco automaticamente na primeira execução: se o arquivo .db NÃO existe,
/// Cria o arquivo e executa o Database/script.sql (tabelas + dados de teste).
/// Para recriar o banco do zero, basta apagar o arquivo Database/webnovel.db.
public static class DatabaseInitializer
{
    public static void Inicializar(string connectionString, string caminhoScript)
    {
        var csb = new SqliteConnectionStringBuilder(connectionString);
        string arquivoBanco = csb.DataSource;

        if (File.Exists(arquivoBanco))
        {
            MigrarSenhas(connectionString); // banco antigo (sem senhas): adapta sem perder os dados
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(arquivoBanco)!);

        try
        {
            string script = File.ReadAllText(caminhoScript);

            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            connection.Execute(script);
        }
        catch
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(arquivoBanco)) File.Delete(arquivoBanco);
            throw;
        }
    }

    /// Bancos criados antes da autenticação por senha não têm a coluna SenhaHash.
    /// Cria a coluna e define a senha provisória "123456" para as contas já existentes.
    private static void MigrarSenhas(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        int temColuna = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM pragma_table_info('Usuario') WHERE name = 'SenhaHash';");
        if (temColuna > 0)
            return;

        using var transacao = connection.BeginTransaction();

        connection.Execute("ALTER TABLE Usuario ADD COLUMN SenhaHash TEXT NOT NULL DEFAULT '';", transaction: transacao);

        var ids = connection.Query<int>("SELECT Id FROM Usuario;", transaction: transacao).ToList();
        foreach (int id in ids)
        {
            connection.Execute(
                "UPDATE Usuario SET SenhaHash = @Hash WHERE Id = @Id;",
                new { Hash = PasswordHasher.Gerar("123456"), Id = id },
                transacao);
        }

        transacao.Commit();
    }
}
