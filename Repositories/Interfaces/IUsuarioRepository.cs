using PortalWebnovels.Models;

namespace PortalWebnovels.Repositories.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Usuario? ObterPorEmail(string email);
    IEnumerable<Usuario> Buscar(string? termo);
    bool EmailEmUso(string email);
    IDictionary<int, string> ObterNomes();
}
