using System.Globalization;

namespace PortalWebnovels.Repositories.Dapper;

/// Converte datas para o texto usado no SQLite ("yyyy-MM-dd HH:mm:ss")
internal static class Fmt
{
    public static string Data(DateTime data) => data.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    public static string? Data(DateTime? data) => data?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}
