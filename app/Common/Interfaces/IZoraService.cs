using Microsoft.Extensions.DependencyInjection;

namespace zora.Common.Interfaces
{
    public interface IZoraService { }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddZoraServices(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<IZoraService>()
                .AddClasses(classes => classes.AssignableTo<IZoraService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}
