using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Text;

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
            var foldersConfigInfo = section.TypeDocumentation.Project.FoldersConfigInfo;
            TypeDocumentation typeDocumentation = section.TypeDocumentation;
            Project project = typeDocumentation.Project;
            Dictionary<string, MethodFormFile> outputSectionsPaths = new Dictionary<string, MethodFormFile>();
            string templateNameSection = project.SectionNameTemplate.Template;
            char templateSeparator = project.SectionNameTemplate.Separator;

            foldersConfigInfo.OutputFilesPDFDirectories.ForEach(directory => outputSectionsPaths.Add(
                System.IO.Path.Combine(section.GetPathProject(), directory, typeDocumentation.Name,
            GetSectionName(templateNameSection, project.Name, project.CodeProject, section.Name, templateSeparator, MethodFormFile.PDF)),
            MethodFormFile.PDF));

            foldersConfigInfo.OutputFilesZIPDirectories.ForEach(directory => outputSectionsPaths.Add(
                System.IO.Path.Combine(section.GetPathProject(), directory, typeDocumentation.Name,
            GetSectionName(templateNameSection, project.Name, project.CodeProject, section.Name, templateSeparator, MethodFormFile.ZIP)),
            MethodFormFile.ZIP));

            List<FileToProject> filesToPDF = new List<FileToProject>();
            section.Files.ForEachImmediate(file =>
            {
                string outputFileName = System.IO.Path.ChangeExtension(
                    System.IO.Path.Combine(section.GetPathProject(), foldersConfigInfo.OutputPageByPagePDF, typeDocumentation.Name, section.Name, file.Name), "pdf");
                filesToPDF.Add(new FileToProject(file.Path, file.Name, outputFileName, file.MethodPDFFile));
            });
            List<FileToProject> sortedfilesToPDF = filesToPDF.OrderBy(item => item.OutputFileName).ToList();
            return new SectionToProject(section.Path, outputSectionsPaths, sortedfilesToPDF);
        }

        const string nameNameProjectVariable = "|название проекта|";
        const string nameCodeProjectVariable = "|шифр проекта|";
        const string nameSectionVariable = "|раздел документации|";
        private static string GetSectionName(string template, string nameProject, string codeProject, string nameSection, char separator, MethodFormFile methodFromFile)
        {
            string[] arrayNameSection = nameSection.Split(separator);
            var sb = new StringBuilder(template.Replace(nameNameProjectVariable, nameProject).Replace(nameCodeProjectVariable, codeProject));
            int[] sectionVariablesIndexes = sb.ToString().AllIndexesOf(nameSectionVariable).ToArray();
            int sectionVariablesCount = sectionVariablesIndexes.Count();

            int offset = 0;
            for (int i = 0; i < sectionVariablesCount - 1 && i < arrayNameSection.Length; i++)
            {
                sb.Replace(nameSectionVariable, arrayNameSection[i], 0, sectionVariablesIndexes[i] + nameSectionVariable.Length + offset);
                offset += arrayNameSection[i].Length - nameSectionVariable.Length;
            }

            if (arrayNameSection.Length >= sectionVariablesCount)
            {
                sb.Replace(nameSectionVariable, String.Join(Convert.ToString(separator), arrayNameSection.Skip(sectionVariablesCount - 1).ToArray()),
                    0, sectionVariablesIndexes[sectionVariablesIndexes.Length - 1] + nameSectionVariable.Length + offset);
            }
            else sb.Replace(nameSectionVariable, "");

            switch (methodFromFile)
            {
                case MethodFormFile.PDF:
                    {
                        return sb.ToString() + ".pdf";
                    }
                case MethodFormFile.ZIP:
                    {
                        return sb.ToString() + ".zip";
                    }
                default:
                    {
                        throw new Exception($"Неизвестный метод формирования раздела документации: {methodFromFile}.");
                    }
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected SectionToProject(SerializationInfo info, StreamingContext context)
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
