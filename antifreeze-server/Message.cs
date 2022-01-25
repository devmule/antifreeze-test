using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;


namespace antifreeze_server
{

    public class MessageSerializator
    {
        public static string Serialize<T>(T obj)
        {

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, obj);
            string retVal = Encoding.Default.GetString(ms.ToArray());
            ms.Dispose();
            return retVal;

        }

        public static T Deserialize<T>(string json)
        {

            T obj = Activator.CreateInstance<T>();

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(ms);

            ms.Close();
            ms.Dispose();

            return obj;

        }
    }


    [DataContract]
    public class Message
    {
        [DataMember(EmitDefaultValue = false)]
        public MessageGameState? state;
        [DataMember(EmitDefaultValue = false)]
        public List<MessageUnitPositioning> positions = new List<MessageUnitPositioning>();
    }


    public class MessageUnitPositioning
    {
        public int id { get; set; }
        public double x { get; set; }
        public double y { get; set; }
    }


    public class MessageGameState
    {
        public int grid { get; set; }
        public int units { get; set; }
    }


}
