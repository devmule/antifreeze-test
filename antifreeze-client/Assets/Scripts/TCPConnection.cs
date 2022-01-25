using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPConnection : MonoBehaviour
{

    public String IPAdress = "localhost";
    public Int32 Port = 8080;

    private TcpClient tcpClient;
    private Thread listeningThread;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // must be invoked from outside
    public void InitiateConnection()
    {

        if (tcpClient != null) return;

        try
        {
            // run listening connection in separate thread
            listeningThread = new Thread(new ThreadStart(ListenConnection));
            listeningThread.IsBackground = true;
            listeningThread.Start();

        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

    }

    public void BreakConnection()
    {

        if (tcpClient == null) return;
        tcpClient.Close();
        tcpClient = null;

    }

    async private void ListenConnection()
    {

        try
        {

            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(IPAdress, Port);

            if (tcpClient.Connected)
            {

                NetworkStream stream = tcpClient.GetStream();

                while (tcpClient.Connected)
                {
                    byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        // message received
                    }
                }

            }

        }
        catch (SocketException e)
        {
            Debug.LogError(e);
        }

    }


    public void SendMessage(byte[] message)
    {

        if (tcpClient == null) return;

        try
        {

            NetworkStream stream = tcpClient.GetStream();
            stream.Write(message, 0, message.Length);

        }
        catch (SocketException e)
        {
            Debug.LogError(e);
        }

    }

}
