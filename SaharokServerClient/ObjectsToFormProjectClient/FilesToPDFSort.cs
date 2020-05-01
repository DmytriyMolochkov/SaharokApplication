using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectsProjectClient;

namespace ObjectsToFormProjectClient
{
    public class FilesToPDFSort
    {
        public List<FileToProject> FilesToPDFfromPDF = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromWord = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromExcel = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromKompas = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromAutoCad = new List<FileToProject>();

        public static void CheckFilesToPDFSortToErrors(FilesToPDFSort filesToPDFSort)
        {
            List<Task> tasks = new List<Task>();

            List<FileToProject> filesToProject = new List<FileToProject>();
            filesToProject.AddRange(filesToPDFSort.FilesToPDFfromAutoCad);
            filesToProject.AddRange(filesToPDFSort.FilesToPDFfromExcel);
            filesToProject.AddRange(filesToPDFSort.FilesToPDFfromKompas);
            filesToProject.AddRange(filesToPDFSort.FilesToPDFfromPDF);
            filesToProject.AddRange(filesToPDFSort.FilesToPDFfromWord);

            var ErrorFiles = filesToProject.Where(file => file.MethodPDFFile == MethodPDFFile.NoPDFMethod)
                .Select(file => file.Path);

            if (ErrorFiles.Count() > 0)
                tasks.Add(Task.Run(() =>
                {
                    if (ErrorFiles.Count() == 1)
                    {
                        throw new Exception("Недопустимое расширение у файла: "
                                                   + Environment.NewLine + "      "
                                                   + String.Join(Environment.NewLine + "      ", ErrorFiles));
                    }
                    else
                    {
                        throw new Exception("Недопустимое расширение у файлов: "
                                                   + Environment.NewLine + "      "
                                                   + String.Join(Environment.NewLine + "      ", ErrorFiles));
                    }
                }));

            ErrorFiles = filesToProject.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                .Where(file => file.MethodPDFFile != MethodPDFFile.AutoCad)
                .GroupBy(file => file.OutputFileName).Where(group => group.ToList().Count > 1)
                .SelectMany(group => group).ToList()
                .Select(file => file.Path);
            if (ErrorFiles.Count() > 0)
                tasks.Add(Task.Run(() =>
                       throw new Exception("Имя PDF файлов будет одинаковым у следующих файлов: "
                           + Environment.NewLine + "      "
                           + String.Join(Environment.NewLine + "      ", ErrorFiles))));

            Task.WaitAll(tasks.ToArray());
        }
    }
}
