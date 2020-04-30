using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saharok.Model;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Excel;
using Kompas6API5;
using KAPITypes;
using KompasAPI7;
using Kompas6Constants;
using Pdf2d_LIBRARY;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.Interop.Common;
using WordApp = Microsoft.Office.Interop.Word.Application;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using KompasApp = Kompas6API5.KompasObject;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.IO.Compression;
using System.ComponentModel;
using ObjectsProjectClient;
using Saharok.Model.Client;
using ObjectsToFormProjectClient;

namespace Saharok.Model
{
    public static class FormProject
    {
        
        public static IEnumerable<IEnumerable<T>> SplitIntoGroupsByTheResultOfDividing<T>(
            this IEnumerable<T> source,
            int count)
        {
            return source
            .Select((x, y) => new { Index = y, Value = x })
            .GroupBy(x => x.Index / count)
            .Select(x => x.Select(y => y.Value).ToList())
            .ToList();
        }

        public static IEnumerable<IEnumerable<T>> SplitIntoGroupsByTheRemainderOfDividing<T>(
        this IEnumerable<T> source,
        int count)
        {
            return source
            .Select((x, y) => new { Index = y, Value = x })
            .GroupBy(x => x.Index % count)
            .Select(x => x.Select(y => y.Value).ToList())
            .ToList();
        }

        private class Apps
        {
            public class Kompas
            {
                public KompasApp kompasApp = null;
                public IApplication kompasApi7 = null;
                public IConverter ConverterPDF = null;

                public Kompas Quit()
                {
                    if (kompasApp != null)
                    {
                        kompasApp.Quit();
                        return null;
                    }
                    else
                        return null;
                }
            }

            public ExcelApp excel = null;
            public List<WordApp> words = new List<WordApp>();
            public List<Kompas> kompases = new List<Kompas>();

            public static void RunWord(ref WordApp word)
            {
                if (word == null)
                {
                    word = new WordApp();
                    word.Visible = false;
                    word.DisplayAlerts = WdAlertLevel.wdAlertsNone;
                    word.ScreenUpdating = false;
                }
            }

            public static void QuitWord(ref WordApp word)
            {
                if (word != null)
                {
                    word.Quit(ref saveChanges, ref oMissing, ref oMissing);
                    word = null;
                }
            }

            public static void QuitWords(ref List<WordApp> words)
            {
                foreach (WordApp word in words)
                {
                    if (word != null)
                    {
                        word.Quit(ref saveChanges, ref oMissing, ref oMissing);
                    }
                }
            }

            public static void RunExcel(ref ExcelApp excel)
            {
                if (excel == null)
                {
                    excel = new ExcelApp();
                    excel.Visible = true;
                    excel.Visible = false;
                    excel.ScreenUpdating = false;
                }
            }

            public static void QuitExcel(ref ExcelApp excel)
            {
                if (excel != null)
                {
                    excel.Quit();
                    excel = null;
                }
            }

            public static void RunKompas(ref Kompas kompas)
            {
                if (kompas.kompasApp == null)
                {
                    kompas.kompasApp = ConnectKompas.CreateKompas();
                    kompas.kompasApi7 = (IApplication)kompas.kompasApp.ksGetApplication7();
                    kompas.kompasApi7.Visible = false;
                    kompas.ConverterPDF = kompas.kompasApi7.get_Converter(Path.Combine(kompas.kompasApp.ksSystemPath(5), "Pdf2d.dll"));
                }
            }

            public static void QuitKompas(ref Kompas kompas)
            {
                if (kompas != null)
                {
                    if (kompas.kompasApp != null)
                    {
                        kompas.kompasApp.Quit();
                        kompas = null;
                    }
                }
            }

            public static void QuitApps(ref Apps apps)
            {
                QuitWords(ref apps.words);
                QuitExcel(ref apps.excel);
                foreach (Kompas kompas in apps.kompases)
                {
                    kompas.Quit();
                }
            }
        }


        //public static List<SectionToProject> GetSectionsToProject(Project project, string nameDirectorySection, string nameDirectoryFile)
        //{
        //    List<SectionToProject> Sections = new List<SectionToProject>();
        //    project.TypeDocumentations.ForEachImmediate(typeDocumentation =>
        //    {
        //        typeDocumentation.Sections.ForEachImmediate(section =>
        //        {
        //            Sections.Add(GetSectionToProject(section, nameDirectorySection, nameDirectoryFile));
        //        });
        //    });
        //    return Sections;
        //}
        //public static List<SectionToProject> GetSectionsToProject(TypeDocumentation typeDocumentation, string nameDirectorySection, string nameDirectoryFile)
        //{
        //    List<SectionToProject> Sections = new List<SectionToProject>();
        //    Project project = typeDocumentation.Project;
        //    typeDocumentation.Sections.ForEachImmediate(section =>
        //    {
        //        Sections.Add(GetSectionToProject(section, nameDirectorySection, nameDirectoryFile));
        //    });
        //    return Sections;
        //}
        //public static SectionToProject GetSectionToProject(ObjectsProject.Section section, string nameDirectorySection, string nameDirectoryFile)
        //{
        //    TypeDocumentation typeDocumentation = section.TypeDocumentation;
        //    Project project = typeDocumentation.Project;
        //    Dictionary<string, MethodFormFile> outputSectionPaths = new Dictionary<string, MethodFormFile>();
        //    outputSectionPaths.Add(
        //        Path.Combine(Path.Combine(project.Path, nameDirectorySection, "На отправку", typeDocumentation.Name),
        //            String.Join(" ", new string[]
        //            {
        //                section.Name.Split(' ')[0], project.CodeProject + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), project.Name + ".pdf"
        //            })),
        //        MethodFormFile.PDF);

        //    outputSectionPaths.Add(
        //        Path.Combine(Path.Combine(project.Path, nameDirectorySection, "На сервер", typeDocumentation.Name),
        //            String.Join(" ", new string[]
        //            {
        //                section.Name.Split(' ')[0], project.CodeProject + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), project.Name + ".pdf"
        //            })),
        //        MethodFormFile.PDF);

        //    outputSectionPaths.Add(
        //        Path.Combine(Path.Combine(project.Path, nameDirectorySection, "На сервер", typeDocumentation.Name),
        //            String.Join(" ", new string[]
        //            {
        //                section.Name.Split(' ')[0], project.CodeProject + "-" + String.Join(" ", section.Name.Split(' ').Skip(1)), project.Name + ".zip"
        //            })),
        //        MethodFormFile.ZIP);

        //    List<FileToProject> filesToPDF = new List<FileToProject>();
        //    section.Files.ForEachImmediate(file =>
        //    {
        //        string outputFileName = Path.ChangeExtension(Path.Combine(project.Path, nameDirectoryFile, typeDocumentation.Name, section.Name, file.Name), "pdf");
        //        filesToPDF.Add(new FileToProject(file.Path, file.Name, outputFileName, file.MethodPDFFile));
        //    });
        //    List<FileToProject> sortedfilesToPDF = filesToPDF.OrderBy(item => item.OutputFileName).ToList();
        //    return new SectionToProject(section.Path, outputSectionPaths, sortedfilesToPDF);
        //}

        //public static List<FileToProject> GetFilesToPDFToPages(FileSection file)
        //{
        //    string nameDirectoryFile = "PDF постранично";
        //    ObjectsProject.Section section = file.Section;
        //    TypeDocumentation typeDocumentation = section.TypeDocumentation;
        //    Project project = typeDocumentation.Project;
        //    List<FileToProject> filesToPDF = new List<FileToProject>();
        //    string outputFileName = Path.ChangeExtension(Path.Combine(project.Path, nameDirectoryFile, typeDocumentation.Name, section.Name, file.Name), "pdf");
        //    filesToPDF.Add(new FileToProject(file.Path, file.Name, outputFileName, file.MethodPDFFile));
        //    return filesToPDF;
        //}

        public static void CreateProject(Project project)
        {
            ClientObject.SendMessage(project);
            //ClientObject.SendMessage(project);
            //List<SectionToProject> sectionsToProject = GetSectionsToProject(project, "Готовый проект", "PDF постранично");
            //FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            //List<SectionToProject> sectionsToProject=null;
            //FilesToPDFSort filesToPDFSort = null;
            //FilesToPDFSort.CheckFilesToPDFSortToErrors(filesToPDFSort);
            //DoPDFFileUsingApps(filesToPDFSort);
            //CheckSectionIsDone(sectionsToProject);
        }
        public static void CreateProject(TypeDocumentation typeDocumentation)
        {
            ClientObject.SendMessage(typeDocumentation);
            //List<SectionToProject> sectionsToProject = GetSectionsToProject(typeDocumentation, "Готовый проект", "PDF постранично");
            //FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            //List<SectionToProject> sectionsToProject = null;
            //FilesToPDFSort filesToPDFSort = null;
            //FilesToPDFSort.CheckFilesToPDFSortToErrors(filesToPDFSort);
            //DoPDFFileUsingApps(filesToPDFSort);
            //CheckSectionIsDone(sectionsToProject);
        }
        public static void CreateProject(ObjectsProjectClient.Section section)
        {
            ClientObject.SendMessage(section);
            //List<SectionToProject> sectionsToProject = new List<SectionToProject> { GetSectionToProject(section, "Готовый проект", "PDF постранично") };
            //FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            //List<SectionToProject> sectionsToProject = null;
            //FilesToPDFSort filesToPDFSort = null;
            //FilesToPDFSort.CheckFilesToPDFSortToErrors(filesToPDFSort);
            //DoPDFFileUsingApps(filesToPDFSort);
            //CheckSectionIsDone(sectionsToProject);
        }
        public static void CreatFileToPDFToPages(FileSection file)
        {
            ClientObject.SendMessage(file);
            //FilesToPDFSort filesToPDFSort = new FilesToPDFSort(GetFilesToPDFToPages(file));
            //FilesToPDFSort filesToPDFSort = null;
            //FilesToPDFSort.CheckFilesToPDFSortToErrors(filesToPDFSort);
            //DoPDFFileUsingApps(filesToPDFSort);
        }

        public static bool CheckSectionReadiness(FileToProject fileToProject)
        {
            if (fileToProject.SectionToProject != null && fileToProject.SectionToProject.FilesToPDF.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).All(file => file.IsDone == true))
            {
                return true;
            }
            else
                return false;
        }

        public static void CheckSectionIsDone(List<SectionToProject> sectionsToProject)
        {
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();


            sectionsToProject.Where(section => section.IsDone == false).ToList().ForEach(section =>
            {
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    throw new Exception(
                    "Не удалось сформировать раздел: "
                    + Environment.NewLine
                    + Path.GetFileName(section.Path)
                    + " из-за несконвертированных в PDF файлов:"
                    + Environment.NewLine + "      "
                    + String.Join(
                        Environment.NewLine + "      ",
                        section.FilesToPDF.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).Where(file => file.IsDone == false).Select(file => file.Path))
                    );
                }));
            });
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
        }

        public static void CheckFilesToPDFIsDone(List<SectionToProject> sectionsToProject)
        {
            if (sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).Where(file => file.IsDone == false).ToList().Count() > 0)
            {

            }
        }

        private static void DoPDFFileUsingApps(FilesToPDFSort filesToPDFSort)
        {
            try
            {
                Apps apps = new Apps();
                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                List<System.Threading.Tasks.Task> localTasks = new List<System.Threading.Tasks.Task>();
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    filesToPDFSort.FilesToPDFfromPDF.ForEach(file =>
                    {
                        DoPDFfromPDF(file.Path, file.OutputFileName);
                        file.IsDone = true;
                        InfoOfProcess.CompleteFormsFiles++;
                        localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                        {
                            if (CheckSectionReadiness(file))
                            {
                                FormSection(file.SectionToProject);
                            }
                        }));
                    });
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    List<System.Threading.Tasks.Task> stackTasks = new List<System.Threading.Tasks.Task>();
                    filesToPDFSort.FilesToPDFfromWord.SplitIntoGroupsByTheRemainderOfDividing(2).ToList()
                        .ForEach(stack =>
                        {
                            stackTasks.Add(System.Threading.Tasks.Task.Run(() =>
                            {
                                WordApp word = new WordApp();
                                apps.words.Add(word);
                                stack.ToList().ForEach(file =>
                                {
                                    DoPDFfromWord(file.Path, file.OutputFileName, ref word);
                                    file.IsDone = true;
                                    InfoOfProcess.CompleteFormsFiles++;
                                    localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                                    {
                                        if (CheckSectionReadiness(file))
                                        {
                                            FormSection(file.SectionToProject);
                                        }
                                    }));
                                });
                                System.Threading.Tasks.Task.Run(() => Apps.QuitWord(ref word));
                            }));
                        });
                    System.Threading.Tasks.Task.WaitAll(stackTasks.ToArray());
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    if (filesToPDFSort.FilesToPDFfromExcel.Count() > 0)
                        Apps.RunExcel(ref apps.excel);
                    filesToPDFSort.FilesToPDFfromExcel.ForEach(file =>
                    {
                        DoPDFfromExcel(file.Path, file.OutputFileName, ref apps.excel);
                        file.IsDone = true;
                        InfoOfProcess.CompleteFormsFiles++;
                        localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                        {
                            if (CheckSectionReadiness(file))
                            {
                                FormSection(file.SectionToProject);
                            }
                        }));
                    });
                    System.Threading.Tasks.Task.Run(() => Apps.QuitExcel(ref apps.excel));
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    List<System.Threading.Tasks.Task> stackTasks = new List<System.Threading.Tasks.Task>();
                    filesToPDFSort.FilesToPDFfromKompas.SplitIntoGroupsByTheRemainderOfDividing(3).ToList()
                        .ForEach(stack =>
                         {
                             stackTasks.Add(System.Threading.Tasks.Task.Run(() =>
                             {
                                 Apps.Kompas kompas = new Apps.Kompas();
                                 apps.kompases.Add(kompas);
                                 stack.ToList().ForEach(file =>
                                 {
                                     DoPDFfromKompas(file.Path, file.OutputFileName, ref kompas);
                                     file.IsDone = true;
                                     InfoOfProcess.CompleteFormsFiles++;
                                     localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                                     {
                                         if (CheckSectionReadiness(file))
                                         {
                                             FormSection(file.SectionToProject);
                                         }
                                     }));
                                 });
                                 System.Threading.Tasks.Task.Run(() => kompas = kompas.Quit());
                             }));
                         });
                    System.Threading.Tasks.Task.WaitAll(stackTasks.ToArray());
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    filesToPDFSort.FilesToPDFfromAutoCad.ToList()
                        .ForEach(file =>
                        {

                            DoPDFfromAutoCAD(file.Path, file.OutputFileName);
                            file.IsDone = true;
                            InfoOfProcess.CompleteFormsFiles++;
                            localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                            {
                                if (CheckSectionReadiness(file))
                                {
                                    FormSection(file.SectionToProject);
                                }
                            }));
                        });
                }));

                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                System.Threading.Tasks.Task.WaitAll(localTasks.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae;
            }
        }

        private static void DoPDFfromPDF(string fileName, string outputFileName)
        {
            try
            {
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFileName));
                }
                File.Copy(fileName, outputFileName, true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message +
                    "\n TargetSite:\n" + ex.TargetSite +
                    "\n StackTrace:\n" + ex.StackTrace);
            }
        }

        private static void DoPDFfromWord(object fileName, object outputFileName, ref WordApp word)
        {
            try
            {
                Apps.RunWord(ref word);
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFileName.ToString()));
                }
                Microsoft.Office.Interop.Word.Document doc = word.Documents.Open(ref fileName, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                object fileFormat = WdSaveFormat.wdFormatPDF;
                doc.SaveAs(ref outputFileName,
                    ref fileFormat, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                ((_Document)doc).Close(ref saveChanges, ref oMissing, ref oMissing);
                doc = null;
            }
            catch (Exception ex)
            {
                Apps.QuitWord(ref word);
                throw new Exception(ex.Message +
                    "\n TargetSite:\n" + ex.TargetSite +
                    "\n StackTrace:\n" + ex.StackTrace);
            }
        }

        private static void DoPDFfromExcel(string fileName, object outputFileName, ref ExcelApp excel)
        {
            try
            {
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFileName.ToString()));
                }
                Apps.RunExcel(ref excel);
                Microsoft.Office.Interop.Excel.Workbook doc = excel.Workbooks.OpenXML(fileName, oMissing, oMissing);
                doc.Activate();
                doc.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, outputFileName, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                doc.Close(false, oMissing, oMissing);
                doc = null;
            }
            catch (Exception ex)
            {
                Apps.QuitExcel(ref excel);
                throw new Exception(ex.Message +
                    "\n TargetSite:\n" + ex.TargetSite +
                    "\n StackTrace:\n" + ex.StackTrace);
            }
        }

        private static void DoPDFfromKompas(string fileName, string outputFileName, ref Apps.Kompas kompas)
        {
            try
            {
                Apps.RunKompas(ref kompas);
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFileName));
                }
                kompas.ConverterPDF.Convert(fileName, outputFileName, 1, false);
            }
            catch (Exception ex)
            {
                Apps.QuitKompas(ref kompas);
                throw new Exception(ex.Message +
                    "\n TargetSite:\n" + ex.TargetSite +
                    "\n StackTrace:\n" + ex.StackTrace);
            }
        }

        private static void DoPDFfromAutoCAD(string fileName, string outputFileName)
        {
            //тут могла быть ваша реклама
        }

        public static void CombinePDF(List<string> pathSourceFile, string pathPDFFile)
        {
            using (var stream = new FileStream(pathPDFFile, FileMode.Create))
            {
                Merge(pathSourceFile, stream);
            }
        }

        private static void Merge(IEnumerable<string> fileNames, Stream target)
        {
            using (var document = new iTextSharp.text.Document())
            using (var pdf = new PdfCopy(document, target))
            {
                document.Open();
                foreach (string file in fileNames)
                {
                    using (var reader = new PdfReader(file))
                    {
                        pdf.AddDocument(reader);
                    }
                }
            }
        }

        public static void FormSection(SectionToProject sectionToProject)
        {
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
            foreach (var outputSectionPath in sectionToProject.OutputSectionPaths)
            {
                switch (outputSectionPath.Value)
                {
                    case MethodFormFile.ZIP:
                        {
                            if (sectionToProject.FilesToPDF.All(file => File.Exists(file.Path)))
                            {
                                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                                {
                                    if (File.Exists(outputSectionPath.Key))
                                    {
                                        File.Delete(outputSectionPath.Key);
                                    }
                                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(outputSectionPath.Key)))
                                    {
                                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputSectionPath.Key));
                                    }
                                    using (FileStream fileStream = new FileStream(outputSectionPath.Key, FileMode.CreateNew))
                                    {
                                        using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true, Encoding.GetEncoding(866)))
                                        {
                                            foreach (FileToProject fileToProject in sectionToProject.FilesToPDF)
                                            {
                                                try
                                                {
                                                    archive.CreateEntryFromFile(fileToProject.Path, fileToProject.Name);
                                                }
                                                catch (Exception e)
                                                {
                                                    throw e;
                                                }
                                            };

                                        }
                                    }
                                }));
                            }
                            else
                            {
                                throw new Exception("Для архива " + Path.GetFileName(outputSectionPath.Key) + "не удалось найти файлы : "
                                    + Environment.NewLine + "      "
                                    + String.Join(Environment.NewLine + "      ", sectionToProject.FilesToPDF.Where(file => !File.Exists(file.Path)).Select(file => file.Path)));
                            }
                            break;
                        }
                    case MethodFormFile.PDF:
                        {
                            if (sectionToProject.FilesToPDF.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).All(file => File.Exists(file.OutputFileName)))
                            {
                                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                                {
                                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(outputSectionPath.Key)))
                                    {
                                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputSectionPath.Key));
                                    }
                                    if (File.Exists(outputSectionPath.Key))
                                    {
                                        File.Delete(outputSectionPath.Key);
                                    }
                                    CombinePDF(sectionToProject.FilesToPDF
                                        .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                                        .Where(file => file.MethodPDFFile != MethodPDFFile.AutoCad)
                                        .Select(file => file.OutputFileName).ToList(), outputSectionPath.Key);

                                }));
                            }
                            else
                            {
                                throw new Exception("Для альбома " + Path.GetFileName(outputSectionPath.Key) + " не удалось найти PDF файлы : "
                                    + Environment.NewLine + "      "
                                    + String.Join(Environment.NewLine + "      ", sectionToProject.FilesToPDF.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).Where(file => !File.Exists(file.OutputFileName)).Select(file => file.OutputFileName)));
                            }
                            break;
                        }
                    default:
                        {
                            throw new Exception($"Неизвестный метод формирования секции: {outputSectionPath.Value}");
                        }
                }
            }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            sectionToProject.IsDone = true;
            InfoOfProcess.CompleteFormsSections++;
        }

        #region DefaultFields
        private static object oMissing = System.Reflection.Missing.Value;
        private static object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
        #endregion
    }
}
