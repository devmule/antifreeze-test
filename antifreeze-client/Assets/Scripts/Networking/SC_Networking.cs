using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class SC_Networking : MonoBehaviour
{


    [System.Serializable]
    public class OnMessageEvent : UnityEvent<string>{ }

    [SerializeField] public string HostAddress = "localhost";
    [SerializeField] public int Port = 8080;
    [SerializeField] public OnMessageEvent OnMessage;
    private SocketConnection _networkConnection;
    private List<string> _messageList = new List<string>();

    public void SendMessageToServer(string msg)
    {
        if (_networkConnection == null) { return; }
        _networkConnection.Send(msg);
    }

    // Start is called before the first frame update
    void Start()
    {
        _networkConnection = new SocketConnection();
        _networkConnection.InitiateConnection(HostAddress, Port);
        // UnityEngine.UnityException: get_isPlaying can only be called from the main thread.
        // so, we adding messages to the external list
        _networkConnection.OnMessageReceived += (string msg) => { lock (_messageList) { _messageList.Add(msg); } };
    }

    // Update is called once per frame
    void Update()
    {
        lock (_messageList)
        {
            while (_messageList.Count > 0)
            {
                string msg = _messageList[0];
                _messageList.RemoveAt(0);
                OnMessage?.Invoke(msg);
            }
        }
    }
}
