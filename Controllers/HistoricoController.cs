using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Controllers;

public class HistoricoController : BaseController
{
    private readonly IHistoricoRepository _historico;

    public HistoricoController(IHistoricoRepository historico)
    {
        _historico = historico;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        return View(_historico.ObterResumoDoUsuario(UsuarioLogadoId!.Value));
    }
}
