using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PortalWebnovels.Models.ViewModels;

public class CapituloViewModel
{
    public int Id { get; set; }
    public int WebnovelId { get; set; }
    public string? WebnovelTitulo { get; set; }
    public int? Numero { get; set; }
    public bool PrimeiroCapitulo { get; set; } 

    [Required(ErrorMessage = "Informe o título do capítulo.")]
    [StringLength(150, ErrorMessage = "O título pode ter no máximo 150 caracteres.")]
    [DisplayName("Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o texto do capítulo.")]
    [DisplayName("Texto do capítulo")]
    public string Conteudo { get; set; } = string.Empty;

    public bool Editando => Id > 0;
}

public class CapituloExcluirViewModel
{
    public int Id { get; set; }
    public int WebnovelId { get; set; }
    public string WebnovelTitulo { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int TotalComentarios { get; set; }
    public int CapitulosRenumerados { get; set; }
}

public class ComentarioItemViewModel
{
    public int Id { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public DateTime? DataEdicao { get; set; }
    public bool PodeEditar { get; set; }
    public bool PodeExcluir { get; set; }
}

public class CapituloLeituraViewModel
{
    public Webnovel Obra { get; set; } = null!;
    public Capitulo Capitulo { get; set; } = null!;
    public Capitulo? Anterior { get; set; }
    public Capitulo? Proximo { get; set; }
    public bool Logado { get; set; }
    public bool EhAutor { get; set; }
    public List<ComentarioItemViewModel> Comentarios { get; set; } = new();
}

public class ComentarioEditarViewModel
{
    public int Id { get; set; }
    public int CapituloId { get; set; }

    [Required(ErrorMessage = "O comentário não pode ser vazio.")]
    [StringLength(1000, ErrorMessage = "O comentário pode ter no máximo 1000 caracteres.")]
    [DisplayName("Comentário")]
    public string Texto { get; set; } = string.Empty;
}

public class AvaliacaoViewModel
{
    public int WebnovelId { get; set; }
    public string? WebnovelTitulo { get; set; }
    public bool Editando { get; set; }

    [Range(1, 5, ErrorMessage = "Escolha uma nota de 1 a 5.")]
    [DisplayName("Nota")]
    public int Nota { get; set; } = 5;

    [StringLength(10000, ErrorMessage = "O texto pode ter no máximo 10.000 caracteres.")]
    [DisplayName("Sua avaliação (opcional)")]
    public string? Texto { get; set; }
}

public class HistoricoItemViewModel
{
    public int WebnovelId { get; set; }
    public string WebnovelTitulo { get; set; } = string.Empty;
    public int CapituloId { get; set; }
    public int CapituloNumero { get; set; }
    public string CapituloTitulo { get; set; } = string.Empty;
    public DateTime DataLeitura { get; set; }
}
