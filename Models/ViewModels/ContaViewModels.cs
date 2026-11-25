using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PortalWebnovels.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [DisplayName("E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [DisplayName("Senha")]
    public string Senha { get; set; } = string.Empty;
}

public class CadastroViewModel
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter de 2 a 100 caracteres.")]
    [DisplayName("Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [DisplayName("E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter de 6 a 100 caracteres.")]
    [DataType(DataType.Password)]
    [DisplayName("Senha")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha.")]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
    [DataType(DataType.Password)]
    [DisplayName("Confirmar senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}

public class PerfilViewModel
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter de 2 a 100 caracteres.")]
    [DisplayName("Nome")]
    public string Nome { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? Tipo { get; set; }
    public bool PublicacaoRevogada { get; set; }
    public string? MotivoRevogacao { get; set; }
}
