using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Numerics;

namespace AntifreezeServer.AntiGame
{

    public class GameMessageSerializator
    {
        public static string Serialize(GameUpdateMessage msg)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(msg.GetType());
            MemoryStream ms = new MemoryStream();

            serializer.WriteObject(ms, msg);
            string json = Encoding.Default.GetString(ms.ToArray());
            ms.Dispose();

            return json;
        }

        public static GameUpdateMessage Deserialize(string json)
        {
            GameUpdateMessage msg = Activator.CreateInstance<GameUpdateMessage>();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(msg.GetType());

            msg = (GameUpdateMessage)serializer.ReadObject(ms);
            ms.Close();
            ms.Dispose();

            return msg;
        }
    }


    [DataContract]
    public class GameUpdateMessage
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public GameStateDTO GameState;
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<GameUnitStatusDTO> UnitsStatuses;
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<GameUnitDestinationOrderDTO> UnitsDestinationOrders;
    }

    [DataContract]
    public class GameUnitStatusDTO
    {
        [DataMember(IsRequired = true)] 
        public int Uid;
        [DataMember(IsRequired = true)] 
        public float X;
        [DataMember(IsRequired = true)] 
        public float Y;
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool IsMoving = false;
    }

    [DataContract]
    public class GameUnitDestinationOrderDTO
    {
        [DataMember(IsRequired = true)] 
        public int UnitUid;
        [DataMember(IsRequired = true)] 
        public int CellUid;
    }

    [DataContract]
    public class GameStateDTO
    {
        [DataMember(IsRequired = true)] 
        public int GridSize;
        [DataMember(IsRequired = true)] 
        public int UnitsCount;
    }


}
