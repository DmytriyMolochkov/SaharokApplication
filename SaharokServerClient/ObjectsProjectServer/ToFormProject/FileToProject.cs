using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace ObjectsProjectServer
{
    [Serializable]
    public class FileToProject : ISerializable
    {
        public string Path;
        public string Name;
        public string OutputFileName;
        public bool IsDone;
        public MethodPDFFile MethodPDFFile;
        public SectionToProject SectionToProject;

        public FileToProject(string filePath, string name, string outputFileName, MethodPDFFile methodPDFFile)
        {
            Path = filePath;
            Name = name;
            OutputFileName = outputFileName;
            IsDone = false;
            MethodPDFFile = methodPDFFile;
        }

        public static List<FileToProject> GetFilesToProjectToPages(FileSection file)
        {
            
            Section section = file.Section;
            TypeDocumentation typeDocumentation = section.TypeDocumentation;
            Project project = typeDocumentation.Project;
            string nameDirFileToPDFToPages = project.FoldersConfigInfo.OutputPageByPagePDF;
            List<FileToProject> filesToPDF = new List<FileToProject>();
            if (!project.IsVirtualProject)
            {
                string outputFileName = System.IO.Path.ChangeExtension(
                    System.IO.Path.Combine(project.Path, nameDirFileToPDFToPages, typeDocumentation.Name, section.Name, file.Name), "pdf");
                filesToPDF.Add(new FileToProject(file.Path, file.Name, outputFileName, file.MethodPDFFile));
            }
            else
            {
                string outputFileName = System.IO.Path.ChangeExtension(
                   System.IO.Path.Combine(project.FoldersConfigInfo.OutputFilesPDFDirectories.FirstOrDefault(), file.Name), "pdf");
                filesToPDF.Add(new FileToProject(file.Path, file.Name, outputFileName, file.MethodPDFFile));
            }
            return filesToPDF;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected FileToProject(SerializationInfo info, StreamingContext context)
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
