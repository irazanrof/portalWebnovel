using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Controllers;

public class BibliotecaController : BaseController
{
    private readonly IBibliotecaRepository _lista;
    private readonly IWebnovelRepository _obras;
    private readonly IUsuarioRepository _usuarios;

    public BibliotecaController(IBibliotecaRepository lista, IWebnovelRepository obras, IUsuarioRepository usuarios)
    {
        _lista = lista;
        _obras = obras;
        _usuarios = usuarios;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        ViewBag.Nomes = _usuarios.ObterNomes();
        return View(_lista.ObterDoUsuario(UsuarioLogadoId!.Value));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Adicionar(int webnovelId, string? voltarPara)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        try
        {
            var obra = _obras.ObterPorId(webnovelId);
            if (obra == null) return NotFound($"Webnovel #{webnovelId} não localizada.");

            var lista = _lista.ObterDoUsuario(UsuarioLogadoId!.Value);
            lista.Adicionar(obra);   // lança exceção se a obra já estiver na lista
            _lista.Adicionar(UsuarioLogadoId.Value, obra.Id);

            TempData["Sucesso"] = $"'{obra.Titulo}' adicionada à biblioteca.";
        }
        catch (DominioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction("Detalhes", "Webnovel", new { id = webnovelId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remover(int webnovelId, string? voltarPara)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        _lista.Remover(UsuarioLogadoId!.Value, webnovelId);
        TempData["Sucesso"] = "Obra removida da biblioteca.";

        if (voltarPara == "lista")
            return RedirectToAction(nameof(Index));
        return RedirectToAction("Detalhes", "Webnovel", new { id = webnovelId });
    }
}
