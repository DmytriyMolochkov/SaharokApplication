using System;
using System.Collections.Generic;
using System.Linq;
using ObjectsProjectServer;

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
    }

    public class FileToProject
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
    }
    public static class InfoOfProcess
    {
        public static int TotalFormsFiles { get; set; }
        public static int TotalFormsSections { get; set; }
        public static int CompleteFormsFiles { get; set; }
        public static int CompleteFormsSections { get; set; }
    }

    public class FilesToPDFSort
    {
        public List<FileToProject> FilesToPDFfromPDF = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromWord = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromExcel = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromKompas = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromAutoCad = new List<FileToProject>();

        public FilesToPDFSort(List<SectionToProject> sectionsToProject)
        {
            InfoOfProcess.TotalFormsFiles = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).ToList().Count();
            InfoOfProcess.TotalFormsSections = sectionsToProject.Count();

            //CheckFilesToPDFSortToErrors(sectionsToProject);

            FilesToPDFfromPDF = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.PDF).ToList();
            FilesToPDFfromWord = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.Word).ToList();
            FilesToPDFfromExcel = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.Excel).ToList();
            FilesToPDFfromKompas = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.Kompas).ToList();
            FilesToPDFfromAutoCad = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.AutoCad).ToList();
        }
        public FilesToPDFSort(List<FileToProject> filesToPDF)
        {
            InfoOfProcess.TotalFormsFiles = filesToPDF.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).Count();

            filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.NoPDFMethod).ToList()
                .ForEach(errorFile => throw new Exception("Недопустимое расширение у файла: " + errorFile.Path));

            FilesToPDFfromPDF = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.PDF).ToList();
            FilesToPDFfromWord = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Word).ToList();
            FilesToPDFfromExcel = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Excel).ToList();
            FilesToPDFfromKompas = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Kompas).ToList();
            FilesToPDFfromAutoCad = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.AutoCad).ToList();
        }
    }
}
