using System.Reflection;
using System.Runtime.InteropServices;
using Fenrir.Network.Framing;
using Fenrir.Network.Helpers;

namespace Fenrir.Network.Collections;

public class PacketInfo
{
    public byte Id { get; }
    public Type? PacketType { get; }
    public String Name { get; }
    public uint Size { get; }
    public bool IsCompressible { get; }
    
    // TODO: Get the packet id by attribute?

    public PacketInfo(byte id)
    {
        Id = id;
        PacketType = null;
        Name = id.ToString("X");
        Size = 0;
        IsCompressible = false;
    }

    public PacketInfo(Type packetType)
    {
        if (!typeof(IPacket).IsAssignableFrom(packetType))
        {
            throw new ArgumentException("Packet Type must implement IPacket.");
        }

        var attribute = packetType.GetCustomAttribute<PacketAttribute>();
        if (attribute == null)
        {
            throw new ArgumentException("Packet Type must have a PacketAttribute.");
        }

        if (attribute.PacketId == null)
        {
            throw new ArgumentException("PacketAttribute must have a PacketId.");
        }
        // System.InvalidCastException: Unable to cast object of type 'Fenrir.LoginServer.Network.Metadata.PacketType' to type 'System.Nullable`1[System.Byte]'.
        
        Id = (byte)attribute.PacketId;
        PacketType = packetType;

        Name = packetType.Name;
        Size = (uint)Marshal.SizeOf(packetType);

        IsCompressible = attribute.IsCompressible;
    }
    
    public PacketInfo(byte id, Type? packetType)
    {
        Id = id;
        PacketType = packetType;

        if (packetType != null)
        {
            if (!typeof(IPacket).IsAssignableFrom(packetType))
            {
                throw new ArgumentException("Packet Type must implement IPacket.");
            }

            Name = packetType.Name;
            Size = (uint)Marshal.SizeOf(packetType);
        }
        else
        {
            Name = id.ToString("X");
            Size = 0;
        }

        IsCompressible = false;
    }
    
}

public class PacketCollection
{
    private readonly Dictionary<byte, PacketInfo> _packets = new();

    public void RegisterPacket(byte id)
    {
        if (!_packets.TryAdd(id, new PacketInfo(id)))
        {
            throw new ArgumentException($"Packet with ID {id} already exists.");
        }
    }
    
    public void RegisterPacket(PacketInfo packetInfo)
    {
        if (!_packets.TryAdd(packetInfo.Id, packetInfo))
        {
            throw new ArgumentException($"Packet with ID {packetInfo.Id} already exists.");
        }
    }
  
    public PacketInfo GetPacketInfo(byte id)
    {
        if (_packets.TryGetValue(id, out var packetInfo))
        {
            return packetInfo;
        }
        throw new KeyNotFoundException($"Packet with ID {id} not found.");
    }

    public void ForEach(Action<PacketInfo> action)
    {
        foreach (var packet in _packets.Values)
        {
            action(packet);
        }
    }
}
