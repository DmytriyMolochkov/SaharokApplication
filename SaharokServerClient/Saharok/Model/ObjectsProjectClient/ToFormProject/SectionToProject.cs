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
    public class SectionToProject : ISerializable
    {
        public string Path;
        public Dictionary<string, MethodFormFile> OutputSectionPaths;
        public List<FileToProject> FilesToProject;
        public bool IsDone;

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected SectionToProject(SerializationInfo info, StreamingContext context)
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
