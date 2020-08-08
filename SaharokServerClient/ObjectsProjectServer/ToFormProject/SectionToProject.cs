using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace ObjectsProjectServer
{
    [Serializable]
    public class SectionToProject : ISerializable
    {
        public string Path;
        public Dictionary<string, MethodFormFile> OutputSectionPaths;
        public List<FileToProject> FilesToProject;
        public bool IsDone;
        public SectionToProject(string SectionPath, Dictionary<string, MethodFormFile> outputSectionPaths, List<FileToProject> filesToPDF)
        {
            Path = SectionPath;
            OutputSectionPaths = outputSectionPaths;
            filesToPDF.ForEach(file => file.SectionToProject = this);
            IsDone = false;
            FilesToProject = filesToPDF;
        }

        public static List<SectionToProject> GetSections(Project project)
        {
            List<SectionToProject> Sections = new List<SectionToProject>();
            project.TypeDocumentations.ForEachImmediate(typeDocumentation =>
            {
                typeDocumentation.Sections.ForEachImmediate(section =>
                {
                    Sections.Add(GetSection(section));
                });
            });
            return Sections;
        }
        public static List<SectionToProject> GetSections(TypeDocumentation typeDocumentation)
        {
            List<SectionToProject> Sections = new List<SectionToProject>();
            typeDocumentation.Sections.ForEachImmediate(section =>
            {
                Sections.Add(GetSection(section));
            });
            return Sections;
        }
        public static SectionToProject GetSection(Section section)
        {
            string nameDirTypeDocumentation = "Готовый проект";
            string nameDirFileToPDFToPages = "PDF постранично";
            TypeDocumentation typeDocumentation = section.TypeDocumentation;
            Dictionary<string, MethodFormFile> outputSectionsPaths = new Dictionary<string, MethodFormFile>();
            outputSectionsPaths.Add(
                System.IO.Path.Combine(System.IO.Path.Combine(section.GetPathProject(), nameDirTypeDocumentation, "На отправку", typeDocumentation.Name),
                    String.Join(" ", new string[]
                    {
                        section.Name.Split(' ')[0], section.GetCodeProject() + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), section.GetNameProject() + ".pdf"
                    })),
                MethodFormFile.PDF);

            outputSectionsPaths.Add(
                System.IO.Path.Combine(System.IO.Path.Combine(section.GetPathProject(), nameDirTypeDocumentation, "На сервер", typeDocumentation.Name),
                    String.Join(" ", new string[]
                    {
                        section.Name.Split(' ')[0], section.GetCodeProject() + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), section.GetNameProject() + ".pdf"
                    })),
                MethodFormFile.PDF);

            outputSectionsPaths.Add(
                System.IO.Path.Combine(System.IO.Path.Combine(section.GetPathProject(), nameDirTypeDocumentation, "На сервер", typeDocumentation.Name),
                    String.Join(" ", new string[]
                    {
                        section.Name.Split(' ')[0], section.GetCodeProject() + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), section.GetNameProject() + ".zip"
                    })),
                MethodFormFile.ZIP);

            List<FileToProject> filesToPDF = new List<FileToProject>();
            section.Files.ForEachImmediate(file =>
            {
                string outputFileName = System.IO.Path.ChangeExtension(
                    System.IO.Path.Combine(section.GetPathProject(), nameDirFileToPDFToPages, typeDocumentation.Name, section.Name, file.Name), "pdf");
                filesToPDF.Add(new FileToProject(file.Path, file.Name, outputFileName, file.MethodPDFFile));
            });
            List<FileToProject> sortedfilesToPDF = filesToPDF.OrderBy(item => item.OutputFileName).ToList();
            return new SectionToProject(section.Path, outputSectionsPaths, sortedfilesToPDF);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected SectionToProject(SerializationInfo info, StreamingContext context)
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
