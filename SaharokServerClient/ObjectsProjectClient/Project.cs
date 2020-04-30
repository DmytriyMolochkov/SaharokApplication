using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Reflection;

namespace ObjectsProjectClient
{
    [Serializable]
    public class Project : IDisposable, INotifyPropertyChanged, ISerializable
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string PathEditableFiles { get; set; }
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string codeProject;
        public string CodeProject
        {
            get => codeProject;
            set
            {
                codeProject = value;
                OnPropertyChanged(nameof(CodeProject));
            }
        }
        public ObservableCollection<TypeDocumentation> TypeDocumentations { get; set; }
        public FileSystemWatcher watcher;
        public Action<Action> Invoke;


        public Project(string path, string nameProject, string codeProject, Action<Action> action)
        {

            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Path is not directory");
            }
            Path = path;
            PathEditableFiles = System.IO.Path.Combine(Path, "Редактируемые файлы");
            Name = nameProject;
            CodeProject = codeProject;
            Invoke = action;
            TypeDocumentations = new ObservableCollection<TypeDocumentation>(Directory.EnumerateDirectories(PathEditableFiles, "*", SearchOption.TopDirectoryOnly)
                .Select(line => new TypeDocumentation(line, System.IO.Path.GetFileName(line), this)).ToList());
            watcher = new FileSystemWatcher();

            watcher.Path = PathEditableFiles;
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName; ;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            watcher.Created += new FileSystemEventHandler(OnCreate);
            watcher.Deleted += new FileSystemEventHandler(OnDelete);
            watcher.Renamed += new RenamedEventHandler(OnRename);


        }
        void OnDelete(object source, FileSystemEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.FullPath) == PathEditableFiles)
            {
                string name = System.IO.Path.GetFileName(e.Name);
                TypeDocumentation deleteElement = TypeDocumentations.Where(item => name == item.Name).FirstOrDefault();
                if (deleteElement != null)
                {
                    Invoke(() => TypeDocumentations.Remove(deleteElement));
                }
            }
            else
            {
                GetTypeDocumentation(e.FullPath)?.OnDelete(source, e);
            }
        }

        void OnCreate(object source, FileSystemEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.FullPath) == PathEditableFiles)
            {
                if (Directory.Exists(e.FullPath))
                {
                    string name = System.IO.Path.GetFileName(e.Name);
                    Invoke(() => TypeDocumentations.Add(new TypeDocumentation(e.FullPath, name, this)));
                }
            }
            else
            {
                GetTypeDocumentation(e.FullPath)?.OnCreate(source, e);
            }
        }

        void OnRename(object source, RenamedEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.OldFullPath) == PathEditableFiles)
            {
                if (Directory.Exists(e.FullPath))
                {
                    string oldName = System.IO.Path.GetFileName(e.OldName);
                    string newName = System.IO.Path.GetFileName(e.Name);
                    string newPath = e.FullPath;
                    TypeDocumentation renameElement = TypeDocumentations.Where(item => oldName == item.Name).FirstOrDefault();
                    string oldPath = renameElement.Path;
                    Invoke(() =>
                    {
                        renameElement.Name = newName;
                        renameElement.Path = newPath;
                        renameElement.Sections.ForEachImmediate(line => line.RenamePath(line, oldPath, newPath));
                    });
                }
            }
            else
            {
                GetTypeDocumentation(e.FullPath)?.OnRename(source, e);
            }
        }
        public void Dispose()
        {
            watcher?.Dispose();
        }

        ~Project()
        {
            Dispose();
        }

        protected TypeDocumentation GetTypeDocumentation(string path)
        {
            return TypeDocumentations.Where(item => path.StartsWith(item.Path)).FirstOrDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
    }
}
