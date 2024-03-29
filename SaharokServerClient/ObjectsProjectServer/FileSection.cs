﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace ObjectsProjectServer
{
    [Serializable]
    public class FileSection : ISerializable, IFilesToProjectContainer
    {
        private string name;
        public string Name
        {
            get => name;
            set{ name = value; }
        }
        public string Path { get; set; }
        public Section Section { get; set; }
        public MethodPDFFile MethodPDFFile { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected FileSection(SerializationInfo info, StreamingContext context)
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
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(FileToProject.GetFilesToProjectToPages(this));
            return filesToPDFSort;
        }

        public string GetNameProject()
        {
            return Section.TypeDocumentation.Project.GetNameProject();
        }

        public string GetCodeProject()
        {
            return Section.GetCodeProject();
        }

        public string GetPathProject()
        {
            return Section.TypeDocumentation.Project.GetPathProject();
        }
    }
}
