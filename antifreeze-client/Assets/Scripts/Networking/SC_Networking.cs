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
    private INetwork _networkConnection = new SocketConnection();


    public void SendMessageToServer(string msg)
    {
        _networkConnection.Send(msg);
    }

    // Start is called before the first frame update
    void Start()
    {
        _networkConnection.Start(HostAddress, Port);
    }



    // Update is called once per frame
    void Update()
    {
        var messages = _networkConnection.CollectMessages();

        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            OnMessage?.Invoke(message);
        }
    }
}
