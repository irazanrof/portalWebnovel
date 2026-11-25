using PortalWebnovels.Exceptions;
using PortalWebnovels.Models.Enums;

namespace PortalWebnovels.Models;

public class Webnovel
{
    private readonly List<Capitulo> _capitulos = new();
    private readonly List<Avaliacao> _avaliacoes = new();
    private readonly List<Genero> _generos = new();  

    public int Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Sinopse { get; private set; } = string.Empty;
    public StatusObra Status { get; private set; }
    public int AutorId { get; private set; }         
    public DateTime DataCriacao { get; private set; }

    public IReadOnlyCollection<Capitulo> Capitulos => _capitulos;
    public IReadOnlyCollection<Avaliacao> Avaliacoes => _avaliacoes;
    public IReadOnlyCollection<Genero> Generos => _generos;

    protected Webnovel() { }   // Dapper

    public Webnovel(string titulo, string sinopse, Usuario autor, IEnumerable<Genero> generos,
                    string tituloPrimeiroCapitulo, string conteudoPrimeiroCapitulo, int numeroPrimeiroCapitulo = 1)
    {
        if (autor == null)
            throw new DominioException("A webnovel precisa de um autor.");

        if (!autor.PodePublicar())
            throw new PublicacaoNaoPermitidaException(
                "Você não tem permissão para publicar webnovels. Procure a administração do site.");

        Titulo = ValidarTitulo(titulo);
        Sinopse = ValidarSinopse(sinopse);

        if (numeroPrimeiroCapitulo != 0 && numeroPrimeiroCapitulo != 1)
            throw new DominioException("O primeiro capítulo deve ser o número 1 (ou 0, no caso de um prólogo).");

        AutorId = autor.Id;
        Status = StatusObra.EmAndamento;
        DataCriacao = DateTime.Now;

        DefinirGeneros(generos);
        _capitulos.Add(new Capitulo(numeroPrimeiroCapitulo, tituloPrimeiroCapitulo, conteudoPrimeiroCapitulo));
    }

    //  DADOS DA OBRA ------------------------------------------------------------
    public static void ValidarDados(string? titulo, string? sinopse)
    {
        ValidarTitulo(titulo);
        ValidarSinopse(sinopse);
    }

    public void EditarDados(string titulo, string sinopse)
    {
        string novoTitulo = ValidarTitulo(titulo);
        string novaSinopse = ValidarSinopse(sinopse);
        Titulo = novoTitulo;
        Sinopse = novaSinopse;
    }

    public void AlterarStatus(StatusObra novoStatus)
    {
        if (!Enum.IsDefined(typeof(StatusObra), novoStatus))
            throw new DominioException("Status inválido.");
        Status = novoStatus;
    }

    public void DefinirGeneros(IEnumerable<Genero>? generos)
    {
        _generos.Clear();
        if (generos == null) return;
        foreach (var genero in generos)
            if (!_generos.Any(g => g.Id == genero.Id))
                _generos.Add(genero);
    }

    // CAPÍTULOS ------------------------------------------------------------
    public Capitulo AdicionarCapitulo(string titulo, string conteudo)
    {
        int proximo = _capitulos.Count == 0 ? 1 : _capitulos.Max(c => c.Numero) + 1;
        var capitulo = new Capitulo(proximo, titulo, conteudo);
        _capitulos.Add(capitulo);
        return capitulo;
    }

    public Capitulo EditarCapitulo(int numero, string titulo, string conteudo)
    {
        var capitulo = _capitulos.FirstOrDefault(c => c.Numero == numero)
            ?? throw new DominioException("Capítulo não encontrado nesta obra.");
        capitulo.Editar(titulo, conteudo);
        return capitulo;
    }

    public IReadOnlyList<Capitulo> RemoverCapitulo(int numero)
    {
        var capitulo = _capitulos.FirstOrDefault(c => c.Numero == numero)
            ?? throw new DominioException("Capítulo não encontrado nesta obra.");

        if (_capitulos.Count == 1)
            throw new DominioException(
                "A webnovel precisa ter pelo menos um capítulo. Para apagar tudo, exclua a webnovel.");

        _capitulos.Remove(capitulo);

        var renumerados = new List<Capitulo>();

        // Excluir o prólogo (0) não deixa buraco: a sequência 1, 2, 3... continua válida.
        if (numero >= 1)
        {
            foreach (var seguinte in _capitulos.Where(c => c.Numero > numero).OrderBy(c => c.Numero))
            {
                seguinte.DecrementarNumero();
                renumerados.Add(seguinte);
            }
        }
        return renumerados;
    }

    //  AVALIAÇÕES ------------------------------------------------------------
    public Avaliacao AdicionarAvaliacao(int usuarioId, int nota, string? texto)
    {
        if (_avaliacoes.Any(a => a.UsuarioId == usuarioId))
            throw new AvaliacaoDuplicadaException("Você já avaliou esta webnovel. Edite a sua avaliação existente.");

        var avaliacao = new Avaliacao(usuarioId, nota, texto);
        _avaliacoes.Add(avaliacao);
        return avaliacao;
    }

    public Avaliacao EditarAvaliacao(int usuarioId, int nota, string? texto)
    {
        var avaliacao = _avaliacoes.FirstOrDefault(a => a.UsuarioId == usuarioId)
            ?? throw new DominioException("Você ainda não avaliou esta webnovel.");
        avaliacao.Alterar(nota, texto);
        return avaliacao;
    }

    public double MediaAvaliacoes() => _avaliacoes.Count == 0 ? 0 : _avaliacoes.Average(a => a.Nota);


    internal void CarregarCapitulos(IEnumerable<Capitulo> capitulos) { _capitulos.Clear(); _capitulos.AddRange(capitulos); }
    internal void CarregarAvaliacoes(IEnumerable<Avaliacao> avaliacoes) { _avaliacoes.Clear(); _avaliacoes.AddRange(avaliacoes); }
    internal void CarregarGeneros(IEnumerable<Genero> generos) { _generos.Clear(); _generos.AddRange(generos); }

    // VALIDAÇÕES ------------------------------------------------------------
    private static string ValidarTitulo(string? titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DominioException("O título é obrigatório.");
        titulo = titulo.Trim();
        if (titulo.Length < 3)
            throw new DominioException("O título deve ter no mínimo 3 caracteres.");
        if (titulo.Length > 150)
            throw new DominioException("O título pode ter no máximo 150 caracteres.");
        return titulo;
    }

    private static string ValidarSinopse(string? sinopse)
    {
        if (string.IsNullOrWhiteSpace(sinopse))
            throw new DominioException("A sinopse é obrigatória.");
        sinopse = sinopse.Trim();
        if (sinopse.Length < 50)
            throw new DominioException("A sinopse deve ter no mínimo 50 caracteres.");
        if (sinopse.Length > 5000)
            throw new DominioException("A sinopse pode ter no máximo 5000 caracteres.");
        return sinopse;
    }
}
