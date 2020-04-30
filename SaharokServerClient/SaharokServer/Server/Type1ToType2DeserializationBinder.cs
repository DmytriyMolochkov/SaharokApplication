using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ObjectsProjectServer;
using ObjectsToFormProjectServer;

namespace SaharokServer.Server
{
    sealed class Type1ToType2DeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;
            typeName = typeName.Replace("Client", "Server");
            assemblyName = assemblyName.Replace("Client", "Server");
            typeToDeserialize = Type.GetType(String.Format($"{typeName}, {assemblyName}"));
            //typeToDeserialize = Type.GetType(String.Format($"{abc}"));
            return typeToDeserialize;
        }
    }
}
