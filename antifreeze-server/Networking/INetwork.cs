using System;
using System.Net.Sockets;

namespace AntifreezeServer.Networking
{
    public interface INetwork
    {
        
        public void Start(string host, int port);

        public void Broadcast(string message);

        public event EventHandler<ClientConnectedEventArgs> OnClientConnected;

    }

    public interface IClientConnection
    {

        public void Close();

        public void Send(string message);

        public Action<string> OnMessageReceived { get; set; }

        public Action<int> OnClose { get; set; }

    }

    public class ClientConnectedEventArgs : EventArgs
    {
        public IClientConnection ClientConnection { get; set; }
    }

}
