using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AntifreezeServer.Networking
{

    class TcpServer : INetwork
    {

        private TcpListener _tcpListener = null;
        private List<TcpClient> _tcpClientsList = new List<TcpClient>();


        public void Start(int port)
        {

            if (_tcpListener != null) return;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress localAddr = ipHostInfo.AddressList[0];
            _tcpListener = new TcpListener(localAddr, port);
            _tcpListener.Start();

            // listening connections in separate thread
            Console.WriteLine("Tcp server listening at port {0}", port);
            Thread t = new Thread(new ThreadStart(_startListening));
            t.Start();

        }

        private void _startListening()
        {
            try
            {
                while (true)
                {

                    TcpClient client = _tcpListener.AcceptTcpClient();
                    Console.WriteLine("Client connected!");

                    Thread t = new Thread(new ParameterizedThreadStart(_handleClient));
                    t.Start(client);

                    _tcpClientsList.Add(client);

                    // emit connection
                    ClientConnectedEventArgs e = new ClientConnectedEventArgs();
                    e.Client = client;
                    if (OnClientConnected != null) OnClientConnected(this, e);

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                _tcpListener.Stop();
            }
        }

        private void _handleClient(Object obj)
        {
            var client = (TcpClient)obj;
            var stream = client.GetStream();

            string str = null;
            Byte[] bytes = new Byte[256];
            int i;

            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    str = Encoding.ASCII.GetString(bytes, 0, i);
                    Message message = MessageSerializator.Deserialize(str);

                    // emit Message
                    var e = new MessageEventArgs();
                    e.Message = message;
                    if (OnMessageReceived != null) OnMessageReceived(this, e);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("HandleClient Exception: {0}", e.ToString());
            }

            client.Close();
            _tcpClientsList.Remove(client);

            Console.WriteLine("Client disconnected!");

        }

        public void Broadcast(Message message)
        {

            foreach (TcpClient client in _tcpClientsList) Send(client, message);

        }

        public void Send(TcpClient client, Message message)
        {

            string str = MessageSerializator.Serialize(message);
            Byte[] data = Encoding.ASCII.GetBytes(str);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

        }

        public event EventHandler<MessageEventArgs> OnMessageReceived;

        public event EventHandler<ClientConnectedEventArgs> OnClientConnected;

    }


}
