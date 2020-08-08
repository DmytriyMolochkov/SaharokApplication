using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Reflection;

namespace Saharok
{
    [Serializable]
    public class FileSection : INotifyPropertyChanged, ISerializable
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


        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected FileSection(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.GetValue(this, info);
        }


        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.AddValue(this, info);
        }
    }
}
