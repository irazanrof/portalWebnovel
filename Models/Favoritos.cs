using PortalWebnovels.Exceptions;

namespace PortalWebnovels.Models;

public class Favoritos
{
    private readonly List<Webnovel> _obras = new();

    public int UsuarioId { get; }
    public IReadOnlyCollection<Webnovel> Obras => _obras;

    public Favoritos(int usuarioId)
    {
        if (usuarioId <= 0) throw new DominioException("Usuário inválido.");
        UsuarioId = usuarioId;
    }

    public bool Contem(Webnovel obra) => _obras.Any(o => o.Id == obra.Id);

    public void Adicionar(Webnovel obra)
    {
        if (Contem(obra))
            throw new ObraJaNaBibliotecaException("Esta obra já está nos seus favoritos.");
        _obras.Add(obra);
    }

    public void Remover(Webnovel obra)
    {
        if (_obras.RemoveAll(o => o.Id == obra.Id) == 0)
            throw new DominioException("Esta obra não está nos seus favoritos.");
    }

    internal void Carregar(IEnumerable<Webnovel> obras) { _obras.Clear(); _obras.AddRange(obras); }
}
