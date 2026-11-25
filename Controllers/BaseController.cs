using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;

namespace PortalWebnovels.Controllers;

/// Controller base: sabe quem é o usuário logado (guardado na Session) e bloqueia acessos indevidos.
public abstract class BaseController : Controller
{
    protected int? UsuarioLogadoId => HttpContext.Session.GetInt32("UsuarioId");
    protected bool EstaLogado => UsuarioLogadoId != null;
    protected bool EhAdmin => HttpContext.Session.GetString("UsuarioTipo") == "Administrador";

    protected void IniciarSessao(Usuario usuario)
    {
        HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
        HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
        HttpContext.Session.SetString("UsuarioTipo", usuario.Tipo);
    }

    // Rascunho da webnovel em criação (guardado na Session até o primeiro capítulo ser salvo).
    private const string ChaveRascunho = "RascunhoWebnovel";

    protected WebnovelRascunho? ObterRascunho()
    {
        var json = HttpContext.Session.GetString(ChaveRascunho);
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize<WebnovelRascunho>(json); }
        catch (JsonException) { return null; }
    }

    protected void SalvarRascunho(WebnovelRascunho rascunho)
        => HttpContext.Session.SetString(ChaveRascunho, JsonSerializer.Serialize(rascunho));

    protected void LimparRascunho() => HttpContext.Session.Remove(ChaveRascunho);

    // Devolve um redirecionamento se o acesso não é permitido; null se pode seguir.
    protected IActionResult? ExigirLogin()
    {
        if (EstaLogado) return null;
        TempData["Erro"] = "Entre na sua conta para continuar.";
        return RedirectToAction("Entrar", "Conta");
    }

    protected IActionResult? ExigirAdmin()
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;
        if (EhAdmin) return null;
        TempData["Erro"] = "Apenas administradores podem acessar esta área.";
        return RedirectToAction("Index", "Home");
    }
}
