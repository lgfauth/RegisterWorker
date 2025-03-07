using Application.Injections;
using Domain.Settings;
using RegisterWorker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;
        config.AddEnvironmentVariables().AddUserSecrets<Program>();        
    })
    .ConfigureServices((context, services) =>
    {
        var variables = context.Configuration.Get<EnvirolmentVariables>();
        services.AddSingleton(variables);

        DependenceInjections.Injections(services, variables.MONGODBSETTINGS_CONNECTIONSTRING);
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
