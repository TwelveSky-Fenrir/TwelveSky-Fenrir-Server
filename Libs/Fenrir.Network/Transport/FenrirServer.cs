using System.Net.Sockets;
using Fenrir.Network.Collections;
using Fenrir.Network.Dispatcher;
using Fenrir.Network.Framing;
using Fenrir.Network.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fenrir.Network.Transport;

/// <summary>Represents a tcp server that can be used to listen for incoming connections.</summary>
/// <typeparam name="TMessage">The type of the message.</typeparam>
/// <typeparam name="TPacketType"></typeparam>
public abstract class FenrirServer<TPacketType, TMessage>
    where TMessage : struct // TODO: Figure out
{
    private readonly CancellationTokenSource _cts;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMessageDispatcher<TPacketType, TMessage> _messageDispatcher;
    private readonly FenrirServerOptions _options;
    private readonly IServiceProvider _provider;
    private readonly Socket _socket;
    private readonly PeriodicTimer _timer;
    private readonly List<IClient> _clients;

    /// <summary>Initializes a new instance of the <see cref="FenrirServer{ISession,TMessage}" /> class.</summary>
    /// <param name="options">The server options.</param>
    /// <param name="messageDispatcher">The message dispatcher.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="provider">The service provider.</param>
    /// <param name="sessions">The session collection.</param>
    protected FenrirServer(
        IOptions<FenrirServerOptions> options,
        IMessageDispatcher<TPacketType, TMessage> messageDispatcher,
        ILoggerFactory loggerFactory,
        IServiceProvider provider,
        ISessionCollection<ISession> sessions)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cts = new CancellationTokenSource();
        _options = options.Value;
        _messageDispatcher = messageDispatcher;
        _loggerFactory = loggerFactory;
        _provider = provider;
        _logger = loggerFactory.CreateLogger("Fenrir.Transport.FenrirServer");
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.KeepAliveInterval));
        Sessions = sessions;
        _clients = new List<IClient>(); // TODO: Can use struct or span?
    }

    /// <summary>Gets the session collection of type <typeparamref name="ISession" />.</summary>
    public ISessionCollection<ISession> Sessions { get; }

    /// <summary>Starts the server asynchronously.</summary>
    public async Task StartAsync()
    {
        var endpoint = _options.GetRemoteEndPoint();

        try
        {
            _socket.Bind(endpoint);
        }
        catch (SocketException e)
        {
            _logger.LogError(e, "Failed to bind to {EndPoint}", endpoint);
            throw;
        }

        _socket.Listen(_options.MaxConnections);

        _logger.LogInformation("Listening on {EndPoint}", endpoint);

        if (_options.EnableKeepAlive)
            _ = KeepAliveAsync();

        while (!_cts.IsCancellationRequested)
        {
            var clientSocket = await _socket.AcceptAsync(_cts.Token).ConfigureAwait(false);
            var logger = _loggerFactory.CreateLogger("Fenrir.Transport.FenrirSession");
            var messageParser = _provider.GetRequiredService<IMessageParser<TMessage>>();
            
            // TODO: Remove socket from session.
            var session = CreateSession(clientSocket, messageParser, _messageDispatcher, logger, _options);

            // TODO: Client Factory?
            var client = CreateClient(clientSocket);

            if (!CanAddSession(session))
            {
                await session.DisposeAsync().ConfigureAwait(false);
                continue;
            }

            Sessions.AddSession(session);

            _ = OnSessionConnectedAsync(session)
                .ContinueWith(_ => OnSessionConnectedAsync(session), _cts.Token)
                .Unwrap()
                .ContinueWith(_ => session.ReceiveAsync(), _cts.Token)
                .Unwrap()
                .ContinueWith(_ => OnSessionDisconnectedAsync(session), _cts.Token)
                .Unwrap()
                .ContinueWith(_ => session.DisposeAsync().AsTask(), _cts.Token)
                .Unwrap()
                .ContinueWith(_ => Sessions.RemoveSession(session.SessionId), _cts.Token)
                .ConfigureAwait(false);
        }
    }

    protected abstract IClient CreateClient(Socket socket);

    /// <summary>Initializes a new instance of the <see cref="ISession" /> class.</summary>
    /// <param name="socket">The bound socket.</param>
    /// <param name="messageParser">The message parser.</param>
    /// <param name="messageDispatcher">The message dispatcher.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The server options.</param>
    protected abstract ISession CreateSession(
        Socket socket,
        IMessageParser<TMessage> messageParser,
        IMessageDispatcher<TPacketType, ISession, TMessage> messageDispatcher,
        ILogger logger,
        FenrirServerOptions options);

    private async Task KeepAliveAsync()
    {
        while (await _timer.WaitForNextTickAsync(_cts.Token).ConfigureAwait(false))
        {
            if (_cts.IsCancellationRequested)
                return;

            await Sessions.ExecuteAsync(x =>
            {
                Sessions.RemoveSession(x.SessionId);
                return x.DisposeAsync();
            }, x => !x.IsConnected).ConfigureAwait(false);
        }
    }

    /// <summary>Determines whether the session can be added to the session collection.</summary>
    /// <param name="session">The session to add.</param>
    /// <returns><see langword="true" /> if the session can be added; otherwise, <see langword="false" />.</returns>
    protected virtual bool CanAddSession(ISession session)
    {
        return !Sessions.IsFull &&
               Sessions.CountSessions(x =>
                   x.RemoteEndPoint.Address.ToString().Equals(session.RemoteEndPoint.Address.ToString(),
                       StringComparison.InvariantCultureIgnoreCase)) < _options.MaxConnectionsByIpAddress;
    }

    /// <summary>Called when a session is connected.</summary>
    /// <param name="session">The connected session.</param>
    protected virtual Task OnSessionConnectedAsync(ISession session)
    {
        _logger.LogInformation("Session ({Name}) connected from {EndPoint}", session, session.RemoteEndPoint);
        return Task.CompletedTask;
    }

    /// <summary>Called when a session is disconnected.</summary>
    /// <param name="session">The session will be disconnected.</param>
    protected virtual Task OnSessionDisconnectedAsync(ISession session)
    {
        _logger.LogInformation("Session ({Name}) disconnected from {EndPoint}", session, session.RemoteEndPoint);
        return Task.CompletedTask;
    }
}