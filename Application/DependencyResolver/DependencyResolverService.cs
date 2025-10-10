using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyResolver;

public static class DependencyResolverService
{
    public static void RegisterApplicationLayer(IServiceCollection services)
    {
        services.AddScoped<IInstruksPdfService, InstruksPdfService>();
        services.AddScoped<IInstruksService, InstruksService>();
        services.AddScoped<ICategoryService, CategoryService>();
        
    }
}