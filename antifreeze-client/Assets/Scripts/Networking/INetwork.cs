using System;
using System.Collections.Generic;

public interface INetwork
{

    public List<string> CollectReceivedMessages();

    public void Start(string host, int port);

    public void Send(string message);

    public void Close();

}
