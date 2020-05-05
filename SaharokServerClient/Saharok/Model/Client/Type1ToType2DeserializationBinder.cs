using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ObjectsProjectClient;

namespace Saharok
{
    sealed class Type1ToType2DeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;
            typeName = typeName.Replace("Server", "Client");
            assemblyName = assemblyName.Replace("Server", "Client");
            typeToDeserialize = Type.GetType(String.Format($"{typeName}, {assemblyName}"));
            return typeToDeserialize;
        }
    }
}
