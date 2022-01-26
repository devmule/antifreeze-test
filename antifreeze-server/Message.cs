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


    [DataContract]
    public class Message
    {
        [DataMember(EmitDefaultValue = false)]
        public GameStateDTO? state;
        [DataMember(EmitDefaultValue = false)]
        public List<UnitPositioningDTO>? positions;
    }


    public class UnitPositioningDTO
    {
        public int id { get; set; }
        public Vector2 coords { get; set; }
        public bool IsMoving { get; set; }
    }


    public class GameStateDTO
    {
        public int grid { get; set; }
        public int units { get; set; }
    }


}
