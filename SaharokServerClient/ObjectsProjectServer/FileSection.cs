using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectsProjectServer
{
    public class FileSection
    {
        private string name;
        public string Name { get; set; }
        public string Path { get; set; }
        public Section Section { get; set; }
        public MethodPDFFile MethodPDFFile { get; set; }
        //public FileSection(string path, string name, Section parent)
        //{
        //    Path = path;
        //    Name = name;
        //    Section = parent;
        //    MethodPDFFile = TypeFile.ChooseMethodPDFFile(this);
        //}
    }
}
