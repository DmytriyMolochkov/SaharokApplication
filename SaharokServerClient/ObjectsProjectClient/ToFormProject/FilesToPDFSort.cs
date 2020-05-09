using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsProjectClient
{
    [Serializable]
    public class FilesToPDFSort : ISerializable
    {
        public List<FileToProject> FilesToProjectfromPDF = new List<FileToProject>();
        public List<FileToProject> FilesToProjectfromWord = new List<FileToProject>();
        public List<FileToProject> FilesToProjectfromExcel = new List<FileToProject>();
        public List<FileToProject> FilesToProjectfromKompas = new List<FileToProject>();
        public List<FileToProject> FilesToProjectfromAutoCad = new List<FileToProject>();

        private List<FileToProject> FilesWithExtensionError = new List<FileToProject>();
        private List<FileToProject> FilesWithNameError = new List<FileToProject>();

        private List<SectionToProject> AllSectionsToProject = new List<SectionToProject>();

        public void CheckFilesToPDFSortToErrors()
        {
            List<string> errorMessages = new List<string>();

            if (FilesWithExtensionError.Count() == 1)
            {
                errorMessages.Add($"Недопустимое расширение у файла:{Environment.NewLine}      " +
                    String.Join(Environment.NewLine + "      ", FilesWithExtensionError.Select(e => e.Path)));
            }
            else if(FilesWithExtensionError.Count() > 1)
            {
                errorMessages.Add($"Недопустимое расширение у файлов:{Environment.NewLine}      " +
                    String.Join(Environment.NewLine + "      ", FilesWithExtensionError.Select(e => e.Path)));
            }


            if (FilesWithNameError.Count() > 0)
                errorMessages.Add($"Имя PDF файлов будет одинаковым у следующих файлов:{Environment.NewLine}      " +
                    String.Join(Environment.NewLine + "      ", FilesWithNameError.Select(e => e.Path)) );

            if (errorMessages.Count > 0)
                throw new Exception(String.Join(Environment.NewLine + Environment.NewLine, errorMessages));
        }


        public IEnumerable<SectionToProject> GetAllSectionsToProject()
        {
            return AllSectionsToProject;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected FilesToPDFSort(SerializationInfo info, StreamingContext context)
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
