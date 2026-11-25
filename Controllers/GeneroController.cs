using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Controllers;

/// CRUD de gêneros. Acessado pela página Administração; restrito a administradores.
public class GeneroController : BaseController
{
    private readonly IGeneroRepository _generoRepo;

    public GeneroController(IGeneroRepository generoRepo)
    {
        _generoRepo = generoRepo ?? throw new ArgumentNullException(nameof(generoRepo));
    }

    // LISTAR ----------------------------------------------------------
    [HttpGet]
    public IActionResult Index()
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        return View(_generoRepo.ObterTodos());
    }

    // CRIAR ----------------------------------------------------------
    [HttpGet]
    public IActionResult Criar()
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        return View(new GeneroViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(GeneroViewModel vm)
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            if (_generoRepo.ExisteComNome(vm.Nome))
                throw new RegistroDuplicadoException("Já existe um gênero com este nome.");

            var genero = new Genero(vm.Nome);
            _generoRepo.Adicionar(genero);

            TempData["Sucesso"] = $"Gênero '{genero.Nome}' cadastrado com sucesso!";
            return RedirectToAction(nameof(Index)); 
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // EDITAR ----------------------------------------------------------
    [HttpGet]
    public IActionResult Editar(int id)
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        var genero = _generoRepo.ObterPorId(id);
        if (genero == null)
            return NotFound($"Gênero #{id} não localizado.");

        return View(new GeneroViewModel { Id = genero.Id, Nome = genero.Nome });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, GeneroViewModel vm)
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        if (id != vm.Id)
            return BadRequest("Inconsistência entre o ID da URL e o ID do formulário.");

        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            var genero = _generoRepo.ObterPorId(id);
            if (genero == null)
                return NotFound($"Gênero #{id} não localizado.");

            if (_generoRepo.ExisteComNome(vm.Nome, ignorarId: id))
                throw new RegistroDuplicadoException("Já existe outro gênero com este nome.");

            genero.AlterarNome(vm.Nome);      // método de domínio (R1)
            _generoRepo.Atualizar(genero);

            TempData["Sucesso"] = $"Gênero '{genero.Nome}' atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // EXCLUIR ----------------------------------------------------------
    [HttpGet]
    public IActionResult Excluir(int id)
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        var genero = _generoRepo.ObterPorId(id);
        if (genero == null)
            return NotFound($"Gênero #{id} não localizado.");

        return View(genero);
    }

    [HttpPost, ActionName("Excluir")]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirConfirmado(int id)
    {
        var bloqueio = ExigirAdmin();
        if (bloqueio != null) return bloqueio;

        var genero = _generoRepo.ObterPorId(id);
        if (genero == null)
            return RedirectToAction(nameof(Index));

        try
        {
            if (_generoRepo.EstaEmUso(id))
                throw new DominioException(
                    $"O gênero '{genero.Nome}' não pode ser excluído porque há webnovels classificadas nele.");

            _generoRepo.Remover(id);
            TempData["Sucesso"] = $"Gênero '{genero.Nome}' excluído com sucesso!";
        }
        catch (DominioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
