namespace PortalWebnovels.Models.ViewModels;

/// <summary>Dados da tela de gerenciamento de usuários: termo pesquisado e resultado.</summary>
public class AdminViewModel
{
    public string? Busca { get; set; }
    public List<Usuario> Usuarios { get; set; } = new();
}
