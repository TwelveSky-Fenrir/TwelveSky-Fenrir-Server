using Fenrir.LoginServer;
using Fenrir.LoginServer.Handlers;
using Fenrir.LoginServer.Network.Dispatcher;
using Fenrir.LoginServer.Network.Framing;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Collections;
using Fenrir.Network.Dispatcher;
using Fenrir.Network.Framing;
using Fenrir.Network.Options;
using Fenrir.Network.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<ISessionCollection<Session<PacketType, MessageMetadata, Packet>>, SessionCollection<MessageMetadata, Packet>>();
        
        services.AddSingleton(provider =>
        {
            var dispatcher = new MessageDispatcher();
            var loginHandler = provider.GetRequiredService<LoginHandler>();
        
            dispatcher.RegisterHandler(PacketType.LoginRequest, loginHandler.HandleLoginAsync);
            return dispatcher;
        });
        
        
        //services.AddSingleton<IMessageDispatcher<PacketType, MessageMetadata, Packet>, MessageDispatcher>();
        
        // TODO: is there a better way ?
        // services.AddSingleton(provider =>
        // {
        //     var dispatcher = provider.GetRequiredService<IMessageDispatcher<PacketType, MessageMetadata, Packet>>();
        //     var loginHandler = provider.GetRequiredService<LoginHandler>();
        //
        //     dispatcher.RegisterHandler(PacketType.LoginRequest, loginHandler.HandleLoginAsync);
        //     return dispatcher;
        // });


        services.AddSingleton<IMessageParser<MessageMetadata>, MessageParser>();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();

        services.Configure<FenrirServerOptions>(options =>
        {
            options.IpAddress = "127.0.0.1";
            options.Port = 11091;
            options.MaxConnections = 100;
            options.EnableKeepAlive = true;
            options.KeepAliveInterval = 10;
            options.EnableLogging = true;
            options.MaxConnectionsByIpAddress = 10;
            // TODO: Nagle?
        });

        services.AddSingleton<LoginServer>();
    })
    .Build();

var loginServer = host.Services.GetRequiredService<LoginServer>();

await loginServer.StartAsync();
