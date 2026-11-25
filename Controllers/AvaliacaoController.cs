using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Models.ViewModels;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Controllers;

public class AvaliacaoController : BaseController
{
    private readonly IWebnovelRepository _obras;

    public AvaliacaoController(IWebnovelRepository obras)
    {
        _obras = obras;
    }

    // AVALIAR ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Avaliar(int webnovelId)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var obra = _obras.ObterPorId(webnovelId);
        if (obra == null) return NotFound($"Webnovel #{webnovelId} não localizada.");

        return View("Formulario", new AvaliacaoViewModel { WebnovelId = obra.Id, WebnovelTitulo = obra.Titulo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Avaliar(AvaliacaoViewModel vm)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        vm.Editando = false;
        if (!ModelState.IsValid)
            return View("Formulario", vm);

        try
        {
            var obra = _obras.ObterPorId(vm.WebnovelId);
            if (obra == null) return NotFound($"Webnovel #{vm.WebnovelId} não localizada.");

            // Lança AvaliacaoDuplicadaException se o usuário já avaliou esta obra
            var avaliacao = obra.AdicionarAvaliacao(UsuarioLogadoId!.Value, vm.Nota, vm.Texto);
            _obras.AdicionarAvaliacao(obra.Id, avaliacao);

            TempData["Sucesso"] = "Avaliação registrada. Obrigada!";
            return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Formulario", vm);
        }
    }

    // EDITAR ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Editar(int webnovelId)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var obra = _obras.ObterPorId(webnovelId);
        if (obra == null) return NotFound($"Webnovel #{webnovelId} não localizada.");

        var minha = obra.Avaliacoes.FirstOrDefault(a => a.UsuarioId == UsuarioLogadoId);
        if (minha == null)
            return RedirectToAction(nameof(Avaliar), new { webnovelId });

        return View("Formulario", new AvaliacaoViewModel
        {
            WebnovelId = obra.Id,
            WebnovelTitulo = obra.Titulo,
            Editando = true,
            Nota = minha.Nota,
            Texto = minha.Texto
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(AvaliacaoViewModel vm)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        vm.Editando = true;
        if (!ModelState.IsValid)
            return View("Formulario", vm);

        try
        {
            var obra = _obras.ObterPorId(vm.WebnovelId);
            if (obra == null) return NotFound($"Webnovel #{vm.WebnovelId} não localizada.");

            var avaliacao = obra.EditarAvaliacao(UsuarioLogadoId!.Value, vm.Nota, vm.Texto);
            _obras.AtualizarAvaliacao(avaliacao);

            TempData["Sucesso"] = "Avaliação atualizada!";
            return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Formulario", vm);
        }
    }
}
