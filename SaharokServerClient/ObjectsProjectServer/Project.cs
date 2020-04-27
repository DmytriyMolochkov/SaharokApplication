using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ObjectsProjectServer
{
    public class Project
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string PathEditableFiles { get; set; }
        public string Name { get; set; }
        public string CodeProject { get; set; }
        public ObservableCollection<TypeDocumentation> TypeDocumentations { get; set; }



        //public Project(string path, string nameProject, string codeProject, Action<Action> action)
        //{

        //    if (!Directory.Exists(path))
        //    {
        //        throw new ArgumentException("Path is not directory");
        //    }
        //    Path = path;
        //    PathEditableFiles = System.IO.Path.Combine(Path, "Редактируемые файлы");
        //    Name = nameProject;
        //    CodeProject = codeProject;
        //    TypeDocumentations = new ObservableCollection<TypeDocumentation>(Directory.EnumerateDirectories(PathEditableFiles, "*", SearchOption.TopDirectoryOnly)
        //        .Select(line => new TypeDocumentation(line, System.IO.Path.GetFileName(line), this)).ToList());
        //}

        //protected TypeDocumentation GetTypeDocumentation(string path)
        //{
        //    return TypeDocumentations.Where(item => path.StartsWith(item.Path)).FirstOrDefault();
        //}
    }
}
