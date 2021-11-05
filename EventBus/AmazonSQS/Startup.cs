using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.SQS;
using ApolloBus.AmazonSQS.Model;
using ApolloBus.AmazonSQS.Model.Interfaces;
using ApolloBus.InterfacesAbstraction;
using ApolloBus.Polly;
using ApolloBus.StartupServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.AmazonSQS
{
    public static class Startup
    {
        public static void AddAmazonSQS(this IServiceCollection services, IConfiguration configuration)
        {

            //ServiceBusClientOptions serviceBusClientOptions = configuration.GetSection("ServiceBus:ServiceBusClientOptions").Get<ServiceBusClientOptions>();

            //ServiceBusProcessorOptions serviceBusProcessorOptions = configuration.GetSection("ServiceBus:ServiceBusProcessorOptions").Get<ServiceBusProcessorOptions>();


            ComplementaryConfig complementaryConfig = configuration.GetSection("AmazonSQS:ComplementaryConfig").Get<ComplementaryConfig>();
            Credentials aWSOptions = configuration.GetSection("AmazonSQS:Credentials").Get<Credentials>();

            //string complementaryConfigValid = complementaryConfig.IsValid(); 
            //if (complementaryConfigValid != string.Empty)
            //{
            //    Log.Logger.Error(complementaryConfigValid);
            //    throw new Exception(complementaryConfigValid);
            //}


            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();
            services.AddSingleton(Log.Logger);

            services.AddSingleton<IComplementaryConfigAmazonSQS, ComplementaryConfig>();

            services.AddSingleton<IPollyPolicy, PollyPolicy>(sp =>
            {
                var cConfig = sp.GetRequiredService<IComplementaryConfigAmazonSQS>();
                var logger = sp.GetRequiredService<ILogger>();
                return new PollyPolicy(logger, cConfig);
            });


            AWSOptions awsOptions = new AWSOptions
            {
                Credentials = new Amazon.Runtime.BasicAWSCredentials(aWSOptions.AccessKey, aWSOptions.SecretKey),
                Region = Amazon.RegionEndpoint.GetBySystemName("us-east-2")
            };
            services.AddAWSService<IAmazonSQS>(awsOptions);

            services.AddSingleton<AmazonSQSConnection>(sp =>
            {
                var amazonSQS = sp.GetRequiredService<IAmazonSQS>();
                var logger = sp.GetRequiredService<ILogger>();
                var polly = sp.GetRequiredService<IPollyPolicy>();
                return new AmazonSQSConnection(complementaryConfig, amazonSQS, logger, polly);
            });
                



            services.AddSingleton<IApolloBus, ApolloBusAmazonSQS>(sp =>
            {
                var amazonSQSConnection = sp.GetRequiredService<AmazonSQSConnection>();
                var logger = sp.GetRequiredService<ILogger>();
                var subcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                var serviceProvider = sp.GetRequiredService<IServiceScopeFactory>();
                var amazonSQS = sp.GetRequiredService<IAmazonSQS>();
                return new ApolloBusAmazonSQS(serviceProvider, subcriptionsManager, logger, amazonSQSConnection, amazonSQS);
            });

            RegisterHandlers.AddHandlers(services);
            HangfireServices.AddHangfireServices(services, configuration);
        }
    }
}
