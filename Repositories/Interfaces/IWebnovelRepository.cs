using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;

namespace PortalWebnovels.Repositories.Interfaces;

public interface IWebnovelRepository : IRepository<Webnovel>
{
    Webnovel? ObterPorCapituloId(int capituloId);
    IEnumerable<WebnovelResumoViewModel> ObterResumos(string? busca, int? generoId, int? autorId);

    int AdicionarCapitulo(int webnovelId, Capitulo capitulo);
    void AtualizarCapitulo(Capitulo capitulo);
    void RemoverCapitulo(int capituloId, IEnumerable<Capitulo> renumerados);
    int AdicionarAvaliacao(int webnovelId, Avaliacao avaliacao);
    void AtualizarAvaliacao(Avaliacao avaliacao);
}
