namespace PortalWebnovels.Models.ViewModels;

/// Dados da tela de gerenciamento de usuários: termo pesquisado e resultado.
public class AdminViewModel
{
    public string? Busca { get; set; }
    public List<Usuario> Usuarios { get; set; } = new();
}
