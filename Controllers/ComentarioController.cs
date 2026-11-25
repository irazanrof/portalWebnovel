using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Controllers;

public class ComentarioController : BaseController
{
    private readonly IComentarioRepository _comentarios;
    private readonly IWebnovelRepository _obras;
    private readonly IUsuarioRepository _usuarios;

    public ComentarioController(IComentarioRepository comentarios, IWebnovelRepository obras, IUsuarioRepository usuarios)
    {
        _comentarios = comentarios;
        _obras = obras;
        _usuarios = usuarios;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(int capituloId, string? texto)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        try
        {
            var comentario = new Comentario(UsuarioLogadoId!.Value, capituloId, texto ?? string.Empty);
            _comentarios.Adicionar(comentario);
            TempData["Sucesso"] = "Comentário publicado!";
        }
        catch (DominioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction("Ler", "Capitulo", new { id = capituloId });
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var comentario = _comentarios.ObterPorId(id);
        if (comentario == null) return NotFound($"Comentário #{id} não localizado.");
        if (comentario.UsuarioId != UsuarioLogadoId)
        {
            TempData["Erro"] = "Você só pode editar os seus próprios comentários.";
            return RedirectToAction("Ler", "Capitulo", new { id = comentario.CapituloId });
        }

        return View(new ComentarioEditarViewModel { Id = comentario.Id, CapituloId = comentario.CapituloId, Texto = comentario.Texto });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, ComentarioEditarViewModel vm)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;
        if (id != vm.Id) return BadRequest("Inconsistência entre o ID da URL e o do formulário.");

        if (!ModelState.IsValid)
            return View(vm);

        var comentario = _comentarios.ObterPorId(id);
        if (comentario == null) return NotFound($"Comentário #{id} não localizado.");
        if (comentario.UsuarioId != UsuarioLogadoId)
        {
            TempData["Erro"] = "Você só pode editar os seus próprios comentários.";
            return RedirectToAction("Ler", "Capitulo", new { id = comentario.CapituloId });
        }

        try
        {
            comentario.Editar(vm.Texto);
            _comentarios.Atualizar(comentario);
            TempData["Sucesso"] = "Comentário atualizado!";
            return RedirectToAction("Ler", "Capitulo", new { id = comentario.CapituloId });
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var comentario = _comentarios.ObterPorId(id);
        if (comentario == null) return RedirectToAction("Index", "Webnovel");

        var usuario = _usuarios.ObterPorId(UsuarioLogadoId!.Value);
        var obra = _obras.ObterPorCapituloId(comentario.CapituloId);

        if (usuario == null || obra == null || !usuario.PodeExcluirComentario(comentario, obra))
        {
            TempData["Erro"] = "Você não tem permissão para excluir este comentário.";
        }
        else
        {
            _comentarios.Remover(id);
            TempData["Sucesso"] = "Comentário excluído.";
        }
        return RedirectToAction("Ler", "Capitulo", new { id = comentario.CapituloId });
    }
}
