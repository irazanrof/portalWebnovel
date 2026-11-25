using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;

namespace PortalWebnovels.Repositories.Interfaces;

public interface IHistoricoRepository
{
    HistoricoLeitura? ObterPorUsuarioEObra(int usuarioId, int webnovelId);
    void Registrar(HistoricoLeitura historico);
    IEnumerable<HistoricoItemViewModel> ObterResumoDoUsuario(int usuarioId);
}
