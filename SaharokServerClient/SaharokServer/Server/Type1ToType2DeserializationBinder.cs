﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ObjectsProjectServer;

namespace SaharokServer
{
    sealed class Type1ToType2DeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;
            typeName = typeName.Replace("Saharok", "ObjectsProjectServer");
            assemblyName = assemblyName.Replace("Saharok", "ObjectsProjectServer");
            typeToDeserialize = Type.GetType(String.Format($"{typeName}, {assemblyName}"));
            return typeToDeserialize;
        }
    }
}
