using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Numerics;

namespace AntifreezeServer
{

    public class MessageSerializator
    {
        public static string Serialize(Message msg)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(msg.GetType());
            MemoryStream ms = new MemoryStream();

            serializer.WriteObject(ms, msg);
            string json = Encoding.Default.GetString(ms.ToArray());

            ms.Dispose();

            return json;

        }

        public static Message Deserialize(string json)
        {

            Message msg = Activator.CreateInstance<Message>();

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(msg.GetType());

            msg = (Message)serializer.ReadObject(ms);

            ms.Close();
            ms.Dispose();

            return msg;

        }
    }

    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }


    [DataContract]
    public class Message
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public GameStateDTO GameState;
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<UnitStatusDTO> UnitsPositions;
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<UnitDestinationOrderDTO> UnitsDestinationOrders;
    }

    [DataContract]
    public class UnitStatusDTO
    {
        [DataMember]
        public int Uid { get; set; }
        [DataMember]
        public Vector2 Coords { get; set; }
        [DataMember]
        public bool IsMoving { get; set; }
    }

    [DataContract]
    public class UnitDestinationOrderDTO
    {
        [DataMember]
        public int UnitUid { get; set; }
        [DataMember]
        public int CellUid { get; set; }
    }

    [DataContract]
    public class GameStateDTO
    {
        [DataMember]
        public int GridSize { get; set; }
        [DataMember]
        public int UnitsCount { get; set; }
    }


}
