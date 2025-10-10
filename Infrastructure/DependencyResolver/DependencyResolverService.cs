using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.Repositories;

namespace Infrastructure.DependencyResolver;

public static class DependencyResolverService
{
    public static void RegisterInfrastructureLayer(IServiceCollection services)
    {
        services.AddScoped<IInstruksRepository, InstruksRepository>();
        services.AddScoped<IInstruksRepository, InstruksRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
    }
}