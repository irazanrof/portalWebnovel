using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Controllers;

/// Área do administrador: ver usuários e revogar/restaurar a permissão de publicar.
public class AdminController : BaseController
{
    private readonly IUsuarioRepository _usuarios;

    public AdminController(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    /// Painel da Administração: reúne os atalhos para as áreas administrativas (Gêneros, Usuários).
    [HttpGet]
    public IActionResult Index()
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        return View();
    }

    [HttpGet]
    public IActionResult Usuarios(string? busca)
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        var vm = new AdminViewModel
        {
            Busca = busca?.Trim(),
            Usuarios = _usuarios.Buscar(busca).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Revogar(int usuarioId, string? motivo, string? busca)
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        try
        {
            var admin = _usuarios.ObterPorId(UsuarioLogadoId!.Value) as Administrador
                ?? throw new DominioException("Apenas administradores podem revogar permissões.");
            var alvo = _usuarios.ObterPorId(usuarioId) as UsuarioComum
                ?? throw new DominioException("Só é possível revogar a permissão de usuários comuns.");

            admin.RevogarPublicacaoDe(alvo, motivo ?? string.Empty);
            _usuarios.Atualizar(alvo);
            TempData["Sucesso"] = $"Permissão de publicar revogada para {alvo.Nome}.";
        }
        catch (DominioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Usuarios), new { busca });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Restaurar(int usuarioId, string? busca)
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        try
        {
            var admin = _usuarios.ObterPorId(UsuarioLogadoId!.Value) as Administrador
                ?? throw new DominioException("Apenas administradores podem restaurar permissões.");
            var alvo = _usuarios.ObterPorId(usuarioId) as UsuarioComum
                ?? throw new DominioException("Usuário comum não encontrado.");

            admin.RestaurarPublicacaoDe(alvo);
            _usuarios.Atualizar(alvo);
            TempData["Sucesso"] = $"Permissão de publicar restaurada para {alvo.Nome}.";
        }
        catch (DominioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Usuarios), new { busca });
    }
}
