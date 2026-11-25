using PortalWebnovels.Models;

namespace PortalWebnovels.Repositories.Interfaces;

public interface IComentarioRepository : IRepository<Comentario>
{
    IEnumerable<Comentario> ObterPorCapitulo(int capituloId);
}
