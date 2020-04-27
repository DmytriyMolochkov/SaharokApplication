//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;
//using System.ComponentModel;

//namespace Saharok.Model.ObjectsProject
//{
//    public class Section : INotifyPropertyChanged
//    {
//        private string name;
//        public string Name
//        {
//            get => name;
//            set
//            {
//                name = value;
//                OnPropertyChanged(nameof(Name));
//            }
//        }
//        public string Path { get; set; }
//        public TypeDocumentation TypeDocumentation { get; set; }
//        public ObservableCollection<FileSection> Files { get; set; }

//        public Section(string path, string name, TypeDocumentation parent)
//        {
//            Path = path;
//            Name = name;
//            TypeDocumentation = parent;
//            LoadFilesWithRetries();
//        }

//        public void OnDelete(object source, FileSystemEventArgs e)
//        {
//            string name = System.IO.Path.GetFileName(e.Name);
//            FileSection deleteElement = Files.Where(item => name == item.Name).FirstOrDefault();
//            if (System.IO.Path.GetDirectoryName(e.FullPath) == Path && deleteElement != null)
//            {
//                App.Current.Dispatcher.Invoke(() => Files.Remove(deleteElement));
//            }
//        }

//        public void OnCreate(object source, FileSystemEventArgs e)
//        {
//            if (System.IO.Path.GetDirectoryName(e.FullPath) == Path && File.Exists(e.FullPath) && FileFilter(e.FullPath))
//            {
//                string name = System.IO.Path.GetFileName(e.Name);
//                App.Current.Dispatcher.Invoke(() => Files.Add(new FileSection(e.FullPath, name, this)));
//            }
//        }

//        private bool IsNotCreateAfterRename(object source, RenamedEventArgs e)
//        {
//            if (Files.Any(file => file.Path == e.OldFullPath))
//            {
//                return true;
//            }
//            else
//            {
//                if (FileFilter(e.FullPath))
//                {
//                    string name = System.IO.Path.GetFileName(e.Name);
//                    App.Current.Dispatcher.Invoke(() => Files.Add(new FileSection(e.FullPath, name, this)));
//                }
//                return false;
//            }
//        }

//        private bool IsNotDeleteAfterRename(object source, RenamedEventArgs e)
//        {
//            if (FileFilter(e.FullPath))
//            {
//                return true;
//            }
//            else
//            {

//                string name = System.IO.Path.GetFileName(e.OldName);
//                FileSection deleteElement = Files.Where(item => name == item.Name).FirstOrDefault();
//                App.Current.Dispatcher.Invoke(() => Files.Remove(deleteElement));
//                return false;
//            }
//        }
//        public void OnRename(object source, RenamedEventArgs e)
//        {
//            if (System.IO.Path.GetDirectoryName(e.OldFullPath) == Path && File.Exists(e.FullPath) && IsNotCreateAfterRename(source, e) && IsNotDeleteAfterRename(source, e))
//            {
//                string oldName = System.IO.Path.GetFileName(e.OldName);
//                string newName = System.IO.Path.GetFileName(e.Name);
//                string newPath = e.FullPath;
//                FileSection renameElement = Files.Where(item => oldName == item.Name).FirstOrDefault();
//                App.Current.Dispatcher.Invoke(() => renameElement.Name = newName);
//                App.Current.Dispatcher.Invoke(() => renameElement.Path = newPath);
//            }
//        }

//        protected async void LoadFilesWithRetries()
//        {
//            int @try = 0;
//            do
//            {
//                try
//                {
//                    LoadFiles();
//                    return;
//                }
//                catch (System.IO.IOException e)
//                {

//                    if (@try++ < 10)
//                    {
//                        if (@try > 1)
//                            await Task.Delay(500);
//                    }
//                    else
//                    {
//                        throw e;
//                    }
//                }
//            } while (true);
//        }

//        protected void LoadFiles()
//        {
//            Files = new ObservableCollection<FileSection>(Directory.EnumerateFiles(Path, "*", SearchOption.TopDirectoryOnly)
//                .Where(line => FileFilter(line)).Select(line => new FileSection(line, System.IO.Path.GetFileName(line), this)));
//        }

//        public void RenamePath(Section element, string oldPath, string newPath)
//        {
//            element.Path = element.Path.Replace(oldPath, newPath);
//            element.Files.ForEachImmediate(line => line.RenamePath(line, oldPath, newPath));
//        }

//        private bool FileFilter(string fileName)
//        {
//            List<string> ignoredExtensions = new List<string> { ".bak", ".tmp", ".sс$", ".dwl", ".cd~" };
//            string extensionFile = System.IO.Path.GetExtension(fileName).ToLower();
//            bool result = ignoredExtensions.All(arg => arg != extensionFile) && File.GetAttributes(fileName) != FileAttributes.Hidden && !fileName.Contains("~$");
//            if (result)
//                return true;
//            else
//                return false;
//        }


//        public event PropertyChangedEventHandler PropertyChanged;
//        protected virtual void OnPropertyChanged(string propertyName = "")
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//    }
//}
