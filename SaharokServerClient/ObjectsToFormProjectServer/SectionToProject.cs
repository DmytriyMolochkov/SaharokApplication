using System;
using System.Collections.Generic;
using System.Linq;
using ObjectsProjectServer;
using System.IO;

namespace ObjectsToFormProjectServer
{
    public class SectionToProject
    {
        public string Path;
        public Dictionary<string, MethodFormFile> OutputSectionPaths;
        public List<FileToProject> FilesToPDF;
        public bool IsDone;
        public SectionToProject(string SectionPath, Dictionary<string, MethodFormFile> outputSectionPaths, List<FileToProject> filesToPDF)
        {
            Path = SectionPath;
            OutputSectionPaths = outputSectionPaths;
            filesToPDF.ForEach(file => file.SectionToProject = this);
            IsDone = false;
            FilesToPDF = filesToPDF;
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
            Project project = typeDocumentation.Project;
            Dictionary<string, MethodFormFile> outputSectionsPaths = new Dictionary<string, MethodFormFile>();
            outputSectionsPaths.Add(
                System.IO.Path.Combine(System.IO.Path.Combine(project.Path, nameDirTypeDocumentation, "На отправку", typeDocumentation.Name),
                    String.Join(" ", new string[]
                    {
                        section.Name.Split(' ')[0], project.CodeProject + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), project.Name + ".pdf"
                    })),
                MethodFormFile.PDF);

            outputSectionsPaths.Add(
                System.IO.Path.Combine(System.IO.Path.Combine(project.Path, nameDirTypeDocumentation, "На сервер", typeDocumentation.Name),
                    String.Join(" ", new string[]
                    {
                        section.Name.Split(' ')[0], project.CodeProject + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), project.Name + ".pdf"
                    })),
                MethodFormFile.PDF);

            outputSectionsPaths.Add(
                System.IO.Path.Combine(System.IO.Path.Combine(project.Path, nameDirTypeDocumentation, "На сервер", typeDocumentation.Name),
                    String.Join(" ", new string[]
                    {
                        section.Name.Split(' ')[0], project.CodeProject + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), project.Name + ".zip"
                    })),
                MethodFormFile.ZIP);

            List<FileToProject> filesToPDF = new List<FileToProject>();
            section.Files.ForEachImmediate(file =>
            {
                string outputFileName = System.IO.Path.ChangeExtension(
                    System.IO.Path.Combine(project.Path, nameDirFileToPDFToPages, typeDocumentation.Name, section.Name, file.Name), "pdf");
                filesToPDF.Add(new FileToProject(file.Path, file.Name, outputFileName, file.MethodPDFFile));
            });
            List<FileToProject> sortedfilesToPDF = filesToPDF.OrderBy(item => item.OutputFileName).ToList();
            return new SectionToProject(section.Path, outputSectionsPaths, sortedfilesToPDF);
            
        }
    }
}
