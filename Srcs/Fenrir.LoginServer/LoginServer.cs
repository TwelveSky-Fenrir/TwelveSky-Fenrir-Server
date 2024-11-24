using System.Net.Sockets;
using Fenrir.LoginServer.Network.Dispatcher;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Collections;
using Fenrir.Network.Dispatcher;
using Fenrir.Network.Framing;
using Fenrir.Network.Options;
using Fenrir.Network.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fenrir.LoginServer;

//using LoginServerSessionType = Session<MessageMetadata, Packet>;

public class LoginServer : FenrirServer<PacketType, LoginSession, MessageMetadata>
{
    private readonly ILogger<LoginServer> _logger;
    private readonly IMessageDispatcher<PacketType, MessageMetadata, MessageMetadata> _messageDispatcher;
    private readonly IServiceProvider _provider;
    private readonly ISessionCollection<LoginSession> _sessions;

    public LoginServer(
        IOptions<FenrirServerOptions> options,
        IMessageDispatcher<PacketType, MessageMetadata, Packet> messageDispatcher,
        ILoggerFactory loggerFactory,
        IServiceProvider provider,
        ISessionCollection<LoginSession> sessions)
        : base(options, messageDispatcher, loggerFactory, provider, sessions)
    {
        _messageDispatcher = messageDispatcher;
        _logger = loggerFactory.CreateLogger<LoginServer>();
        _provider = provider;
        _sessions = sessions;
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
    protected override Session<PacketType, MessageMetadata, Packet> CreateSession(Socket socket,
        IMessageParser<MessageMetadata> messageParser,
        IMessageDispatcher<PacketType, LoginSession, MessageMetadata> messageDispatcher,
        ILogger logger,
        FenrirServerOptions options)
    {
        // TODO: Factory?
        var session = new LoginSession(socket, messageParser, messageDispatcher, logger, options);

        _logger.LogInformation($"Nouvelle session créée avec l'ID de session : {session.SessionId}");

        return session;
    }
    
    // protected override LoginSession CreateSession(Socket socket, IMessageParser<MessageMetadata> messageParser,
    //     IMessageDispatcher<PacketType, LoginSession, MessageMetadata> messageDispatcher, ILogger logger, FenrirServerOptions options)
    // {
    //     throw new NotImplementedException();
    // }

    protected override bool CanAddSession(LoginSession session)
    {
        var canAdd = !_sessions.IsFull;
        if (!canAdd) _logger.LogWarning("Cannot add session. The session collection is full.");
        return canAdd;
    }

    protected override async Task OnSessionConnectedAsync(LoginSession session)
    {
        _sessions.AddSession(session);
        _logger.LogInformation("Client {SessionId} connected.", session.SessionId);
        await base.OnSessionConnectedAsync(session);
    }

    protected override async Task OnSessionDisconnectedAsync(LoginSession session)
    {
        _sessions.RemoveSession(session.SessionId);
        _logger.LogInformation("Client {SessionId} disconnected.", session.SessionId);
        await base.OnSessionDisconnectedAsync(session);
    }



}