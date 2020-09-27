using System;
using System.Collections.Generic;
using System.Text;

namespace MyDataFlowPattern
{
    public static class DataFlowJsonSerializer
    {

        private const byte DataSeparator = (byte) ':';
        
        private static readonly Dictionary<string, Type> JsonTypes = new Dictionary<string, Type>();

        private static void RegisterTypeIfNotExists(string typeName, Type type)
        {
            lock (JsonTypes)
                JsonTypes.TryAdd(typeName, type);
        }

        private static bool TryGetJsonType(string typeName, out Type type)
        {
            lock (JsonTypes)
                return JsonTypes.TryGetValue(typeName, out type);
        }
        private static (string typeName, string data) GetTypeNameAndData(this byte[] data)
        {
            var i = 0;
            foreach (var b in data)
            {
                if (b == DataSeparator)
                {
                    var typeName = data.AsSpan(0, i);
                    var payLoad = data.AsSpan(i+1, data.Length - i-1);
                    return (Encoding.UTF8.GetString(typeName), Encoding.UTF8.GetString(payLoad));
                }

                i++;
            }
            
            throw new Exception("Unknown packageType");
        }


        public static void RegisterJsonSerializer(this MyDataFlowBase serializer)
        {
            serializer.RegisterSerializerDeserializer(
                o =>
                {
                    var list = new List<byte>();
                    var type = o.GetType();
                    var typeName = type.ToString();
                    RegisterTypeIfNotExists(typeName, type);
                    
                    list.AddRange(Encoding.ASCII.GetBytes(typeName));
                    list.Add(DataSeparator);
                    list.AddRange(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(o)));
                    return list.ToArray();
                },
                bytes =>
                {
                    var (typeName, data) = bytes.GetTypeNameAndData();

                    if (TryGetJsonType(typeName, out var type))
                        return Newtonsoft.Json.JsonConvert.DeserializeObject(data, type);
                    
                    throw new Exception("Invalid DataFlow type: "+type);
                });
        }
    }
}