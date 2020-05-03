using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Excel;
using KompasAPI7;
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

        public static void CreateProject(Project project)
        {
            ClientObject.SendMessage(project);
            FilesToPDFSort filesToPDFSort = ClientObject.ReceiveMessage();
            filesToPDFSort.CheckFilesToPDFSortToErrors();
            DoPDFFileUsingApps(filesToPDFSort);
            CheckSectionsIsDone(filesToPDFSort);
        }
        public static void CreateProject(TypeDocumentation typeDocumentation)
        {
            ClientObject.SendMessage(typeDocumentation);
            ClientObject.SendMessage(typeDocumentation);
            FilesToPDFSort filesToPDFSort = ClientObject.ReceiveMessage();
            filesToPDFSort.CheckFilesToPDFSortToErrors();
            DoPDFFileUsingApps(filesToPDFSort);
            CheckSectionsIsDone(filesToPDFSort);
        }
        public static void CreateProject(ObjectsProjectClient.Section section)
        {
            ClientObject.SendMessage(section);
            ClientObject.SendMessage(section);
            FilesToPDFSort filesToPDFSort = ClientObject.ReceiveMessage();
            filesToPDFSort.CheckFilesToPDFSortToErrors();
            DoPDFFileUsingApps(filesToPDFSort);
            CheckSectionsIsDone(filesToPDFSort);
        }
        public static void CreateProject(FileSection file)
        {
            ClientObject.SendMessage(file);
            ClientObject.SendMessage(file);
            FilesToPDFSort filesToPDFSort = ClientObject.ReceiveMessage();
            filesToPDFSort.CheckFilesToPDFSortToErrors();
            DoPDFFileUsingApps(filesToPDFSort);
            CheckSectionsIsDone(filesToPDFSort);
        }

        public static bool CheckSectionReadiness(FileToProject fileToProject)
        {
            if (fileToProject.SectionToProject != null &&
                fileToProject.SectionToProject.FilesToProject
                .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                .All(file => file.IsDone == true))
            {
                return true;
            }
            else
                return false;
        }

        public static void CheckSectionsIsDone(FilesToPDFSort filesToPDFSort)
        {
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
            filesToPDFSort.GetAllSectionsToProject().Where(section => section.IsDone == false).ToList().ForEach(section =>
            {
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    throw new Exception(
                    $"Не удалось сформировать раздел:{Environment.NewLine}" +
                    $"{Path.GetFileName(section.Path)} из-за несконвертированных в PDF файлов: {Environment.NewLine}      " +
                    String.Join($"{Environment.NewLine}      ",
                        section.FilesToProject
                        .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                        .Where(file => file.IsDone == false)
                        .Select(file => file.Path)) + Environment.NewLine);
                }));
            });
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
        }

        //public static void CheckFilesToProjectIsDone(List<SectionToProject> sectionsToProject)
        //{
        //    if (sectionsToProject
        //        .SelectMany(section => section.FilesToProject)
        //        .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
        //        .All(file => file.IsDone == true))
        //    {

        //    }
        //    else
        //        ;
        //}

        private static object syncRoot = new Object();

        private static void DoPDFFileUsingApps(FilesToPDFSort filesToPDFSort)
        {
            try
            {
                Apps apps = new Apps();
                InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                List<System.Threading.Tasks.Task> localTasks = new List<System.Threading.Tasks.Task>();
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    bool sectionReadiness;
                    filesToPDFSort.FilesToProjectfromPDF.ForEach(file =>
                    {
                        DoPDFfromPDF(file.Path, file.OutputFileName);
                        lock (syncRoot)
                        {
                            file.IsDone = true;
                            infoOfProcess.CompleteFormsFiles++;
                            sectionReadiness = CheckSectionReadiness(file);
                        }
                        if (sectionReadiness)
                        {
                            localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                            {
                                FormSection(file.SectionToProject);
                            }));
                        }
                    });
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    List<System.Threading.Tasks.Task> stackTasks = new List<System.Threading.Tasks.Task>();
                    filesToPDFSort.FilesToProjectfromWord.SplitIntoGroupsByTheRemainderOfDividing(2).ToList()
                        .ForEach(stack =>
                        {
                            stackTasks.Add(System.Threading.Tasks.Task.Run(() =>
                            {
                                WordApp word = new WordApp();
                                apps.words.Add(word);
                                bool sectionReadiness;
                                stack.ToList().ForEach(file =>
                                {
                                    DoPDFfromWord(file.Path, file.OutputFileName, ref word);
                                    lock (syncRoot)
                                    {
                                        file.IsDone = true;
                                        infoOfProcess.CompleteFormsFiles++;
                                        sectionReadiness = CheckSectionReadiness(file);
                                    }
                                    if (sectionReadiness)
                                    {
                                        localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                                        {
                                            FormSection(file.SectionToProject);
                                        }));
                                    }
                                });
                                System.Threading.Tasks.Task.Run(() => Apps.QuitWord(ref word));
                            }));
                        });
                    System.Threading.Tasks.Task.WaitAll(stackTasks.ToArray());
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    if (filesToPDFSort.FilesToProjectfromExcel.Count() > 0)
                        Apps.RunExcel(ref apps.excel);
                    bool sectionReadiness;
                    filesToPDFSort.FilesToProjectfromExcel.ForEach(file =>
                    {
                        DoPDFfromExcel(file.Path, file.OutputFileName, ref apps.excel);
                        lock (syncRoot)
                        {
                            file.IsDone = true;
                            infoOfProcess.CompleteFormsFiles++;
                            sectionReadiness = CheckSectionReadiness(file);
                        }
                        if (sectionReadiness)
                        {
                            localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                            {
                                FormSection(file.SectionToProject);
                            }));
                        }
                    });
                    System.Threading.Tasks.Task.Run(() => Apps.QuitExcel(ref apps.excel));
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    List<System.Threading.Tasks.Task> stackTasks = new List<System.Threading.Tasks.Task>();
                    filesToPDFSort.FilesToProjectfromKompas.SplitIntoGroupsByTheRemainderOfDividing(3).ToList()
                        .ForEach(stack =>
                         {
                             stackTasks.Add(System.Threading.Tasks.Task.Run(() =>
                             {
                                 Apps.Kompas kompas = new Apps.Kompas();
                                 apps.kompases.Add(kompas);
                                 bool sectionReadiness;
                                 stack.ToList().ForEach(file =>
                                 {
                                     DoPDFfromKompas(file.Path, file.OutputFileName, ref kompas);
                                     lock (syncRoot)
                                     {
                                         file.IsDone = true;
                                         infoOfProcess.CompleteFormsFiles++;
                                         sectionReadiness = CheckSectionReadiness(file);
                                     }
                                     if (sectionReadiness)
                                     {
                                         localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                                         {
                                             FormSection(file.SectionToProject);
                                         }));
                                     }
                                 });
                                 System.Threading.Tasks.Task.Run(() => kompas = kompas.Quit());
                             }));
                         });
                    System.Threading.Tasks.Task.WaitAll(stackTasks.ToArray());
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    bool sectionReadiness;
                    filesToPDFSort.FilesToProjectfromAutoCad.ToList().ForEach(file =>
                        {
                            DoPDFfromAutoCAD(file.Path, file.OutputFileName);
                            lock (syncRoot)
                            {
                                file.IsDone = true;
                                infoOfProcess.CompleteFormsFiles++;
                                sectionReadiness = CheckSectionReadiness(file);
                            }
                            if (sectionReadiness)
                            {
                                localTasks.Add(System.Threading.Tasks.Task.Run(() =>
                                {
                                    FormSection(file.SectionToProject);
                                }));
                            }
                        });
                }));

                var emptySectionsFromPDF = new List<string>();
                filesToPDFSort.GetAllSectionsToProject()
                    .Where(section => section.FilesToProject.All(file => file.MethodPDFFile == MethodPDFFile.DontPDF))
                    .ToList()
                    .ForEach(section => 
                    {
                        emptySectionsFromPDF.Add(section.Path);
                        tasks.Add(System.Threading.Tasks.Task.Run(() => FormSection(section)));
                    });

                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                System.Threading.Tasks.Task.WaitAll(localTasks.ToArray());

                if (emptySectionsFromPDF.Count == 1)
                {
                    System.Windows.MessageBox.Show($"Обратите внивание, что следующий раздел не содержат файлов для PDF альбома: {Environment.NewLine}      " +
                        String.Join($"{Environment.NewLine}      ", emptySectionsFromPDF) +
                        $"{Environment.NewLine}" +
                        $"{Environment.NewLine}      " +
                        "поэтому для него был сформирован только ZIP архив.");
                }
                else if (emptySectionsFromPDF.Count > 1)
                {
                    System.Windows.MessageBox.Show($"Обратите внивание, что следующие разделы не содержат файлов для PDF альбома: {Environment.NewLine}      " +
                    String.Join($"{Environment.NewLine}      ", emptySectionsFromPDF) +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}      " +
                    "поэтому для них были сформированы только ZIP архивы.");
                }
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
                throw new Exception($"При копировании файла {fileName} произошла ошибка: {Environment.NewLine} {ex.Message}.{Environment.NewLine}"
                    );
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
                throw new Exception($"При формировании файла PDF {fileName} Word'ом произошла ошибка: {Environment.NewLine} {ex.Message}.{Environment.NewLine}");
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
                throw new Exception($"При формировании файла PDF {fileName} Excel'ем произошла ошибка: {Environment.NewLine} {ex.Message}.{Environment.NewLine}");
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
                throw new Exception($"При формировании файла PDF {fileName} Kompas'ом произошла ошибка: {Environment.NewLine} {ex.Message}.{Environment.NewLine}");
            }
        }

        private static void DoPDFfromAutoCAD(string fileName, string outputFileName)
        {
            //тут могла быть ваша реклама
        }

        public static void CombinePDF(List<string> pathSourceFiles, string pathPDFFile)
        {
            if (pathSourceFiles.Count > 0)
            {
                using (var stream = new FileStream(pathPDFFile, FileMode.Create))
                {
                    Merge(pathSourceFiles, stream);
                }
            }
        }
        private static void Merge(List<string> files, FileStream stream)
        {
            try
            {
                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();

                using (var pdf = new PdfDocument(new PdfWriter(stream)))
                {
                    PdfMerger merger = new PdfMerger(pdf);
                    foreach (var file in files)
                    {
                        using (var SourcePdf = new PdfDocument(new PdfReader(file)))
                        {
                            merger.Merge(SourcePdf, 1, SourcePdf.GetNumberOfPages());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static void FormSection(SectionToProject sectionToProject)
        {
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
            foreach (var outputSectionPath in sectionToProject.OutputSectionPaths)
            {
                switch (outputSectionPath.Value)
                {
                    case MethodFormFile.ZIP:
                        {
                            if (sectionToProject.FilesToProject.All(file => File.Exists(file.Path)))
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
                                            foreach (FileToProject fileToProject in sectionToProject.FilesToProject)
                                            {
                                                try
                                                {
                                                    archive.CreateEntryFromFile(fileToProject.Path, fileToProject.Name);
                                                }
                                                catch (AggregateException ae)
                                                {
                                                    throw ae;
                                                }
                                            };
                                        }
                                    }
                                }));
                            }
                            else
                            {
                                throw new Exception($"Для архива {Path.GetFileName(outputSectionPath.Key)} не удалось найти файлы: {Environment.NewLine}      "
                                    + String.Join(
                                        $"{Environment.NewLine}      ", sectionToProject.FilesToProject
                                        .Where(file => !File.Exists(file.Path))
                                        .Select(file => file.Path))
                                    + Environment.NewLine);
                            }
                            break;
                        }
                    case MethodFormFile.PDF:
                        {
                            if (sectionToProject.FilesToProject.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).All(file => File.Exists(file.OutputFileName)))
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
                                    try
                                    {
                                        CombinePDF(sectionToProject.FilesToProject
                                            .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                                            .Where(file => file.MethodPDFFile != MethodPDFFile.AutoCad)
                                            .Select(file => file.OutputFileName).ToList(), outputSectionPath.Key);
                                    }
                                    catch (AggregateException ae)
                                    {
                                        throw ae;
                                    }

                                }));
                            }
                            else
                            {
                                throw new Exception($"Для альбома {Path.GetFileName(outputSectionPath.Key)} не удалось найти PDF файлы: {Environment.NewLine}      "
                                    + String.Join(
                                        $"{Environment.NewLine}      ", sectionToProject.FilesToProject
                                        .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                                        .Where(file => !File.Exists(file.OutputFileName))
                                        .Select(file => file.OutputFileName))
                                    + Environment.NewLine);
                            }
                            break;
                        }
                    default:
                        {
                            throw new Exception($"Неизвестный метод формирования секции: {outputSectionPath.Value}.{Environment.NewLine}");
                        }
                }
            }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            sectionToProject.IsDone = true;
            infoOfProcess.CompleteFormsSections++;
        }

        #region DefaultFields
        private static object oMissing = System.Reflection.Missing.Value;
        private static object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
        #endregion
    }
}
