using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ObjectsProjectServer;
using ObjectsToFormProjectServer;

namespace SaharokServer
{
    public static class FormProject
    {
        #region GetSectionToProject
        public static List<SectionToProject> GetSectionsToProject(Project project, string nameDirectorySection, string nameDirectoryFile)
        {
            List<SectionToProject> Sections = new List<SectionToProject>();
            project.TypeDocumentations.ForEachImmediate(typeDocumentation =>
            {
                typeDocumentation.Sections.ForEachImmediate(section =>
                {
                    Sections.Add(GetSectionToProject(section, nameDirectorySection, nameDirectoryFile));
                });
            });
            return Sections;
        }
        public static List<SectionToProject> GetSectionsToProject(TypeDocumentation typeDocumentation, string nameDirectorySection, string nameDirectoryFile)
        {
            List<SectionToProject> Sections = new List<SectionToProject>();
            typeDocumentation.Sections.ForEachImmediate(section =>
            {
                Sections.Add(GetSectionToProject(section, nameDirectorySection, nameDirectoryFile));
            });
            return Sections;
        }

        public static List<SectionToProject> GetSectionsToProject(Section section, string nameDirectorySection, string nameDirectoryFile)
        {
            List<SectionToProject> Sections = new List<SectionToProject>();
            Sections.Add(GetSectionToProject(section, nameDirectorySection, nameDirectoryFile));
            return Sections;
        }
        public static SectionToProject GetSectionToProject(Section section, string nameDirectorySection, string nameDirectoryFile)
        {
            TypeDocumentation typeDocumentation = section.TypeDocumentation;
            Project project = typeDocumentation.Project;
            Dictionary<string, MethodFormFile> outputSectionPaths = new Dictionary<string, MethodFormFile>();
            outputSectionPaths.Add(
                Path.Combine(Path.Combine(project.Path, nameDirectorySection, "На отправку", typeDocumentation.Name),
                    String.Join(" ", new string[]
                    {
                        section.Name.Split(' ')[0], project.CodeProject + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), project.Name + ".pdf"
                    })),
                MethodFormFile.PDF);

            outputSectionPaths.Add(
                Path.Combine(Path.Combine(project.Path, nameDirectorySection, "На сервер", typeDocumentation.Name),
                    String.Join(" ", new string[]
                    {
                        section.Name.Split(' ')[0], project.CodeProject + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), project.Name + ".pdf"
                    })),
                MethodFormFile.PDF);

            outputSectionPaths.Add(
                Path.Combine(Path.Combine(project.Path, nameDirectorySection, "На сервер", typeDocumentation.Name),
                    String.Join(" ", new string[]
                    {
                        section.Name.Split(' ')[0], project.CodeProject + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), project.Name + ".zip"
                    })),
                MethodFormFile.ZIP);

            List<FileToProject> filesToPDF = new List<FileToProject>();
            section.Files.ForEachImmediate(file =>
            {
                string outputFileName = Path.ChangeExtension(Path.Combine(project.Path, nameDirectoryFile, typeDocumentation.Name, section.Name, file.Name), "pdf");
                filesToPDF.Add(new FileToProject(file.Path, file.Name, outputFileName, file.MethodPDFFile));
            });
            List<FileToProject> sortedfilesToPDF = filesToPDF.OrderBy(item => item.OutputFileName).ToList();
            return new SectionToProject(section.Path, outputSectionPaths, sortedfilesToPDF);
        }
        #endregion

        public static List<FileToProject> GetFilesToPDFToPages(FileSection file)
        {
            string nameDirectoryFile = "PDF постранично";
            Section section = file.Section;
            TypeDocumentation typeDocumentation = section.TypeDocumentation;
            Project project = typeDocumentation.Project;
            List<FileToProject> filesToPDF = new List<FileToProject>();
            string outputFileName = Path.ChangeExtension(Path.Combine(project.Path, nameDirectoryFile, typeDocumentation.Name, section.Name, file.Name), "pdf");
            filesToPDF.Add(new FileToProject(file.Path, file.Name, outputFileName, file.MethodPDFFile));
            return filesToPDF;
        }

        #region CreateProject
        public static void CreateProject(Project project)
        {

            List<SectionToProject> sectionsToProject = GetSectionsToProject(project, "Готовый проект", "PDF постранично");
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            //DoPDFFileUsingApps(filesToPDFSort);
            //CheckSectionIsDone(sectionsToProject);
        }
        public static void CreateProject(TypeDocumentation typeDocumentation)
        {
            List<SectionToProject> sectionsToProject = GetSectionsToProject(typeDocumentation, "Готовый проект", "PDF постранично");
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            //DoPDFFileUsingApps(filesToPDFSort);
            //CheckSectionIsDone(sectionsToProject);
        }
        public static void CreateProject(Section section)
        {
            List<SectionToProject> sectionsToProject = GetSectionsToProject(section, "Готовый проект", "PDF постранично");
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            //DoPDFFileUsingApps(filesToPDFSort);
            //CheckSectionIsDone(sectionsToProject);
        }
        public static void CreatFileToPDFToPages(FileSection file)
        {
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(GetFilesToPDFToPages(file));
            //DoPDFFileUsingApps(filesToPDFSort);
        }
        #endregion
    }
}
