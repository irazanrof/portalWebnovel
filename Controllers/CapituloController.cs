using Microsoft.AspNetCore.Mvc;
using PortalWebnovels.Exceptions;
using PortalWebnovels.Models;
using PortalWebnovels.Models.ViewModels;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Controllers;

public class CapituloController : BaseController
{
    private readonly IWebnovelRepository _obras;
    private readonly IUsuarioRepository _usuarios;
    private readonly IComentarioRepository _comentarios;
    private readonly IHistoricoRepository _historico;
    private readonly IGeneroRepository _generos;

    public CapituloController(IWebnovelRepository obras, IUsuarioRepository usuarios,
        IComentarioRepository comentarios, IHistoricoRepository historico, IGeneroRepository generos)
    {
        _generos = generos;
        _obras = obras;
        _usuarios = usuarios;
        _comentarios = comentarios;
        _historico = historico;
    }

    // LER ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Ler(int id)
    {
        var obra = _obras.ObterPorCapituloId(id);
        if (obra == null) return NotFound($"Capítulo #{id} não localizado.");

        var capitulo = obra.Capitulos.First(c => c.Id == id);
        var ordenados = obra.Capitulos.OrderBy(c => c.Numero).ToList();
        int pos = ordenados.IndexOf(capitulo);

        Usuario? usuario = EstaLogado ? _usuarios.ObterPorId(UsuarioLogadoId!.Value) : null;

        // Registra o histórico: só o último capítulo lido de cada obra
        if (usuario != null)
        {
            var historico = _historico.ObterPorUsuarioEObra(usuario.Id, obra.Id);
            if (historico == null)
                historico = new HistoricoLeitura(usuario.Id, obra, capitulo);
            else
                historico.AtualizarLeitura(obra, capitulo);
            _historico.Registrar(historico);
        }

        var nomes = _usuarios.ObterNomes();
        var comentarios = _comentarios.ObterPorCapitulo(id).Select(c => new ComentarioItemViewModel
        {
            Id = c.Id,
            UsuarioNome = nomes.TryGetValue(c.UsuarioId, out var n) ? n : "?",
            Texto = c.Texto,
            Data = c.Data,
            DataEdicao = c.DataEdicao,
            PodeEditar = usuario != null && c.UsuarioId == usuario.Id,
            PodeExcluir = usuario != null && usuario.PodeExcluirComentario(c, obra)   // polimorfismo
        }).ToList();

        return View(new CapituloLeituraViewModel
        {
            Obra = obra,
            Capitulo = capitulo,
            Anterior = pos > 0 ? ordenados[pos - 1] : null,
            Proximo = pos < ordenados.Count - 1 ? ordenados[pos + 1] : null,
            Logado = usuario != null,
            EhAutor = obra.AutorId == UsuarioLogadoId,
            Comentarios = comentarios
        });
    }

    // CRIAR ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Criar(int webnovelId)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var obra = _obras.ObterPorId(webnovelId);
        if (obra == null) return NotFound($"Webnovel #{webnovelId} não localizada.");
        if (obra.AutorId != UsuarioLogadoId)
        {
            TempData["Erro"] = "Somente o autor pode adicionar capítulos.";
            return RedirectToAction("Detalhes", "Webnovel", new { id = webnovelId });
        }

        int proximo = obra.Capitulos.Count == 0 ? 1 : obra.Capitulos.Max(c => c.Numero) + 1;
        return View("Formulario", new CapituloViewModel
        {
            WebnovelId = obra.Id,
            WebnovelTitulo = obra.Titulo,
            Numero = proximo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(CapituloViewModel vm)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        if (!ModelState.IsValid)
            return View("Formulario", vm);

        try
        {
            var obra = _obras.ObterPorId(vm.WebnovelId);
            if (obra == null) return NotFound($"Webnovel #{vm.WebnovelId} não localizada.");
            if (obra.AutorId != UsuarioLogadoId)
            {
                TempData["Erro"] = "Somente o autor pode adicionar capítulos.";
                return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
            }

            var capitulo = obra.AdicionarCapitulo(vm.Titulo, vm.Conteudo);
            _obras.AdicionarCapitulo(obra.Id, capitulo);

            TempData["Sucesso"] = $"Capítulo {capitulo.Numero} publicado!";
            return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Formulario", vm);
        }
    }

    // PRIMEIRO CAPÍTULO ------------------------------------------------------------------
    [HttpGet]
    public IActionResult PrimeiroCapitulo()
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var rascunho = ObterRascunho();
        if (rascunho == null)
        {
            TempData["Erro"] = "Preencha os dados da webnovel antes de criar o primeiro capítulo.";
            return RedirectToAction("Criar", "Webnovel");
        }

        return View("Formulario", new CapituloViewModel
        {
            WebnovelTitulo = rascunho.Titulo,
            Numero = rascunho.NumeroPrimeiroCapitulo,
            PrimeiroCapitulo = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PrimeiroCapitulo(CapituloViewModel vm)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var rascunho = ObterRascunho();
        if (rascunho == null)
        {
            TempData["Erro"] = "Os dados da webnovel expiraram. Preencha-os novamente.";
            return RedirectToAction("Criar", "Webnovel");
        }

        vm.PrimeiroCapitulo = true;
        vm.WebnovelTitulo = rascunho.Titulo;
        vm.Numero = rascunho.NumeroPrimeiroCapitulo;

        if (!ModelState.IsValid)
            return View("Formulario", vm);

        try
        {
            var usuario = _usuarios.ObterPorId(UsuarioLogadoId!.Value)
                ?? throw new DominioException("Usuário não encontrado.");

            var generos = _generos.ObterTodos().Where(g => rascunho.GenerosIds.Contains(g.Id)).ToList();

            // Construtor q valida tudo (título, sinopse, permissão do autor e primeiro capítulo).
            var obra = new Webnovel(rascunho.Titulo, rascunho.Sinopse, usuario, generos,
                                    vm.Titulo, vm.Conteudo, rascunho.NumeroPrimeiroCapitulo);

            int id = _obras.Adicionar(obra);
            LimparRascunho();

            TempData["Sucesso"] = $"Webnovel '{obra.Titulo}' publicada com sucesso!";
            return RedirectToAction("Detalhes", "Webnovel", new { id });
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Formulario", vm);
        }
    }

    // EDITAR ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Editar(int id)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var obra = _obras.ObterPorCapituloId(id);
        if (obra == null) return NotFound($"Capítulo #{id} não localizado.");
        if (obra.AutorId != UsuarioLogadoId)
        {
            TempData["Erro"] = "Somente o autor pode editar capítulos.";
            return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
        }

        var capitulo = obra.Capitulos.First(c => c.Id == id);
        return View("Formulario", new CapituloViewModel
        {
            Id = capitulo.Id,
            WebnovelId = obra.Id,
            WebnovelTitulo = obra.Titulo,
            Numero = capitulo.Numero,
            Titulo = capitulo.Titulo,
            Conteudo = capitulo.Conteudo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, CapituloViewModel vm)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;
        if (id != vm.Id) return BadRequest("Inconsistência entre o ID da URL e o do formulário.");

        if (!ModelState.IsValid)
            return View("Formulario", vm);

        try
        {
            var obra = _obras.ObterPorCapituloId(id);
            if (obra == null) return NotFound($"Capítulo #{id} não localizado.");
            if (obra.AutorId != UsuarioLogadoId)
            {
                TempData["Erro"] = "Somente o autor pode editar capítulos.";
                return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
            }

            int numero = obra.Capitulos.First(c => c.Id == id).Numero;
            var capitulo = obra.EditarCapitulo(numero, vm.Titulo, vm.Conteudo);   // atualiza DataModificacao
            _obras.AtualizarCapitulo(capitulo);

            TempData["Sucesso"] = "Capítulo atualizado!";
            return RedirectToAction(nameof(Ler), new { id });
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Formulario", vm);
        }
    }

    // EXCLUIR ------------------------------------------------------------------ 
    [HttpGet]
    public IActionResult Excluir(int id)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var obra = _obras.ObterPorCapituloId(id);
        if (obra == null) return NotFound($"Capítulo #{id} não localizado.");
        if (obra.AutorId != UsuarioLogadoId)
        {
            TempData["Erro"] = "Somente o autor pode excluir capítulos.";
            return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
        }
        if (obra.Capitulos.Count <= 1)
        {
            TempData["Erro"] = "A webnovel precisa ter pelo menos um capítulo. Para apagar tudo, exclua a webnovel.";
            return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
        }

        var capitulo = obra.Capitulos.First(c => c.Id == id);
        return View(new CapituloExcluirViewModel
        {
            Id = capitulo.Id,
            WebnovelId = obra.Id,
            WebnovelTitulo = obra.Titulo,
            Numero = capitulo.Numero,
            Titulo = capitulo.Titulo,
            TotalComentarios = _comentarios.ObterPorCapitulo(id).Count(),
            CapitulosRenumerados = capitulo.Numero >= 1 ? obra.Capitulos.Count(c => c.Numero > capitulo.Numero) : 0
        });
    }

    [HttpPost, ActionName("Excluir")]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirConfirmado(int id)
    {
        var bloqueio = ExigirLogin();
        if (bloqueio != null) return bloqueio;

        var obra = _obras.ObterPorCapituloId(id);
        if (obra == null) return NotFound($"Capítulo #{id} não localizado.");
        if (obra.AutorId != UsuarioLogadoId)
        {
            TempData["Erro"] = "Somente o autor pode excluir capítulos.";
            return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
        }

        try
        {
            int numero = obra.Capitulos.First(c => c.Id == id).Numero;
            var renumerados = obra.RemoverCapitulo(numero);   // a obra aplica a regra e renumera os seguintes
            _obras.RemoverCapitulo(id, renumerados);

            TempData["Sucesso"] = $"Capítulo {numero} excluído.";
        }
        catch (DominioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction("Detalhes", "Webnovel", new { id = obra.Id });
    }
}
