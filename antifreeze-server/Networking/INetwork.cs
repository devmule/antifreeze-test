using System;
using System.Net.Sockets;

namespace AntifreezeServer.Networking
{
    public interface INetwork
    {
        
        public void Start(int port);

        public void Send(TcpClient client, Message message);

        public void Broadcast(Message message);

        public event EventHandler<MessageEventArgs> OnMessageReceived;

        public event EventHandler<ClientConnectedEventArgs> OnClientConnected;

    }

    public class ClientConnectedEventArgs : EventArgs
    {
        public TcpClient Client { get; set; }
    }

}
