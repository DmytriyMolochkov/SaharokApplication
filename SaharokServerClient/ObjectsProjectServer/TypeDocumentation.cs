using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.Serialization;

namespace ObjectsProjectServer
{
    [Serializable]
    public class TypeDocumentation : ISerializable
    {
        private string name;
        public string Name
        {
            get => name;
            set => name = value;
        }

        public string Path { get; set; }
        public Project Project { get; set; }
        public ObservableCollection<Section> Sections { get; set; }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected TypeDocumentation(SerializationInfo info, StreamingContext context)
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
