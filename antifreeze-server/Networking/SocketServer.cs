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

    class SocketClientConnection
    {

        // todo ping

        private PacketProtocol _packetProtocol = new PacketProtocol(1024);
        private Socket _socketConnection;

        public SocketClientConnection(Socket socket)
        {
            _socketConnection = socket;

            _packetProtocol.MessageArrived += _onMessageArrived;

            var t = new Thread(new ThreadStart(_startListening));
            t.Start();
        }

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
                // Console.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Close();

        }

        private void _onMessageArrived(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data, 0, data.Length);
            OnMessageReceived?.Invoke(str);
        }

        public void Close()
        {

            Console.WriteLine("Client disconnected!");

            if (_socketConnection == null) return;

            _socketConnection.Shutdown(SocketShutdown.Both);
            _socketConnection.Close();
            _socketConnection = null;

            if (OnClose != null) { OnClose(0); }

        }

        public void Send(string msg)
        {

            if (_socketConnection == null) return;
            if (!_socketConnection.Connected) return;

            var bytes = Encoding.UTF8.GetBytes(msg);

            _socketConnection.Send(PacketProtocol.WrapMessage(bytes));

        }

        public Action<string> OnMessageReceived;

        public Action<int> OnClose;

    }

    class SocketServer
    {

        private Socket listener;
        private List<SocketClientConnection> _clients = new List<SocketClientConnection>();

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
                    Console.WriteLine("Client connected!");
                    var clientConnection = new SocketClientConnection(clientSocket);
                    clientConnection.OnClose += (int status) => _clients.Remove(clientConnection);
                    _clients.Add(clientConnection);
                    OnClientConnected?.Invoke(clientConnection);

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
        public Action<SocketClientConnection> OnClientConnected;
    }
}
