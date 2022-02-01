using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketConnection : INetwork
{

    private ManualResetEvent _sendingMessageAddedEvent = new ManualResetEvent(false);
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

    private void _startReceivingLoop()
    {
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
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void _startTansmittingLoop()
    {
        try
        {
            while (_socketConnection != null)
            {
                List<string> cloneList;
                lock (_messagesToSend) {
                    cloneList = _messagesToSend.ToList();
                    _messagesToSend.Clear();
                }

                _sendingMessageAddedEvent.Reset();

                for (int i = 0; i < cloneList.Count; i++)
                {
                    var message = cloneList[i];
                    var bytes = Encoding.UTF8.GetBytes(message);
                    _socketConnection.Send(MessageProtocol.WrapData(bytes));
                }

                _sendingMessageAddedEvent.WaitOne();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public List<string> CollectReceivedMessages()
    {
        var returnList = new List<string>();

        lock (_receivedMessages) {
            for (int i = 0; i < _receivedMessages.Count; i++)
            {
                var message = _receivedMessages[i];
                returnList.Add(message);
            }
            _receivedMessages.Clear();
        }

        return returnList;
    }

    public void Start(string hostAddress, int port)
    {

        if (_socketConnection != null) return;

        try
        {

            var ipHostInfo = Dns.GetHostEntry(hostAddress);
            var iPEndPoint = new IPEndPoint(ipHostInfo.AddressList[0], port);

            _socketConnection = new Socket(iPEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // connect async with callback
            _socketConnection.BeginConnect(iPEndPoint, new AsyncCallback((IAsyncResult ar) => {

                _socketConnection.EndConnect(ar);
                Debug.Log("Socket connected to " + _socketConnection.RemoteEndPoint.ToString());

                // when connected, begin transmitting and receiving threads

                var receivingThread = new Thread(new ThreadStart(_startReceivingLoop));
                receivingThread.Start();

                var transmittingThread = new Thread(new ThreadStart(_startTansmittingLoop));
                transmittingThread.Start();

            }), null);

        }
        catch (Exception e)
        {
            Debug.LogError("Unexpected exception: " + e.ToString());
        }

    }

    public void Send(string msg)
    {

        if (_socketConnection == null) { return; }
        if (!_socketConnection.Connected) { return; }

       lock (_messagesToSend) { 
            _messagesToSend.Add(msg); 
            _sendingMessageAddedEvent.Set();
       }


    }

    public void Close()
    {
        if (_socketConnection == null) { return; }
        _socketConnection.Shutdown(SocketShutdown.Both);
        _socketConnection.Close();
        _socketConnection = null;
    }
}
