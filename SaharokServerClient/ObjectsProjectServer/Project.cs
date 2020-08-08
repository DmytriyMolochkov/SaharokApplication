using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace ObjectsProjectServer
{
    [Serializable]
    public class Project : ISerializable, IFilesToProjectContainer
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string PathEditableFiles { get; set; }
        private string name;
        public string Name
        {
            get => name;
            set => name = value;
        }
        private string codeProject;
        public string CodeProject
        {
            get => codeProject;
            set => codeProject = value;
        }
        public ObservableCollection<TypeDocumentation> TypeDocumentations { get; set; }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Project(SerializationInfo info, StreamingContext context)
        {
            info = FieldsSerializble.GetValue(this, info, new string[] { "watcher" });
        }


        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info = FieldsSerializble.AddValue(this, info, new string[] { "watcher" });
        }

        public FilesToPDFSort GetFilesToPDFSort()
        {
            List<SectionToProject> sectionsToProject = SectionToProject.GetSections(this);
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            return filesToPDFSort;
        }

        public string GetNameProject()
        {
            return Name;
        }

        public string GetCodeProject()
        {
            return CodeProject;
        }

        public string GetPathProject()
        {
            return Path;
        }
    }
}
