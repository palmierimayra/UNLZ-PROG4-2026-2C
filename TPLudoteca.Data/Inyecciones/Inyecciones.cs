using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TPLudoteca.Data.Repositorios;

namespace TPLudoteca.Data.Inyecciones
{
    public static class DataAccessExtensions
    {
        public static IServiceCollection AddDataAccess(
            this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(o =>
                o.UseSqlServer(connectionString));
            services.AddScoped<IJuegoRepository, JuegoRepository>();
            services.AddScoped<ICategoriaJuegoRepository, CategoriaJuegoRepository>();
            services.AddScoped<IAlquilerRepository, AlquilerRepository>();
            return services;
        }
    }
}
