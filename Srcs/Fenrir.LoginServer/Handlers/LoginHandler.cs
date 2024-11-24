using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Transport;
using Microsoft.Extensions.Logging;

namespace Fenrir.LoginServer.Handlers;

public class LoginHandler(ILogger<LoginHandler> logger)
{
    // TODO: Can logger be DI?
    private readonly ILogger<LoginHandler> _logger = logger;

    // TODO: Can packets be structs?
    // TODO: Can use Builder pattern?
    public class LoginPacket : Packet
    {
        public override PacketType PacketType => PacketType.LoginRequest;
        
        public string Username { get; set; }
        public string Password { get; set; }

        public override void Deserialize(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
    
    // TODO: make a MySession Session<MessageMetadata, Packet>
    // TODO: can this be handled better?
    public ValueTask HandleLoginAsync(Session<MessageMetadata, Packet> session, Packet packet)
    {
        if (packet is LoginHandler.LoginPacket loginPacket)
        {
            return HandleLoginAsync(session, loginPacket);
        }
        else
        {
            return ValueTask.FromException(new InvalidOperationException("Invalid Packet Type"));
        }
    }
    
    public ValueTask HandleLoginAsync(Session<MessageMetadata, Packet> session, LoginPacket packet)
    {
        // TODO: Validation? Throw if bad content?
        _logger.LogInformation("Handling login request for {Username}", packet.Username);
        return ValueTask.CompletedTask;
    }
}