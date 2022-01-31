using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketConnection : INetwork
{
    private List<string> _receivedMessages = new List<string>();
    private List<string> _messagesToSend = new List<string>();
    private MessageProtocol _messageProtocol = new MessageProtocol();
    private Socket _socketConnection;

    public SocketConnection()
    {
        _messageProtocol.MessageReceived += _onMessageReceived;
    }

    private void _onMessageReceived(byte[] data)
    {
        var message = Encoding.UTF8.GetString(data, 0, data.Length);
        lock (_receivedMessages) { _receivedMessages.Add(message); }
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
            byte[] buffer = new byte[1024 * 4];

            try
            {
                while (_socketConnection != null && (bytesRec = _socketConnection.Receive(buffer)) > 0)
                {

                    byte[] data = new byte[bytesRec];


                    Array.Copy(buffer, 0, data, 0, bytesRec);
                    _messageProtocol.DataReceived(data);

                }
            }
            catch (SocketException se)
            {
                Debug.LogError(se.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }
        catch (Exception e)
        {
            Debug.LogError("Unexpected exception: " + e.ToString());
        }

    }

    public List<string> CollectMessages()
    {
        var returnList = new List<string>();

        lock (_receivedMessages) { 

            while (_receivedMessages.Count > 0)
            {
                var message = _receivedMessages[0];
                _receivedMessages.RemoveAt(0);
                returnList.Add(message);
            }

            _receivedMessages.Clear();

        }

        return returnList;
    }

    public void Start(string hostAddress, int port)
    {
        var ipHostInfo = Dns.GetHostEntry(hostAddress);
        var iPEndPoint = new IPEndPoint(ipHostInfo.AddressList[0], port);

        var t = new Thread(new ParameterizedThreadStart(_listenConnection));
        t.Start(iPEndPoint);
    }

    public void Send(string msg)
    {

        if (_socketConnection == null) { return; }
        if (!_socketConnection.Connected) { return; }

       /* lock (_messagesToSend) { 
            _messagesToSend.Add(msg); 
        } */

        var bytes = Encoding.UTF8.GetBytes(msg);

        _socketConnection.Send(MessageProtocol.WrapData(bytes));

    }

    public void Close()
    {
        if (_socketConnection == null) { return; }
        _socketConnection.Shutdown(SocketShutdown.Both);
        _socketConnection.Close();
        _socketConnection = null;
    }
}
