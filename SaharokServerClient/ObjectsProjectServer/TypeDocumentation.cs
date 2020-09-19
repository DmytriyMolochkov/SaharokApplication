using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.Serialization;

namespace ObjectsProjectServer
{
    [Serializable]
    public class TypeDocumentation : ISerializable, IFilesToProjectContainer
    {
        private string name;
        public string Name
        {
            get => name;
            set => name = value;
        }

        public string Path { get; set; }
        public Project Project { get; set; }
        public ObservableCollection<Section> Sections { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected TypeDocumentation(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.GetValue(this, info);
        }


        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.AddValue(this, info);
        }
        public FilesToPDFSort GetFilesToPDFSort()
        {
            List<SectionToProject> sectionsToProject = SectionToProject.GetSections(this);
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            return filesToPDFSort;
        }

        public string GetNameProject()
        {
            return Project.GetNameProject();
        }

        public string GetCodeProject()
        {
            return Project.GetCodeProject();
        }

        public string GetPathProject()
        {
            return Project.GetPathProject();
        }
    }
}
