using PortalWebnovels.Exceptions;

namespace PortalWebnovels.Models;

/// Nota de 1 a 5 e texto opcional (até 10.000 caracteres).
public class Avaliacao : Interacao
{
    public int Nota { get; private set; }
    public string? Texto { get; private set; }

    protected Avaliacao() { }   // Dapper

    internal Avaliacao(int usuarioId, int nota, string? texto) : base(usuarioId)
    {
        Nota = ValidarNota(nota);
        Texto = ValidarTexto(texto);
    }

    public override void Validar()
    {
        ValidarNota(Nota);
        ValidarTexto(Texto);
    }

    public void Alterar(int nota, string? texto)
    {
        int novaNota = ValidarNota(nota);
        string? novoTexto = ValidarTexto(texto);
        Nota = novaNota;
        Texto = novoTexto;
    }

    private static int ValidarNota(int nota)
    {
        if (nota < 1 || nota > 5)
            throw new DominioException("A nota deve ser de 1 a 5.");
        return nota;
    }

    private static string? ValidarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;
        texto = texto.Trim();
        if (texto.Length > 10000)
            throw new DominioException("O texto da avaliação não pode passar de 10.000 caracteres.");
        return texto;
    }
}
