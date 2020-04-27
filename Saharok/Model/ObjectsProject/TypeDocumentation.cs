//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Collections.ObjectModel;
//using System.Windows.Input;
//using System.ComponentModel;

//namespace Saharok.Model.ObjectsProject
//{
//    public class TypeDocumentation : INotifyPropertyChanged
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
//        public Project Project { get; set; }
//        public ObservableCollection<Section> Sections { get; set; }

//        public TypeDocumentation(string path, string name, Project parent)
//        {
//            Path = path;
//            Name = name;
//            Project = parent;
//            LoadSectionsWithRetries();
//        }

//        public Section GetSection(string path)
//        {
//            return Sections.Where(s => path.StartsWith(s.Path)).FirstOrDefault();
//        }

//        public void OnDelete(object source, FileSystemEventArgs e)
//        {
//            if (System.IO.Path.GetDirectoryName(e.FullPath) == Path)
//            {
//                string name = System.IO.Path.GetFileName(e.Name);
//                Section deleteElement = Sections.Where(item => name == item.Name).FirstOrDefault();
//                if (deleteElement != null)
//                {
//                    App.Current.Dispatcher.Invoke(() => Sections.Remove(deleteElement));
//                }
//            }
//            else
//            {
//                GetSection(e.FullPath)?.OnDelete(source, e);
//            }
//        }

//        public void OnCreate(object source, FileSystemEventArgs e)
//        {
//            if (System.IO.Path.GetDirectoryName(e.FullPath) == Path)
//            {
//                if (Directory.Exists(e.FullPath))
//                {
//                    string name = System.IO.Path.GetFileName(e.Name);
//                    App.Current.Dispatcher.Invoke(() => Sections.Add(new Section(e.FullPath, name, this)));
//                }
//            }
//            else
//            {
//                GetSection(e.FullPath)?.OnCreate(source, e);
//            }
//        }

//        public void OnRename(object source, RenamedEventArgs e)
//        {
//            if (System.IO.Path.GetDirectoryName(e.OldFullPath) == Path)
//            {
//                if (Directory.Exists(e.FullPath))
//                {
//                    string oldName = System.IO.Path.GetFileName(e.OldName);
//                    string newName = System.IO.Path.GetFileName(e.Name);
//                    string newPath = e.FullPath;
//                    Section renameElement = Sections.Where(item => oldName == item.Name).FirstOrDefault();
//                    string oldPath = renameElement.Path;
//                    App.Current.Dispatcher.Invoke(() => renameElement.Name = newName);
//                    App.Current.Dispatcher.Invoke(() => renameElement.Path = newPath);
//                    App.Current.Dispatcher.Invoke(() => renameElement.Files.ForEachImmediate(line => line.RenamePath(line, oldPath, newPath)));
//                }
//            }
//            else
//            {
//                GetSection(e.FullPath)?.OnRename(source, e);
//            }
//        }

//        protected async void LoadSectionsWithRetries()
//        {
//            int @try = 0;
//            do
//            {
//                try
//                {
//                    LoadSections();
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

//        protected void LoadSections()
//        {
//            Sections = new ObservableCollection < Section > (Directory.EnumerateDirectories(Path, "*", SearchOption.TopDirectoryOnly)
//                .Select(line => new Section(line, System.IO.Path.GetFileName(line), this)));
//        }

        
//        public event PropertyChangedEventHandler PropertyChanged;
//        protected virtual void OnPropertyChanged(string propertyName = "")
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//    }
//}
