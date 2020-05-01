using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ObjectsProjectServer
{
    public class InfoOfProcess : ISerializable
    {
        private static InfoOfProcess instance;

        private InfoOfProcess()
        { }

        public static InfoOfProcess getInstance()
        {
            if (instance == null)
                instance = new InfoOfProcess();
            return instance;
        }

        private static int totalFormsFiles;
        public static int TotalFormsFiles
        {
            get => totalFormsFiles;
            set => totalFormsFiles = value;
        }
        private static int totalFormsSections;
        public static int TotalFormsSections
        {
            get => totalFormsSections;
            set => totalFormsSections = value;
        }

        private static int completeFormsFiles;
        public static int CompleteFormsFiles
        {
            get => completeFormsFiles;
            set => completeFormsFiles = value;
        }

        private static int completeFormsSections;
        public static int CompleteFormsSections
        {
            get => completeFormsSections;
            set => completeFormsSections = value;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected InfoOfProcess(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.GetValue(this, info);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.AddValue(this, info);
        }
    }
}
