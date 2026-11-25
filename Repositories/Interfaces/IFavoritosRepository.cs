using PortalWebnovels.Models;

namespace PortalWebnovels.Repositories.Interfaces;

public interface IFavoritosRepository
{
    Favoritos ObterDoUsuario(int usuarioId);
    void Adicionar(int usuarioId, int webnovelId);
    void Remover(int usuarioId, int webnovelId);
}
