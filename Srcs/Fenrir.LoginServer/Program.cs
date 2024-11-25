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
using Fenrir.Network.Util;
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
        logger.LogWarning($"Packet Id {packetInfo.Id:X} is not defined in PacketType enum");
        return;
    }
    PacketType packetType = (PacketType) packetInfo.Id;
    
    logger.LogInformation($"{packetType.GetType().Name} {packetType:X} {packetType} {packetInfo.Name}: {size} bytes");
});



    // var deserializer = (Func<ReadOnlyMemory<byte>, object>)Delegate.CreateDelegate(typeof(Func<ReadOnlyMemory<byte>, object>), genericMethod);
    // var deserializer = (Func<ReadOnlySpan<byte>, object>)Delegate.CreateDelegate(typeof(Func<ReadOnlySpan<byte>, object>), genericMethod);

byte[] buffer = new byte[30];
Span<byte> span = buffer;
Memory<byte> memory = buffer;

LoginHandler.LoginRequestPacket loginRequestPacket = new();
loginRequestPacket.SetUsername("Liam");
loginRequestPacket.SetPassword("Secret");


// Write struct to buffer.
Marshaling.SerializeStructToMemory(loginRequestPacket, memory);

// Hex dump buffer to log.
logger.LogInformation(Utils.HexDump(buffer));


// private static Func<ReadOnlyMemory<byte>, object> GetDeserializer(Type type)
// {
// if (!_deserializers.TryGetValue(type, out var deserializer))
// {

Type type = typeof(LoginHandler.LoginRequestPacket);

//var method = typeof(Marshaling).GetMethod(nameof(Marshaling.DeserializeStructFromSpan), [typeof(ReadOnlyMemory<byte>)]); // , [type]
//var genericMethod = method.MakeGenericMethod(type);

//var deserializer = (Func<ReadOnlyMemory<byte>, object>)Delegate.CreateDelegate(typeof(Func<ReadOnlyMemory<byte>, object>), genericMethod);


var method = typeof(Marshaling).GetMethod(nameof(Marshaling.DeserializeStructFromSpan), new[] { typeof(ReadOnlyMemory<byte>) });
var genericMethod = method.MakeGenericMethod(type);
//var deserializer = (Func<ReadOnlyMemory<byte>, object>)Delegate.CreateDelegate(typeof(Func<ReadOnlyMemory<byte>, object>), genericMethod);

// Please invoke the method.
ReadOnlyMemory<byte> readOnlyMemory = memory;

var oops = genericMethod.Invoke(null, new object[] { readOnlyMemory });
LoginHandler.LoginRequestPacket loginRequestPacket2 = (LoginHandler.LoginRequestPacket)oops;
logger.LogInformation($"Username: {loginRequestPacket2.GetUsername()} Password: {loginRequestPacket2.GetPassword()}");

//var obj = deserializer();




    //     _deserializers[type] = deserializer;
    // }
//     return deserializer;
// }

// var loginServer = host.Services.GetRequiredService<LoginServer>();
//
// await loginServer.StartAsync();

