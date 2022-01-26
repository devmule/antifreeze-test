using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AntifreezeServer
{

    class ServerListener
    {

        private TcpListener Server = null;
        private List<TcpClient> TcpClientsList = new List<TcpClient>();


        public void Start(int port)
        {

            if (Server != null) return;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress localAddr = ipHostInfo.AddressList[0];
            Server = new TcpListener(localAddr, port);
            Server.Start();

            // listening connections in separate thread
            Console.WriteLine("Listening at port {0}", port);
            Thread t = new Thread(new ThreadStart(Listening));
            t.Start();

        }

        private void Listening()
        {
            try
            {
                while (true)
                {

                    TcpClient client = Server.AcceptTcpClient();
                    Console.WriteLine("Client connected!");

                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(client);

                    TcpClientsList.Add(client);

                    // emit connection
                    ClientConnectedEventArgs e = new ClientConnectedEventArgs();
                    e.Client = client;
                    if (OnClientConnected != null) OnClientConnected(this, e);

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                Server.Stop();
            }
        }

        private void HandleClient(Object obj)
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
                    MessageReceivedEventArgs e = new MessageReceivedEventArgs();
                    e.Message = message;
                    if (OnMessageReceived != null) OnMessageReceived(this, e);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("HandleClient Exception: {0}", e.ToString());
            }

            client.Close();
            TcpClientsList.Remove(client);

            Console.WriteLine("Client disconnected!");

        }

        public void Broadcast(Message message)
        {

            foreach (TcpClient client in TcpClientsList) Send(client, message);

        }

        public void Send(TcpClient client, Message message)
        {

            string str = MessageSerializator.Serialize(message);
            Byte[] data = Encoding.ASCII.GetBytes(str);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

        }

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        public event EventHandler<ClientConnectedEventArgs> OnClientConnected;

    }



    public class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }

    public class ClientConnectedEventArgs : EventArgs
    {
        public TcpClient Client { get; set; }
    }


}
