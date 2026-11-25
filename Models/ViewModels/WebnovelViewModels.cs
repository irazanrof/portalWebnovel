using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PortalWebnovels.Models.Enums;

namespace PortalWebnovels.Models.ViewModels;

public class WebnovelResumoViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Sinopse { get; set; } = string.Empty;
    public StatusObra Status { get; set; }
    public int AutorId { get; set; }
    public string AutorNome { get; set; } = string.Empty;
    public string Generos { get; set; } = string.Empty;
    public int TotalCapitulos { get; set; }
    public double? Media { get; set; }
    public int TotalAvaliacoes { get; set; }
}

public class WebnovelListaViewModel
{
    public IEnumerable<WebnovelResumoViewModel> Obras { get; set; } = new List<WebnovelResumoViewModel>();
    public IEnumerable<Genero> Generos { get; set; } = new List<Genero>();
    public string? Busca { get; set; }
    public int? GeneroId { get; set; }
    public bool Minhas { get; set; }
}

public class WebnovelCriarViewModel
{
    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O título deve ter de 3 a 150 caracteres.")]
    [DisplayName("Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a sinopse.")]
    [StringLength(5000, MinimumLength = 50, ErrorMessage = "A sinopse deve ter no mínimo 50 caracteres.")]
    [DisplayName("Sinopse")]
    public string Sinopse { get; set; } = string.Empty;

    [DisplayName("Gêneros")]
    public List<int> GenerosIds { get; set; } = new();

    [Range(0, 1, ErrorMessage = "O primeiro capítulo deve ser 1 (ou 0 para prólogo).")]
    [DisplayName("Número do primeiro capítulo")]
    public int NumeroPrimeiroCapitulo { get; set; } = 1;

    [ValidateNever]
    public IEnumerable<Genero> GenerosDisponiveis { get; set; } = new List<Genero>();
}

/// Dados da obra guardados na Session entre a 1ª etapa (dados da webnovel) e a 2ª (primeiro capítulo).
/// Nada é gravado no banco até o capítulo ser criado: sem capítulo, a webnovel não é publicada.
public class WebnovelRascunho
{
    public string Titulo { get; set; } = string.Empty;
    public string Sinopse { get; set; } = string.Empty;
    public List<int> GenerosIds { get; set; } = new();
    public int NumeroPrimeiroCapitulo { get; set; } = 1;
}

public class WebnovelEditarViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O título deve ter de 3 a 150 caracteres.")]
    [DisplayName("Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a sinopse.")]
    [StringLength(5000, MinimumLength = 50, ErrorMessage = "A sinopse deve ter no mínimo 50 caracteres.")]
    [DisplayName("Sinopse")]
    public string Sinopse { get; set; } = string.Empty;

    [DisplayName("Status")]
    public StatusObra Status { get; set; }

    [DisplayName("Gêneros")]
    public List<int> GenerosIds { get; set; } = new();

    [ValidateNever]
    public IEnumerable<Genero> GenerosDisponiveis { get; set; } = new List<Genero>();
}

public class WebnovelDetalhesViewModel
{
    public Webnovel Obra { get; set; } = null!;
    public string AutorNome { get; set; } = string.Empty;
    public IDictionary<int, string> NomesUsuarios { get; set; } = new Dictionary<int, string>();
    public bool Logado { get; set; }
    public bool EhAutor { get; set; }
    public bool EhAdmin { get; set; }
    public bool NaBiblioteca { get; set; }
    public bool NosFavoritos { get; set; }
    public Avaliacao? MinhaAvaliacao { get; set; }
    public int? ContinuarCapituloId { get; set; }

    public bool PodeGerenciar => EhAutor || EhAdmin;
}
