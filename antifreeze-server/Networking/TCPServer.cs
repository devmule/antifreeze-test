using AntifreezeServer.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntifreezeServer.Networking
{

    class TCPClientConnection : IClientConnection
    {

        private Socket _socket;
        private Stream _stream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        public TCPClientConnection (Socket socket)
        {
            _socket = socket;
            _stream = new NetworkStream(_socket);
            _streamReader = new StreamReader(_stream);
            _streamWriter = new StreamWriter(_stream);

            Thread t = new Thread(new ThreadStart(_listen));
            t.Start();

        }

        private void _listen()
        {

            try
            {
                string message;
                while (_socket != null && _socket.Connected)
                {
                    message = _streamReader.ReadLine();
                    OnMessageReceived?.Invoke(this, new OnMessageEventArgs
                    {
                        Message = message
                    });
                }
            }
            catch (IOException e)
            {
                // client force disconnection
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Close();
            }

        }

        public event EventHandler<OnMessageEventArgs> OnMessageReceived;
        public event EventHandler<OnConnectionClosedEventArgs> OnClose;

        public void Close()
        {
            if (_socket == null) return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _stream.Close();

            _socket = null;
            _stream = null;
            _streamReader = null;
            _streamWriter = null;

            OnClose?.Invoke(this, new OnConnectionClosedEventArgs());
        }

        public void Send(string message)
        {
            if (_streamWriter == null) return;
            try
            {
                _streamWriter.WriteLine(message);
            }
            catch
            {
                Close();
            }
        }
    }

    class TCPServer : INetwork
    {

        private TcpListener _listener;
        private List<TCPClientConnection> _clients = new List<TCPClientConnection>();

        private void _service()
        {

            while (true)
            {
                Socket socket = _listener.AcceptSocket();

                var client = new TCPClientConnection(socket);
                _clients.Add(client);

                client.OnClose += (sender, e) => 
                {
                    if (_clients.Contains(client)) _clients.Remove(client);
                };

                OnClientConnected?.Invoke(this, new ClientConnectedEventArgs 
                { 
                    ClientConnection = client 
                });

            }
        }

        public event EventHandler<ClientConnectedEventArgs> OnClientConnected;

        public void Start(string hostName, int port)
        {

            IPHostEntry host = Dns.GetHostEntry(hostName);
            IPAddress ipAddress = host.AddressList[0];
            _listener = new TcpListener(ipAddress, port);
            _listener.Start();

            Console.WriteLine("Server mounted, listening to port {0}", port);

            Thread t = new Thread(new ThreadStart(_service));
            t.Start();

        }

        public void Broadcast(string message)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];
                client.Send(message);
            }
        }
    }
}
