namespace PortalWebnovels.Models.Enums;

/// Situação da história. É só uma informação: não bloqueia nenhuma operação.
public enum StatusObra
{
    EmAndamento = 0,
    Concluida = 1,
    Hiato = 2,
    Abandonada = 3
}

public static class StatusObraExtensions
{
    public static string Descricao(this StatusObra status) => status switch
    {
        StatusObra.EmAndamento => "Em andamento",
        StatusObra.Concluida => "Concluída",
        StatusObra.Hiato => "Hiato/Pausada",
        StatusObra.Abandonada => "Abandonada",
        _ => status.ToString()
    };

    public static string ClasseBadge(this StatusObra status) => status switch
    {
        StatusObra.EmAndamento => "bg-primary",
        StatusObra.Concluida => "bg-success",
        StatusObra.Hiato => "bg-warning text-dark",
        StatusObra.Abandonada => "bg-secondary",
        _ => "bg-light text-dark"
    };
}
