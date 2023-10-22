using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Genocs.Microservice.Template.Infrastructure.Validations;
public static class Extensions
{
    public static IServiceCollection Behaviors(this IServiceCollection services, Assembly assemblyContainingValidators)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
