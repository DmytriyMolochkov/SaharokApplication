using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ObjectsProjectServer
{
    [Serializable]
    public class InfoOfProcess : ISerializable
    {
        private static InfoOfProcess instance;
        private static object syncRoot = new Object();

        private InfoOfProcess()
        { }

        public static InfoOfProcess GetInstance()
        {
            if (instance == null)
                instance = new InfoOfProcess();
            return instance;
        }

        private int totalFormsFiles;
        public int TotalFormsFiles
        {
            get => totalFormsFiles;
            set => totalFormsFiles = value;
        }
        private int totalFormsSections;
        public int TotalFormsSections
        {
            get => totalFormsSections;
            set => totalFormsSections = value;
        }

        private int completeFormsFiles;
        public int CompleteFormsFiles
        {
            get => completeFormsFiles;
            set => completeFormsFiles = value;
        }

        private int completeFormsSections;
        public int CompleteFormsSections
        {
            get => completeFormsSections;
            set => completeFormsSections = value;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected InfoOfProcess(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.GetValue(this, info);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.AddValue(this, info);
        }
    }
}
