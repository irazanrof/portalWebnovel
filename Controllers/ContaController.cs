using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Controllers;

/// Cadastro, entrada (e-mail + senha), perfil e saída.
public class ContaController : BaseController
{
    private readonly IUsuarioRepository _usuarios;

    public ContaController(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios ?? throw new ArgumentNullException(nameof(usuarios));
    }

    [HttpGet]
    public IActionResult Entrar() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Entrar(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var usuario = _usuarios.ObterPorEmail(vm.Email);
        if (usuario == null || !usuario.VerificarSenha(vm.Senha))
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
            return View(vm);
        }

        IniciarSessao(usuario);
        TempData["Sucesso"] = $"Bem-vindo(a), {usuario.Nome}!";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Cadastrar() => View(new CadastroViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cadastrar(CadastroViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            if (_usuarios.EmailEmUso(vm.Email))
                throw new RegistroDuplicadoException("Já existe uma conta com este e-mail.");

            var usuario = new UsuarioComum(vm.Nome, vm.Email, vm.Senha);
            int id = _usuarios.Adicionar(usuario);

            var salvo = _usuarios.ObterPorId(id)!;
            IniciarSessao(salvo);
            TempData["Sucesso"] = "Conta criada com sucesso! Você já está logado(a).";
            return RedirectToAction("Index", "Home");
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Sair()
    {
        HttpContext.Session.Clear();
        TempData["Sucesso"] = "Você saiu da sua conta.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Perfil()
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var usuario = _usuarios.ObterPorId(UsuarioLogadoId!.Value);
        if (usuario == null) return RedirectToAction(nameof(Entrar));
        return View(MontarPerfil(usuario));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Perfil(PerfilViewModel vm)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var usuario = _usuarios.ObterPorId(UsuarioLogadoId!.Value);
        if (usuario == null) return RedirectToAction(nameof(Entrar));

        if (!ModelState.IsValid)
            return View(MontarPerfil(usuario, vm.Nome));

        try
        {
            usuario.AtualizarPerfil(vm.Nome);
            _usuarios.Atualizar(usuario);
            IniciarSessao(usuario);
            TempData["Sucesso"] = "Perfil atualizado!";
            return RedirectToAction(nameof(Perfil));
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(MontarPerfil(usuario, vm.Nome));
        }
    }

    private static PerfilViewModel MontarPerfil(Usuario usuario, string? nome = null)
    {
        var comum = usuario as UsuarioComum;
        return new PerfilViewModel
        {
            Nome = nome ?? usuario.Nome,
            Email = usuario.Email,
            Tipo = usuario.Tipo,
            PublicacaoRevogada = comum?.PublicacaoRevogada ?? false,
            MotivoRevogacao = comum?.MotivoRevogacao
        };
    }
}
