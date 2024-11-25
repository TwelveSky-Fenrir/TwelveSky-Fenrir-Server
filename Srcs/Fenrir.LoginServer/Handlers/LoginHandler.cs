using System.Runtime.InteropServices;
using System.Text;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Framing;
using Fenrir.Network.Helpers;
using Fenrir.Network.Transport;
using Microsoft.Extensions.Logging;

namespace Fenrir.LoginServer.Handlers;

public class LoginHandler(ILogger<LoginHandler> logger)
{
    // TODO: Can logger be DI?
    private readonly ILogger<LoginHandler> _logger = logger;

    // TODO: Can packets be structs?
    // TODO: Can use Builder pattern?

    // https://stackoverflow.com/questions/46279646/safe-fixed-size-array-in-struct-c-sharp
    
    
    // [Packet(PacketType.LoginRequest)]
    // [StructLayout(LayoutKind.Sequential, Pack = 1)]
    // public struct Ts2PacketCompressed : Ts2Packet
    // {
    //     public bool isCompressed;
    //     public uint compressedSize;
    //     public uint uncompressedSize;
    //     
    //     // Note: Decompression must make a copy of the data.
    //     [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(CompressedDataMarshaler))]
    //     public byte[] data;
    //     //
    //     // public Ts2Packet Decompress()
    //     // {
    //     //     if (!isCompressed)
    //     //     {
    //     //         return this;
    //     //     }
    //     //     
    //     //     byte[] decompressedData = CompressionHelper.Decompress(data, compressedSize, uncompressedSize);
    //     //     return new Ts2Packet
    //     //     {
    //     //         packetType = packetType,
    //     //         unknown1 = unknown1,
    //     //         unknown2 = unknown2,
    //     //         data = decompressedData
    //     //     };
    //     // }
    // }
    //
    // public class CompressedDataMarshaler : ICustomMarshaler
    // {
    //     private static readonly CompressedDataMarshaler _instance = new CompressedDataMarshaler();
    //
    //     public static ICustomMarshaler GetInstance(string cookie) => _instance;
    //
    //     public void CleanUpManagedData(object ManagedObj) { }
    //
    //     public void CleanUpNativeData(IntPtr pNativeData)
    //     {
    //         Marshal.FreeHGlobal(pNativeData);
    //     }
    //
    //     public int GetNativeDataSize() => -1;
    //
    //     public IntPtr MarshalManagedToNative(object ManagedObj)
    //     {
    //         if (ManagedObj is byte[] data)
    //         {
    //             IntPtr pNativeData = Marshal.AllocHGlobal(data.Length);
    //             Marshal.Copy(data, 0, pNativeData, data.Length);
    //             return pNativeData;
    //         }
    //         return IntPtr.Zero;
    //     }
    //
    //     public object MarshalNativeToManaged(IntPtr pNativeData)
    //     {
    //         // This method should not be used directly.
    //         throw new NotImplementedException();
    //     }
    // }
    
    
    
    [Packet(PacketType.LoginRequest)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LoginRequestPacket : IPacket
    {
        // Note; Try StringBuilder?
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        public byte[] name;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        public byte[] password;
  
        // TODO: A better way to read/write these strings.
        // Unfortunately in C# char is not good here because it is UTF-16.
        // Game pretty much always uses ASCII but maps its self as needed into some other charset in specific language clients.
        // Although most I've seen have been custom sets and they generally have overlap with ASCII.
        public string Username => Encoding.ASCII.GetString(name).TrimEnd('\0');
        public string Password => Encoding.ASCII.GetString(password).TrimEnd('\0');
    }

    // public class LoginPacket : Packet
    // {
    //     public override PacketType PacketType => PacketType.LoginRequest;
    //     
    //     public string Username { get; set; }
    //     public string Password { get; set; }
    //
    //     public override void Deserialize(BinaryReader reader)
    //     {
    //         throw new NotImplementedException();
    //     }
    //
    //     public override void Serialize(BinaryWriter writer)
    //     {
    //         throw new NotImplementedException();
    //     }
    // }
    //
    // TODO: make a MySession Session
    // TODO: can this be handled better?
    // public ValueTask HandleLoginAsync(Session session, IPacket packet)
    // {
    //     return HandleLoginAsync(session, (LoginRequestPacket)packet);
    // }
    
    [PacketHandler]
    public ValueTask HandleLoginAsync(Session session, LoginRequestPacket packet)
    {
        // TODO: Validation? Throw if bad content?
        _logger.LogInformation("Handling login request for {Username}", packet.Username);
        return ValueTask.CompletedTask;
    }
}