using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsProjectClient
{
    [Serializable]
    public class FileToProject : ISerializable
    {
        public string Path;
        public string Name;
        public string OutputFileName;
        public bool IsDone;
        public MethodPDFFile MethodPDFFile;
        public SectionToProject SectionToProject;

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected FileToProject(SerializationInfo info, StreamingContext context)
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
