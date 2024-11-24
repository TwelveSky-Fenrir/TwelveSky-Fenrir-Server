using System.Runtime.InteropServices;

namespace Fenrir.Network.Framing;

public class Marshaling
{
    // TODO: Custom exceptions.

    public static void SerializeStructToSpan<T>(T value, Span<byte> destination) where T : struct
    {
        if (destination.Length < Marshal.SizeOf<T>())
            throw new ArgumentException("Destination span is too small.");

        MemoryMarshal.Write(destination, ref value);
    }

    public static T DeserializeStructFromSpan<T>(ReadOnlySpan<byte> source) where T : struct
    {
        if (source.Length < Marshal.SizeOf<T>())
            throw new ArgumentException("Source span is too small.");

        return MemoryMarshal.Read<T>(source);
    }
    
    // Note: Fallback if MemoryMarshal is not available.
    // public static byte[] StructToBytes<T>(T obj) where T : struct
    // {
    //     int size = Marshal.SizeOf<T>();
    //     byte[] buffer = new byte[size];
    //     IntPtr ptr = Marshal.AllocHGlobal(size);
    //
    //     try
    //     {
    //         Marshal.StructureToPtr(obj, ptr, false);
    //         Marshal.Copy(ptr, buffer, 0, size);
    //     }
    //     finally
    //     {
    //         Marshal.FreeHGlobal(ptr);
    //     }
    //
    //     return buffer;
    // }
    
    // TODO: Fallback stream read/write or bitconverter?
}