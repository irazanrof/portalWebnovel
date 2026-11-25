using PortalWebnovels.Models;

namespace PortalWebnovels.Repositories.Interfaces;

/// Repositório especializado de Genero: herda o CRUD genérico e acrescenta consultas próprias.
public interface IGeneroRepository : IRepository<Genero>
{
    bool ExisteComNome(string nome, int? ignorarId = null);

    bool EstaEmUso(int id);
}
