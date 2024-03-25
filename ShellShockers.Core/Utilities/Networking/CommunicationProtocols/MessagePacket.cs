namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols;

public class MessagePacket<T> where T : class
{
	public MessageType Type{ get; set; }
	public T Payload { get; set; }

    public MessagePacket(MessageType type, T payload)
    {
        Type = type;
        Payload = payload;
    }
}