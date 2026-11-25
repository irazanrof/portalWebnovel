using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PortalWebnovels.Models.ViewModels;

/// <summary>
/// ViewModel: modelo feito para o FORMULÁRIO (tela). Tem 'set' público porque o ASP.NET
/// precisa preencher os campos ao receber o formulário (Model Binding).
/// As Data Annotations validam o formato dos campos (ModelState);
/// as regras de negócio continuam dentro da entidade Genero.
/// </summary>
public class GeneroViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do gênero é obrigatório.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres.")]
    [DisplayName("Nome do gênero")]
    public string Nome { get; set; } = string.Empty;
}
