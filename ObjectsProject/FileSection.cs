using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;

namespace ObjectsProject
{
    public class FileSection : INotifyPropertyChanged
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
        public Section Section { get; set; }
        public MethodPDFFile MethodPDFFile { get; set; }
        public FileSection(string path, string name, Section parent)
        {
            Path = path;
            Name = name;
            Section = parent;
            MethodPDFFile = TypeFile.ChooseMethodPDFFile(this);
        }

        public void RenamePath(FileSection element, string oldPath, string newPath)
        {
            element.Path = element.Path.Replace(oldPath, newPath);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
