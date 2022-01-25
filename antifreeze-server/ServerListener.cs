using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace antifreeze_server
{

    class ServerListener
    {

        private TcpListener server = null;
        private List<TcpClient> tcpClientsList = new List<TcpClient>();


        public void Start(int port)
        {

            if (server != null) return;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress localAddr = ipHostInfo.AddressList[0];
            server = new TcpListener(localAddr, port);
            server.Start();

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

                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Client connected!");

                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(client);

                    tcpClientsList.Add(client);

                    // emit connection
                    ClientConnectedEventArgs e = new ClientConnectedEventArgs();
                    e.Client = client;
                    if (OnClientConnected != null) OnClientConnected(this, e);

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                server.Stop();
            }
        }

        private void HandleClient(Object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();

            string imei = String.Empty;

            string data = null;
            Byte[] bytes = new Byte[256];
            int i;
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, i);

                    // emit message
                    MessageReceivedEventArgs e = new MessageReceivedEventArgs();
                    e.MessageText = data;
                    if (OnMessageReceived != null) OnMessageReceived(this, e);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("HandleDeivce Exception: {0}", e.ToString());
                client.Close();
            }

            client.Close();
            tcpClientsList.Remove(client);

            Console.WriteLine("Client disconnected!");

        }

        public void Broadcast(string message)
        {

            foreach (TcpClient client in tcpClientsList) Send(client, message);

        }

        public void Send(TcpClient client, string message)
        {

            Byte[] data = Encoding.ASCII.GetBytes(message);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

        }

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        public event EventHandler<ClientConnectedEventArgs> OnClientConnected;

    }



    public class MessageReceivedEventArgs : EventArgs
    {
        public string MessageText { get; set; }
    }

    public class ClientConnectedEventArgs : EventArgs
    {
        public TcpClient Client { get; set; }
    }


}
