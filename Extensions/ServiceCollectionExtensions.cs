using Microsoft.Extensions.DependencyInjection;
using PortalWebnovels.Repositories.Dapper;
using PortalWebnovels.Repositories.Interfaces;

namespace PortalWebnovels.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebnovelServices(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IGeneroRepository>(_ => new GeneroRepository(connectionString));
        services.AddScoped<IUsuarioRepository>(_ => new UsuarioRepository(connectionString));
        services.AddScoped<IWebnovelRepository>(_ => new WebnovelRepository(connectionString));
        services.AddScoped<IComentarioRepository>(_ => new ComentarioRepository(connectionString));
        services.AddScoped<IBibliotecaRepository>(_ => new BibliotecaRepository(connectionString));
        services.AddScoped<IFavoritosRepository>(_ => new FavoritosRepository(connectionString));
        services.AddScoped<IHistoricoRepository>(_ => new HistoricoRepository(connectionString));

        services.AddDistributedMemoryCache();
        services.AddSession(o =>
        {
            o.IdleTimeout = TimeSpan.FromHours(2);
            o.Cookie.HttpOnly = true;
            o.Cookie.IsEssential = true;
        });
        return services;
    }
}
