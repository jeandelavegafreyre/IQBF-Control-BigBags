using IQBF.Application.Interfaces;
using IQBF.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IQBF.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IShipService, ShipService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBLService, BLService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IReceptionService, ReceptionService>();
        services.AddScoped<IDispatchService, DispatchService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
