using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketConnection
{
    public Action<string> OnMessageReceived;
    private PacketProtocol _packetProtocol = new PacketProtocol(1024);
    private Socket _socketConnection;

    public SocketConnection()
    {
        _packetProtocol.MessageArrived += _onMessageReceived;
    }

    // must be invoked from outside
    public void InitiateConnection(string hostAddress, int port)
    {

        var ipHostInfo = Dns.GetHostEntry(hostAddress);
        var iPEndPoint = new IPEndPoint(ipHostInfo.AddressList[0], port);

        var t  = new Thread(new ParameterizedThreadStart(_listenConnection));
        t.Start(iPEndPoint);

    }

    public void BreakConnection()
    {
        if (_socketConnection == null) { return; }
        _socketConnection.Shutdown(SocketShutdown.Both);
        _socketConnection.Close();
        _socketConnection = null;
    }

    public void Send(string msg)
    {

        if (_socketConnection == null) { return; }
        if (!_socketConnection.Connected) { return; }

        var bytes = Encoding.UTF8.GetBytes(msg);

        _socketConnection.Send(PacketProtocol.WrapMessage(bytes));

    }

    private void _onMessageReceived(byte[] data)
    {
        var str = Encoding.UTF8.GetString(data, 0, data.Length);
        OnMessageReceived?.Invoke(str);
    }

    private void _listenConnection(System.Object obj)
    {
        try
        {

            var iPEndPoint = (IPEndPoint)obj;

            _socketConnection = new Socket(iPEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socketConnection.Connect(iPEndPoint);

            Debug.Log("Socket connected to " + _socketConnection.RemoteEndPoint.ToString());

            int bytesRec;
            byte[] buffer = new byte[1024];

            try
            {
                while (_socketConnection != null && (bytesRec = _socketConnection.Receive(buffer)) > 0)
                {

                    byte[] data = new byte[bytesRec];

                    Array.Copy(buffer, 0, data, 0, bytesRec);
                    _packetProtocol.DataReceived(data);

                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        catch (Exception e)
        {
            Debug.LogError("Unexpected exception: " + e.ToString());
        }

    }

}
