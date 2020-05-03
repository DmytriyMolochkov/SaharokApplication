using System;
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

        private List<FileToProject> AllFilesToProject { get; set; } = new List<FileToProject>();
        private List<SectionToProject> AllSectionsToProject { get; set; } = new List<SectionToProject>();

        public void CheckFilesToPDFSortToErrors()
        {
            List<Task> tasks = new List<Task>();

            var ErrorFiles = AllFilesToProject.Where(file => file.MethodPDFFile == MethodPDFFile.NoPDFMethod).Select(file => file.Path).ToList();

            if (ErrorFiles.Count() > 0)
                tasks.Add(Task.Run(() =>
                {
                    if (ErrorFiles.Count() == 1)
                    {
                        throw new Exception($"Недопустимое расширение у файла:{Environment.NewLine}      " +
                                                   String.Join(Environment.NewLine + "      ", ErrorFiles) + Environment.NewLine);
                    }
                    else
                    {
                        throw new Exception($"Недопустимое расширение у файлов:{Environment.NewLine}      " +
                                                   String.Join(Environment.NewLine + "      ", ErrorFiles) + Environment.NewLine);
                    }
                }));


            var ErrorFiles2 = AllFilesToProject
                .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                .Where(file => file.MethodPDFFile != MethodPDFFile.AutoCad)
                .GroupBy(file => file.OutputFileName).Where(group => group.ToList().Count > 1)
                .SelectMany(group => group)
                .Select(file => file.Path).ToList();
            if (ErrorFiles2.Count() > 0)
                tasks.Add(Task.Run(() =>
                       throw new Exception("Имя PDF файлов будет одинаковым у следующих файлов: "
                           + Environment.NewLine + "      "
                           + String.Join(Environment.NewLine + "      ", ErrorFiles2) + Environment.NewLine)));

            Task.WaitAll(tasks.ToArray());
        }

        public List<FileToProject> GetAllFilesToProject()
        {
            return AllFilesToProject;
        }

        public List<SectionToProject> GetAllSectionsToProject()
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
