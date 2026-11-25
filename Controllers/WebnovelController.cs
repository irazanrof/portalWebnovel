using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Controllers;

public class WebnovelController : BaseController
{
    private readonly IWebnovelRepository _obras;
    private readonly IGeneroRepository _generos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IBibliotecaRepository _biblioteca;
    private readonly IFavoritosRepository _favoritos;
    private readonly IHistoricoRepository _historico;

    public WebnovelController(IWebnovelRepository obras, IGeneroRepository generos, IUsuarioRepository usuarios,
        IBibliotecaRepository biblioteca, IFavoritosRepository favoritos, IHistoricoRepository historico)
    {
        _obras = obras;
        _generos = generos;
        _usuarios = usuarios;
        _biblioteca = biblioteca;
        _favoritos = favoritos;
        _historico = historico;
    }

    private bool PodeGerenciar(Webnovel obra) => EhAdmin || obra.AutorId == UsuarioLogadoId;

    // LISTAR ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Index(string? busca, int? generoId, bool minhas = false)
    {
        int? autorId = null;
        if (minhas)
        {
            var bloqueio = ExigirLogin();
            if (bloqueio != null) return bloqueio;
            autorId = UsuarioLogadoId;
        }

        return View(new WebnovelListaViewModel
        {
            Obras = _obras.ObterResumos(busca, generoId, autorId),
            Generos = _generos.ObterTodos(),
            Busca = busca,
            GeneroId = generoId,
            Minhas = minhas
        });
    }

    // DETALHES ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Detalhes(int id)
    {
        var obra = _obras.ObterPorId(id);
        if (obra == null)
            return NotFound($"Webnovel #{id} não localizada.");

        var nomes = _usuarios.ObterNomes();
        var vm = new WebnovelDetalhesViewModel
        {
            Obra = obra,
            NomesUsuarios = nomes,
            AutorNome = nomes.TryGetValue(obra.AutorId, out var nomeAutor) ? nomeAutor : "?",
            Logado = EstaLogado,
            EhAdmin = EhAdmin,
            EhAutor = obra.AutorId == UsuarioLogadoId
        };

        if (UsuarioLogadoId is int uid)
        {
            vm.NaBiblioteca = _biblioteca.ObterDoUsuario(uid).Obras.Any(o => o.Id == obra.Id);
            vm.NosFavoritos = _favoritos.ObterDoUsuario(uid).Obras.Any(o => o.Id == obra.Id);
            vm.MinhaAvaliacao = obra.Avaliacoes.FirstOrDefault(a => a.UsuarioId == uid);
            vm.ContinuarCapituloId = _historico.ObterPorUsuarioEObra(uid, obra.Id)?.CapituloId;
        }

        return View(vm);
    }

    // CRIAR ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Criar(bool voltar = false)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var usuario = _usuarios.ObterPorId(UsuarioLogadoId!.Value);
        if (usuario == null || !usuario.PodePublicar())   // polimorfismo
        {
            TempData["Erro"] = "Você não tem permissão para publicar webnovels.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new WebnovelCriarViewModel { GenerosDisponiveis = _generos.ObterTodos() };

        // "Voltar" da tela do capítulo reabre o formulário preenchido; qualquer outra entrada começa do zero.
        var rascunho = voltar ? ObterRascunho() : null;
        if (rascunho != null)
        {
            vm.Titulo = rascunho.Titulo;
            vm.Sinopse = rascunho.Sinopse;
            vm.GenerosIds = rascunho.GenerosIds;
            vm.NumeroPrimeiroCapitulo = rascunho.NumeroPrimeiroCapitulo;
        }
        else
        {
            LimparRascunho();
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(WebnovelCriarViewModel vm)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        vm.GenerosDisponiveis = _generos.ObterTodos();
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            var usuario = _usuarios.ObterPorId(UsuarioLogadoId!.Value)
                ?? throw new DominioException("Usuário não encontrado.");

            if (!usuario.PodePublicar())
                throw new PublicacaoNaoPermitidaException(
                    "Você não tem permissão para publicar webnovels. Procure a administração do site.");

            Webnovel.ValidarDados(vm.Titulo, vm.Sinopse);   // mesmas regras do domínio, já nesta etapa

            SalvarRascunho(new WebnovelRascunho
            {
                Titulo = vm.Titulo.Trim(),
                Sinopse = vm.Sinopse.Trim(),
                GenerosIds = vm.GenerosDisponiveis.Where(g => vm.GenerosIds.Contains(g.Id)).Select(g => g.Id).ToList(),
                NumeroPrimeiroCapitulo = vm.NumeroPrimeiroCapitulo
            });

            return RedirectToAction("PrimeiroCapitulo", "Capitulo");
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // EDITAR ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Editar(int id)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var obra = _obras.ObterPorId(id);
        if (obra == null) return NotFound($"Webnovel #{id} não localizada.");
        if (!PodeGerenciar(obra))
        {
            TempData["Erro"] = "Somente o autor (ou um administrador) pode editar esta obra.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        return View(new WebnovelEditarViewModel
        {
            Id = obra.Id,
            Titulo = obra.Titulo,
            Sinopse = obra.Sinopse,
            Status = obra.Status,
            GenerosIds = obra.Generos.Select(g => g.Id).ToList(),
            GenerosDisponiveis = _generos.ObterTodos()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, WebnovelEditarViewModel vm)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;
        if (id != vm.Id) return BadRequest("Inconsistência entre o ID da URL e o do formulário.");

        vm.GenerosDisponiveis = _generos.ObterTodos();
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            var obra = _obras.ObterPorId(id);
            if (obra == null) return NotFound($"Webnovel #{id} não localizada.");
            if (!PodeGerenciar(obra))
            {
                TempData["Erro"] = "Somente o autor (ou um administrador) pode editar esta obra.";
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            obra.EditarDados(vm.Titulo, vm.Sinopse);
            obra.AlterarStatus(vm.Status);
            obra.DefinirGeneros(vm.GenerosDisponiveis.Where(g => vm.GenerosIds.Contains(g.Id)));
            _obras.Atualizar(obra);

            TempData["Sucesso"] = "Webnovel atualizada com sucesso!";
            return RedirectToAction(nameof(Detalhes), new { id });
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // EXCLUIR ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Excluir(int id)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var obra = _obras.ObterPorId(id);
        if (obra == null) return NotFound($"Webnovel #{id} não localizada.");
        if (!PodeGerenciar(obra))
        {
            TempData["Erro"] = "Somente o autor (ou um administrador) pode excluir esta obra.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        return View(obra);
    }

    [HttpPost, ActionName("Excluir")]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirConfirmado(int id)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var obra = _obras.ObterPorId(id);
        if (obra == null) return RedirectToAction(nameof(Index));
        if (!PodeGerenciar(obra))
        {
            TempData["Erro"] = "Somente o autor (ou um administrador) pode excluir esta obra.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        _obras.Remover(id);
        TempData["Sucesso"] = $"Webnovel '{obra.Titulo}' excluída.";
        return RedirectToAction(nameof(Index));
    }
}
