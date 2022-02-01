using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntifreezeServer.Networking
{

    class SocketClientConnection : IClientConnection
    {

        private MessageProtocol _packetProtocol = new MessageProtocol();
        private Socket _socketConnection;

        public SocketClientConnection(Socket socket)
        {
            _socketConnection = socket;

            _packetProtocol.MessageReceived += _onMessageReceived;

            var t = new Thread(new ThreadStart(_startListening));
            t.Start();
        }

        public event EventHandler<OnMessageEventArgs> OnMessageReceived;

        public event EventHandler<OnConnectionClosedEventArgs> OnClose;

        private void _startListening()
        {

            int bytesRec;
            byte[] buffer = new byte[1024];

            try
            {
                while (_socketConnection.Connected && (bytesRec = _socketConnection.Receive(buffer)) > 0)
                {
                    byte[] data = new byte[bytesRec];
                    Array.Copy(buffer, 0, data, 0, bytesRec);
                    _packetProtocol.DataReceived(data);
                }
            }
            catch (SocketException se)
            {
                // client disconnected forcefully
                // Console.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Close();

        }

        private void _onMessageReceived(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data, 0, data.Length);
            OnMessageReceived?.Invoke(this, new OnMessageEventArgs { Message = str });
        }

        public void Close()
        {

            if (_socketConnection == null) return;

            _socketConnection.Shutdown(SocketShutdown.Both);
            _socketConnection.Close();
            _socketConnection = null;

            OnClose?.Invoke(this, new OnConnectionClosedEventArgs { });

        }

        public void Send(string msg)
        {

            if (_socketConnection == null) return;
            if (!_socketConnection.Connected) return;
            var bytes = Encoding.UTF8.GetBytes(msg);

            _socketConnection.Send(MessageProtocol.WrapData(bytes));

        }

    }

    class SocketServer : INetwork
    {

        private Socket listener;
        private List<SocketClientConnection> _clients = new List<SocketClientConnection>();

        public event EventHandler<ClientConnectedEventArgs> OnClientConnected;

        public void Start(string hostName, int port) 
        {

            IPHostEntry host = Dns.GetHostEntry(hostName);
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8080);

            try
            {

                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Thread t = new Thread(new ThreadStart(_listenIncomingConnections));
                t.Start();

                Console.WriteLine("Socket server listening at {0}:{1}", hostName, port);

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketServer Start Exception: {0}", e);
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }

        }

        private void _listenIncomingConnections()
        {
            try
            {
                while (true)
                {

                    Socket clientSocket = listener.Accept();

                    var clientConnection = new SocketClientConnection(clientSocket);
                    clientConnection.OnClose += (sencer, e) => _clients.Remove(clientConnection);
                    _clients.Add(clientConnection);

                    OnClientConnected?.Invoke(this, new ClientConnectedEventArgs { ClientConnection = clientConnection });

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }


        public void Broadcast(string msg)
        {
            SocketClientConnection client;
            for (int i = 0; i < _clients.Count; i++)
            {
                client = _clients[i];
                client.Send(msg);
            }
        }
    }
}
