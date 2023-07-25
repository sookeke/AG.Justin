using AG.Justin.Infrastructure.Repository;
using AG.Justin.Infrastructure.Repository.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AG.Justin.Infrastructure;
public static class DependencyInjection 
{
    public static IServiceCollection AddCustomClaims(this IServiceCollection services, IConfiguration configuration)
    {

        //implement 
        services.AddScoped<IParticipantRepository>(provider =>
            new ParticipantRepository(configuration.GetConnectionString("JustinConnectionString")!));
        return services;
    }   
}
