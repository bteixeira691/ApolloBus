using ApolloBus.InterfacesAbstraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApolloBus.StartupServices
{
    public static class RegisterHandlers
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            var handleTypes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany
                (
                    a => a.GetTypes().Where
                    (
                        x => !x.IsInterface &&
                             !x.IsAbstract &&
                             x.GetInterfaces().Any(y => y.Name.Equals(typeof(IEventHandler<>).Name, StringComparison.InvariantCulture))
                    )
                );

            foreach (var type in handleTypes)
            {
                services.AddTransient(type);
            }
            return services;
        }
    }
}
