using System;
using Api.Mappings;
using Mapster;
using MapsterMapper;

namespace Api.Extensions;

public static class MapsterExtensions
{
    public static IServiceCollection AddMapsterConfiguration(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.Scan(typeof(PollMappingConfig).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
