using System.Net;
using System.Net.Sockets;
using Fenrir.LoginServer.Network;
using Fenrir.LoginServer.Network.Dispatcher;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Collections;
using Fenrir.Network.Dispatcher;
using Fenrir.Network.Framing;
using Fenrir.Network.Options;
using Fenrir.Network.Transport;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fenrir.LoginServer;

//using LoginServerSessionType = Session<MessageMetadata, Packet>;

public class LoginServer : FenrirServer
{
    private readonly ILogger<LoginServer> _logger;
    //private readonly IMessageDispatcher<PacketType, MessageMetadata, MessageMetadata> _messageDispatcher;
    private readonly IServiceProvider _provider;
    // private readonly ISessionCollection<LoginSession> _sessions;
//         ISessionCollection<LoginSession> sessions
    public LoginServer(
        IOptions<FenrirServerOptions> options,
        //IMessageDispatcher<PacketType, MessageMetadata, Packet> messageDispatcher,
        ILoggerFactory loggerFactory,
        IServiceProvider provider)
        : base(options, loggerFactory, provider)
    {
        //_messageDispatcher = messageDispatcher;
        _logger = loggerFactory.CreateLogger<LoginServer>();
        _provider = provider;
    }

    // protected override Session<PacketType, MessageMetadata, Packet> CreateSession(Socket socket,
    //     IMessageParser<MessageMetadata> messageParser,
    //     IMessageDispatcher<PacketType, MessageMetadata, Message<PacketType>> messageDispatcher,
    //     ILogger logger, FenrirServerOptions options)
    // {
    //     var session = new LoginSession(socket, messageParser, messageDispatcher, logger, options);
    //
    //     _logger.LogInformation($"Nouvelle session créée avec l'ID de session : {session.SessionId}");
    //
    //     return session;
    // }
    protected override IClient CreateClient(Socket socket)
    {
        // TODO: How to pass Socket, Session, Logger, Server, Options etc into here?
        throw new NotImplementedException();
        return new LoginClient();
    }

    protected override ISession CreateSession(string sessionId,
        //IMessageParser<MessageMetadata> messageParser,
        //IMessageDispatcher<PacketType, LoginSession, MessageMetadata> messageDispatcher,
        IPEndPoint endpoint,
        ILogger logger, FenrirServerOptions options)
    {
        // TODO: Factory?
        var session = new LoginSession(endpoint, logger, options);

        _logger.LogInformation($"New session created with session ID: {session.SessionId}");

        return session;
    }
    
    // protected override LoginSession CreateSession(Socket socket, IMessageParser<MessageMetadata> messageParser,
    //     IMessageDispatcher<PacketType, LoginSession, MessageMetadata> messageDispatcher, ILogger logger, FenrirServerOptions options)
    // {
    //     throw new NotImplementedException();
    // }

    protected override bool CanAddSession(ISession session)
    {
        // TODO: Fix?
        throw new NotImplementedException();
        // var canAdd = !_sessions.IsFull;
        // if (!canAdd) _logger.LogWarning("Cannot add session. The session collection is full.");
        // return canAdd;
    }

    protected override async Task OnSessionConnectedAsync(ISession session)
    {
        // TODO: Fix.
        throw new NotImplementedException();
        // _sessions.AddSession(session);
        // _logger.LogInformation("Client {SessionId} connected.", session.SessionId);
        // await base.OnSessionConnectedAsync(session);
    }

    protected override async Task OnSessionDisconnectedAsync(ISession session)
    {
        // TODO: Fix
        throw new NotImplementedException();
        // _sessions.RemoveSession(session.SessionId);
        // _logger.LogInformation("Client {SessionId} disconnected.", session.SessionId);
        // await base.OnSessionDisconnectedAsync(session);
    }



}