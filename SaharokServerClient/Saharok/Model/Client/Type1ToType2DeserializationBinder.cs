using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Saharok
{
    sealed class Type1ToType2DeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;
            typeName = typeName.Replace("ObjectsProjectServer", "Saharok");
            assemblyName = assemblyName.Replace("ObjectsProjectServer", "Saharok");
            typeToDeserialize = Type.GetType(String.Format($"{typeName}, {assemblyName}"));
            return typeToDeserialize;
        }
    }
}
