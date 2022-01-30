using System;

/// <summary>
/// reading messages through flow of bytes
/// </summary>
public class MessageProtocol
{

    private int _totalMessageLen = 0;
    private int _messageBytesReceived = 0;
    private byte[] _dataBuffer;

    /// <summary>
    /// write length of message in front of it
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] WrapData(byte[] bytes)
    {
        byte[] lengthPrefix = BitConverter.GetBytes(bytes.Length);
        byte[] ret = new byte[lengthPrefix.Length + bytes.Length];
        lengthPrefix.CopyTo(ret, 0);
        bytes.CopyTo(ret, lengthPrefix.Length);
        return ret;
    }

    public Action<byte[]> MessageReceived;

    /// <summary>
    /// searching for messages
    /// </summary>
    /// <param name="data"></param>
    public void DataReceived(byte[] data)
    {

        int i = 0;
        while (i != data.Length)
        {


            if (_dataBuffer == null)
            {
                // reading data length

                var size = sizeof(int);
                var lengthBuffer = new byte[size];

                Array.Copy(data, i, lengthBuffer, 0, size);
                _totalMessageLen = BitConverter.ToInt32(lengthBuffer, 0);
                _messageBytesReceived = 0;

                i += size;

                if (_totalMessageLen < 0) { throw new Exception("message len can not be less than 0"); }

                _dataBuffer = new byte[_totalMessageLen];

            }


            // reading data itself

            int availableBytesLen = data.Length - i;
            int receivedMessageBytesLen = Math.Min(_totalMessageLen, availableBytesLen);

            Array.Copy(data, i, _dataBuffer, _messageBytesReceived, receivedMessageBytesLen);
            _messageBytesReceived += availableBytesLen;

            i += receivedMessageBytesLen;

            if (_messageBytesReceived == _totalMessageLen)
            {
                MessageReceived?.Invoke(_dataBuffer);
                _dataBuffer = null;
            }

        }
    }
}
