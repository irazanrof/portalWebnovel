namespace PortalWebnovels.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    int Adicionar(T entidade);

    void Atualizar(T entidade);

    void Remover(int id);

    T? ObterPorId(int id);

    IEnumerable<T> ObterTodos();
}
