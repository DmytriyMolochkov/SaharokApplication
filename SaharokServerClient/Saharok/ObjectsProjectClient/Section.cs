using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class Section : INotifyPropertyChanged, ISerializable
    {
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
        public string Path { get; set; }
        public TypeDocumentation TypeDocumentation { get; set; }
        public ObservableCollection<FileSection> Files { get; set; }

        public Section(string path, string name, TypeDocumentation parent)
        {
            Path = path;
            Name = name;
            TypeDocumentation = parent;
            LoadFilesWithRetries();
        }

        public void OnDelete(object source, FileSystemEventArgs e)
        {
            string name = System.IO.Path.GetFileName(e.Name);
            FileSection deleteElement = Files.Where(item => name == item.Name).FirstOrDefault();
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Path && deleteElement != null)
            {
                TypeDocumentation.Project.Invoke(() => Files.Remove(deleteElement));
            }
        }

        public void OnCreate(object source, FileSystemEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.FullPath) == Path && File.Exists(e.FullPath) && FileFilter(e.FullPath))
            {
                string name = System.IO.Path.GetFileName(e.Name);
                if (Files.Where(f => f.Name == name).Count() == 0)
                    TypeDocumentation.Project.Invoke(() =>
                    {
                        Files.Add(new FileSection(e.FullPath, name, this));
                        Files.Sort((a, b) => { return new LogicalStringComparer().Compare(a.Name, b.Name); });
                    });
            }
        }

        private bool IsNotCreateAfterRename(object source, RenamedEventArgs e)
        {
            if (Files.Any(file => file.Path == e.OldFullPath))
            {
                return true;
            }
            else
            {
                if (FileFilter(e.FullPath))
                {
                    string name = System.IO.Path.GetFileName(e.Name);
                    TypeDocumentation.Project.Invoke(() => Files.Add(new FileSection(e.FullPath, name, this)));
                }
                return false;
            }
        }

        private bool IsNotDeleteAfterRename(object source, RenamedEventArgs e)
        {
            if (FileFilter(e.FullPath))
            {
                return true;
            }
            else
            {

                string name = System.IO.Path.GetFileName(e.OldName);
                FileSection deleteElement = Files.Where(item => name == item.Name).FirstOrDefault();
                TypeDocumentation.Project.Invoke(() => Files.Remove(deleteElement));
                return false;
            }
        }
        public void OnRename(object source, RenamedEventArgs e)
        {
            if (System.IO.Path.GetDirectoryName(e.OldFullPath) == Path
                && File.Exists(e.FullPath)
                && IsNotCreateAfterRename(source, e)
                && IsNotDeleteAfterRename(source, e))
            {
                string oldName = System.IO.Path.GetFileName(e.OldName);
                string newName = System.IO.Path.GetFileName(e.Name);
                string newPath = e.FullPath;
                FileSection renameElement = Files.Where(item => oldName == item.Name).FirstOrDefault();
                if (renameElement == null)
                    return;

                TypeDocumentation.Project.Invoke(() =>
                {
                    renameElement.Name = newName;
                    renameElement.Path = newPath;
                    renameElement.MethodPDFFile = TypeFile.ChooseMethodPDFFile(renameElement);
                    Files.Sort((a, b) => { return new LogicalStringComparer().Compare(a.Name, b.Name); });
                });

            }
        }

        public FileSection GetFile(string path)
        {
            return Files.Where(s => path.StartsWith(s.Path)).FirstOrDefault();
        }

        protected async void LoadFilesWithRetries()
        {
            int @try = 0;
            do
            {
                try
                {
                    LoadFiles();
                    return;
                }
                catch (System.IO.IOException e)
                {

                    if (@try++ < 10)
                    {
                        if (@try > 1)
                            await Task.Delay(500);
                    }
                    else
                    {
                        throw e;
                    }
                }
            } while (true);
        }

        protected void LoadFiles()
        {
            Files = new ObservableCollection<FileSection>(Directory.EnumerateFiles(Path, "*", SearchOption.TopDirectoryOnly)
                .Where(line => FileFilter(line)).Select(line => new FileSection(line, System.IO.Path.GetFileName(line), this)));
        }

        public void RenamePath(Section element, string oldPath, string newPath)
        {
            element.Path = element.Path.Replace(oldPath, newPath);
            element.Files.ForEachImmediate(line => line.RenamePath(line, oldPath, newPath));
        }


        [NonSerialized]
        static List<string> ignoredExtensions = new List<string>(ConfigurationManager.AppSettings["IgnoredExtensions"].Replace(" ", String.Empty).Split(',')
           /* ((IgnoredExtensionsConfigSection)ConfigurationManager.GetSection("IgnoredExtensions")).value.Replace(" ", String.Empty).Split(',')*/);

        private bool FileFilter(string fileName)
        {
            string extensionFile = System.IO.Path.GetExtension(fileName).ToLower();
            FileAttributes r = File.GetAttributes(fileName);
            return ignoredExtensions.All(arg => !extensionFile.StartsWith('.' + arg))
                && (File.GetAttributes(fileName) & FileAttributes.Hidden) != FileAttributes.Hidden && !fileName.Contains("~$");
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected Section(SerializationInfo info, StreamingContext context)
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
