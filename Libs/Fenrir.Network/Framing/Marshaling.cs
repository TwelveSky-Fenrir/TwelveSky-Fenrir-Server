﻿using System.Runtime.InteropServices;

namespace Fenrir.Network.Framing;

public class Marshaling
{
    // TODO: Custom exceptions.

    public static void SerializeStructToSpan<T>(T value, Span<byte> destination) where T : struct
    {
        // TODO: Do I need this check here, its done inside Write as well?
        if (destination.Length < Marshal.SizeOf<T>())
            throw new ArgumentException("Destination span is too small.");

        MemoryMarshal.Write(destination, ref value);
    }

    public struct Cake
    {
        public int A;
        
    }

    public static T DeserializeStructFromSpan<T>(ReadOnlySpan<byte> source) where T : struct
    {
        // TODO: Do I need this check here, its done inside Read as well?
        if (source.Length < Marshal.SizeOf<T>())
            throw new ArgumentException("Source span is too small.");
        
        // var a= MemoryMarshal.Cast<byte, Cake>(source);
        // if (a[0].A > 10)
        // {
        //     
        // }
        
        
        
        return MemoryMarshal.Read<T>(source);
    }

    // TODO: A way to get the struct usign a reference/span/memory?
    // public static IPacket ReadPacket(Type type, ReadOnlyMemory<byte> source)
    // {
    //     if (!typeof(IPacket).IsAssignableFrom(type))
    //         throw new ArgumentException("Type must implement IPacket", nameof(type));
    //
    //     if (source.Length < Marshal.SizeOf(type))
    //         throw new ArgumentException("Source span is too small.", nameof(source));
    //
    //     
    //     //source.Span.Slice(0, Marshal.SizeOf(type));
    //     // var method = typeof(MemoryMarshal).GetMethod("Read", new[] { typeof(ReadOnlySpan<byte>) });
    //     // var genericMethod = method.MakeGenericMethod(type);
    //     // var packet = genericMethod.Invoke(null, new object[] { source });
    //
    //     return (IPacket)packet;
    // }


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