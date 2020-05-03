using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsProjectServer
{
    [Serializable]
    public class Section : ISerializable, IFilesToProjectContainer
    {
        private string name;
        public string Name
        {
            get => name;
            set => name = value;
        }
        public string Path { get; set; }
        public TypeDocumentation TypeDocumentation { get; set; }
        public ObservableCollection<FileSection> Files { get; set; }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Section(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.GetValue(this, info);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.AddValue(this, info);
        }

        public FilesToPDFSort GetFilesToPDFSort()
        {
            SectionToProject sectionToProject = SectionToProject.GetSection(this);
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionToProject);
            return filesToPDFSort;
        }
    }
}
