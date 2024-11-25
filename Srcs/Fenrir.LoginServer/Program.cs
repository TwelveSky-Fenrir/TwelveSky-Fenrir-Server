using System.Runtime.InteropServices;
using Fenrir.LoginServer;
using Fenrir.LoginServer.Handlers;
using Fenrir.LoginServer.Network.Dispatcher;
using Fenrir.LoginServer.Network.Framing;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Collections;
using Fenrir.Network.Dispatcher;
using Fenrir.Network.Framing;
using Fenrir.Network.Helpers;
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
        services.AddSingleton<ILoggerFactory, LoggerFactory>();

        // // services.AddSingleton<ISessionCollection<Session<PacketType, MessageMetadata, Packet>>, SessionCollection<MessageMetadata, Packet>>();
        // //
        // services.AddSingleton(_ => {
        //     var dispatcher = new MessageDispatcher();
        //     
        //     // PacketRegistration.RegisterPackets<PacketType, Packet>(dispatcher, "Fenrir.LoginServer.Handlers", "Fenrir.LoginServer.Packets");
        //     
        //     //var loginHandler = provider.GetRequiredService<LoginHandler>();
        //     // dispatcher.RegisterHandler(PacketType.LoginRequest, loginHandler.HandleLoginAsync);
        //     return dispatcher;
        // });

        services.AddSingleton(_ => {
            var packetCollection = new PacketCollection();
            PacketRegistration.RegisterPacketsToCollection(packetCollection, "Fenrir.LoginServer.Packets");
            PacketRegistration.RegisterPacketsToCollection(packetCollection, "Fenrir.LoginServer.Handlers");
            return packetCollection;
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


        // services.AddSingleton<IMessageParser<MessageMetadata>, MessageParser>();
        //
        // services.Configure<FenrirServerOptions>(options =>
        // {
        //     options.IpAddress = "127.0.0.1";
        //     options.Port = 11091;
        //     options.MaxConnections = 100;
        //     options.EnableKeepAlive = true;
        //     options.KeepAliveInterval = 10;
        //     options.EnableLogging = true;
        //     options.MaxConnectionsByIpAddress = 10;
        //     // TODO: Nagle?
        // });
        //
        // services.AddSingleton<LoginServer>();
    })
    .Build();

// Get logger factory
var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("LoginServer");

logger.LogInformation("Starting Login Server");

// TODO: Add test that checks size of all packets recv and send.
// int size = Marshal.SizeOf<LoginHandler.LoginPacketData>();
// Console.WriteLine($"Size of LoginPacketData: {size} bytes");
var packetCollection = host.Services.GetRequiredService<PacketCollection>();
packetCollection.ForEach(packetInfo =>
{
    var size = Marshal.SizeOf(packetInfo.PacketType);
    
    // If Packet Id is in PacketType
    if (!Enum.IsDefined(typeof(PacketType), packetInfo.Id))
    {
        logger.LogWarning($"Packet Id {packetInfo.Id} is not defined in PacketType enum");
        return;
    }
    PacketType packetType = (PacketType) packetInfo.Id;
    
    logger.LogInformation($"{packetType.GetType().Name} {packetType} {packetInfo.Name}: {size} bytes");
});

// var loginServer = host.Services.GetRequiredService<LoginServer>();
//
// await loginServer.StartAsync();

