using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class SC_Networking : MonoBehaviour
{


    [System.Serializable]
    public class OnMessageEvent : UnityEvent<string>{ }


    [SerializeField] public string Host = "localhost";
    [SerializeField] public string Port = "8080";
    [SerializeField] public OnMessageEvent OnMessage;
    private INetwork _networkConnection = new SocketConnection();

    public void ConnectToServer()
    {
        _networkConnection.Start(Host, int.Parse(Port, System.Globalization.NumberStyles.Integer));
    }

    public void SendMessageToServer(string msg)
    {
        _networkConnection.Send(msg);
    }

    public void SetHost(string host) { Host = host; }
    public void SetPort(string port) { Port = port; }

    // Update is called once per frame
    void Update()
    {
        var messages = _networkConnection.CollectReceivedMessages();

        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            OnMessage?.Invoke(message);
        }
    }
}
