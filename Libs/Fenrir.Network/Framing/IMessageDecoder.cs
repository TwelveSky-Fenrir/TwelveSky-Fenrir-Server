using System.Buffers;

namespace Fenrir.Network.Framing;

/// <summary>Describes a way to decode a network messages.</summary>
/// <typeparam name="TMessage">The type of the message.</typeparam>
public interface IMessageDecoder<out TMessage>
{
	/// <summary>Decodes all message from the given <paramref name="sequence" />.</summary>
	/// <param name="sequence">The sequence buffer.</param>
	/// <param name="isReadByServer">Whether the message is read by a server session.</param>
	/// <returns>A collection of network message of type <typeparamref name="TMessage" />.</returns>
	IEnumerable<TMessage> DecodeMessages(ReadOnlySequence<byte> sequence, bool isReadByServer);
}