using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Saharok
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

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected FileToProject(SerializationInfo info, StreamingContext context)
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
