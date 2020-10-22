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
using System.Configuration;
using static Saharok.CustomMethods;

namespace Saharok
{
    [Serializable]
    public class Project : IDisposable, INotifyPropertyChanged, ISerializable
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public FoldersConfigInfo FoldersConfigInfo { get; set; }
        public SectionNameTemplate SectionNameTemplate { get; set; }
        public bool IsVirtualProject { get; set; }
        public List<string> watcherSectionsFilter; //ForVirtualProject

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
        public List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        public Action<Action> Invoke;


        public Project(string path, string nameProject, string codeProject, Action<Action> action)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Несуществующий путь проекта");
            }
            IsVirtualProject = false;
            Path = path;
            FoldersConfigInfo = new FoldersConfigInfo((ProjectFoldersConfigSection)ConfigurationManager.GetSection("ProjectFolders"));
            SectionNameTemplate = new SectionNameTemplate((SectionNameTemplateConfigSection)ConfigurationManager.GetSection("SectionNameTemplate"));
            
            Name = nameProject;
            CodeProject = codeProject;
            Invoke = action;

            string pathWorkingDirectory = System.IO.Path.Combine(Path, FoldersConfigInfo.WorkingDirectory);
            TypeDocumentations = new ObservableCollection<TypeDocumentation>(Directory.EnumerateDirectories(pathWorkingDirectory, "*", SearchOption.TopDirectoryOnly)
                .Select(line => new TypeDocumentation(line, System.IO.Path.GetFileName(line), this)).ToList());
            watchers.Add(new FileSystemWatcher());

            watchers[0].Path = pathWorkingDirectory;
            watchers[0].NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName; ;
            watchers[0].EnableRaisingEvents = true;
            watchers[0].IncludeSubdirectories = true;

            watchers[0].Created += new FileSystemEventHandler(OnCreate);
            watchers[0].Deleted += new FileSystemEventHandler(OnDelete);
            watchers[0].Renamed += new RenamedEventHandler(OnRename);
        }

        public Project(IEnumerable<string> sectionsPaths, string formProjectPath, Action<Action> action)
        {
            var errorMesseges = new List<string>();
            var errorSections = new List<string>();
            var sameNameSections = new List<string>();

            sectionsPaths
                .Where(section => !Directory.Exists(section))
                .ToList()
                .ForEach(section =>
                {
                    errorSections.Add(section);
                });
            if (errorSections.Count > 0)
                errorMesseges.Add($"Несуществующие пути разделов документации:{Environment.NewLine}"
                    + String.Join($"{Environment.NewLine}", errorSections));
            
            sectionsPaths
                .Where(section => Directory.Exists(section))
                .ToList()
                .GroupBy(section => System.IO.Path.GetFileName(section))
                .Where(group => group.ToList().Count > 1)
                .SelectMany(group => group).ToList()
                .ForEach(section =>
                {
                    sameNameSections.Add(section);
                });
            if (sameNameSections.Count > 0)
                errorMesseges.Add($"Одинаковые имена PDF-альбомов будут у следующих разделов:{Environment.NewLine}"
                    + String.Join($"{Environment.NewLine}", sameNameSections));

            if(!Directory.Exists(formProjectPath))
                errorMesseges.Add($"Несуществующий путь для PDF-альбомов:{Environment.NewLine}{formProjectPath}");

            if (errorMesseges.Count > 0)
                throw new Exception(String.Join($"{Environment.NewLine}{Environment.NewLine}", errorMesseges));

            IsVirtualProject = true;
            List<string> typeDocumentationPaths = sectionsPaths.Select(s => System.IO.Path.GetDirectoryName(s)).Distinct().ToList();
            watcherSectionsFilter = sectionsPaths.ToList();

            FoldersConfigInfo = new FoldersConfigInfo();
            FoldersConfigInfo.OutputFilesPDFDirectories.Add(formProjectPath);
            FoldersConfigInfo.OutputPageByPagePDF = System.IO.Path.Combine(formProjectPath, "!temp_" + Guid.NewGuid().ToString().Substring(30));

            var sectionNameTemplateConfigSection = new SectionNameTemplateConfigSection();
            sectionNameTemplateConfigSection.Template = "|раздел документации|";
            sectionNameTemplateConfigSection.Separator = ' ';
            SectionNameTemplate = new SectionNameTemplate(sectionNameTemplateConfigSection);

            Invoke = action;
            TypeDocumentations = new ObservableCollection<TypeDocumentation>(
                typeDocumentationPaths.Select(d => new TypeDocumentation(d, System.IO.Path.GetFileName(d), this, sectionsPaths)));

            typeDocumentationPaths.ForEach(d =>
            {
                var watcher = new FileSystemWatcher();
                watchers.Add(new FileSystemWatcher());
                watcher.Path = System.IO.Path.GetDirectoryName(d);
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                         | NotifyFilters.LastWrite
                                         | NotifyFilters.FileName
                                         | NotifyFilters.DirectoryName;
                watcher.EnableRaisingEvents = true;
                watcher.IncludeSubdirectories = true;
                
                watcher.Created += new FileSystemEventHandler(OnCreate);
                watcher.Deleted += new FileSystemEventHandler(OnDelete);
                watcher.Renamed += new RenamedEventHandler(OnRename);
                watchers.Add(watcher);
            });
        }

        void OnDelete(object source, FileSystemEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.FullPath) == ((FileSystemWatcher)source).Path)
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
            if (System.IO.Path.GetDirectoryName(e.FullPath) == ((FileSystemWatcher)source).Path)
            {
                if (IsVirtualProject)
                    return;
                if (Directory.Exists(e.FullPath))
                {
                    string name = System.IO.Path.GetFileName(e.Name);
                    if (TypeDocumentations.Where(d => d.Name == name).Count() == 0)
                        Invoke(() =>
                        {
                            TypeDocumentations.Add(new TypeDocumentation(e.FullPath, name, this));
                            TypeDocumentations.Sort((a, b) => { return new LogicalStringComparer().Compare(a.Name, b.Name); });
                        });
                }
            }
            else
            {
                GetTypeDocumentation(e.FullPath)?.OnCreate(source, e);
            }
        }

        void OnRename(object source, RenamedEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.OldFullPath) == ((FileSystemWatcher)source).Path)
            {
                if (Directory.Exists(e.FullPath))
                {
                    string oldName = System.IO.Path.GetFileName(e.OldName);
                    string newName = System.IO.Path.GetFileName(e.Name);
                    string newPath = e.FullPath;
                    TypeDocumentation renameElement = TypeDocumentations.Where(item => oldName == item.Name).FirstOrDefault();
                    if (renameElement == null)
                        return;

                    string oldPath = renameElement.Path;
                    Invoke(() =>
                    {
                        renameElement.Name = newName;
                        renameElement.Path = newPath;
                        renameElement.Sections.ForEachImmediate(line => line.RenamePath(line, oldPath, newPath));
                        TypeDocumentations.Sort((a, b) => { return new LogicalStringComparer().Compare(a.Name, b.Name); });
                    });

                    if (IsVirtualProject)
                    {
                        watcherSectionsFilter.ForEach(s => s = s.Replace(oldPath, newPath));
                    }
                }
            }
            else
            {
                GetTypeDocumentation(e.FullPath)?.OnRename(source, e);
            }
        }
        public void Dispose()
        {
            watchers.ForEach(w => w?.Dispose());
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

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected Project(SerializationInfo info, StreamingContext context)
        {
            info = FieldsSerializble.GetValue(this, info, new string[] { "watchers", "watcherSectionsFilter" });
        }


        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info = FieldsSerializble.AddValue(this, info, new string[] { "watchers", "watcherSectionsFilter" });
        }
    }
}
