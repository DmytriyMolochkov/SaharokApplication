using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
//using Microsoft.Office.Interop.Word;
//using Microsoft.Office.Interop.Excel;
//using KompasAPI7;
//using WordApp = Microsoft.Office.Interop.Word.Application;
//using ExcelApp = Microsoft.Office.Interop.Excel.Application;
//using KompasApp = Kompas6API5.KompasObject;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.IO.Compression;
using System.ComponentModel;
using Saharok.Model.Client;
using System.Collections.Concurrent;
using System.Configuration;
using System.Reflection;
using System.Runtime.Remoting;
using Org.BouncyCastle.Asn1.X509;
using System.Diagnostics;

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

        public static bool ApplicationVisibility { get; private set; } = false;
        private static object lockWord = new Object();
        private static object lockKompas = new Object();
        private static object lockExcel = new Object();
        private static object lockAutoCAD = new Object();
        private static object lockNanoCAD = new Object();
        private static object _lock = new Object();

        private static bool isLoadDLLAutoCad = false; // не будет работать корректно если автокадов больше 1шт.

        private static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private static CancellationToken token = cancelTokenSource.Token;
        private static bool isManuallyCancellToken = false;

        public static class Apps
        {
            public class Kompas
            {
                public dynamic kompasApp = null;
                public dynamic kompasApi7 = null;
                public dynamic ConverterPDF = null;

                public void Quit()
                {
                    lock (lockKompas)
                    {
                        try
                        {
                            if (kompasApp != null)
                            {
                                ((object)kompasApp).GetType().InvokeMember("Quit",
                                    BindingFlags.InvokeMethod, null, kompasApp, new object[] { });
                            }
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }

            public static List<dynamic> excels = new List<dynamic>();
            public static List<dynamic> words = new List<dynamic>();
            public static List<Kompas> kompases = new List<Kompas>();
            public static List<dynamic> autocads = new List<dynamic>();
            public static List<dynamic> nanocads = new List<dynamic>();

            public static void RunWord(ref dynamic word)
            {
                lock (lockWord)
                {
                    if (word == null)
                    {
                        word = Activator.CreateInstance(Type.GetTypeFromProgID("Word.Application"));
                        words.Add(word);
                        word.Visible = false;
                        word.DisplayAlerts = 0; //WdAlertLevel.wdAlertsNone
                        word.ScreenUpdating = false;
                    }
                }
            }

            public static void QuitWord(ref dynamic word)
            {
                lock (lockWord)
                {
                    if (word != null)
                    {
                        try
                        {
                            words.Remove(word);
                            word.Quit(saveChanges);
                            word = null;
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }

            public static void QuitWords()
            {
                lock (lockWord)
                {
                    try
                    {
                        words.Where(word => word != null).ForEachImmediate(word =>
                        {
                            try
                            {
                                word.Quit(saveChanges);
                                word = null;
                            }
                            catch { }
                        });
                        words.Clear();
                    }
                    catch { }
                }
            }

            public static void RunExcel(ref dynamic excel)
            {
                lock (lockExcel)
                {
                    if (excel == null)
                    {
                        excel = Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"));
                        excels.Add(excel);
                        excel.Visible = true;
                        excel.Visible = false;
                        excel.ScreenUpdating = false;
                    }
                }
            }

            public static void QuitExcel(ref dynamic excel)
            {
                lock (lockExcel)
                {

                    if (excel != null)
                    {
                        try
                        {
                            excel.Quit();
                            excels.Remove(excel);
                            excel = null;
                        }
                        catch (Exception ex)
                        { }
                    }

                }
            }

            public static void QuitExcels()
            {
                lock (lockExcel)
                {
                    try
                    {
                        excels.Where(excel => excel != null).ForEachImmediate(excel =>
                        {
                            try
                            {
                                excel.Quit();
                                excel = null;
                            }
                            catch { }
                        });
                        excels.Clear();
                    }
                    catch { }
                }
            }

            public static void RunKompas(Kompas kompas)
            {
                lock (lockKompas)
                {
                    if (kompas.kompasApp == null)
                    {
                        kompases?.Add(kompas);
                        kompas.kompasApp = ConnectKompas.CreateKompas();
                        kompas.kompasApi7 = ((object)kompas.kompasApp).GetType().InvokeMember("ksGetApplication7",
                            BindingFlags.InvokeMethod, null, kompas.kompasApp, new object[] { });
                        kompas.kompasApi7.Visible = false;
                        kompas.ConverterPDF = kompas.kompasApi7.GetType().InvokeMember("Converter",
                            BindingFlags.GetProperty, null, kompas.kompasApi7, new object[1] {
                                Path.Combine(((object)kompas.kompasApp).GetType().InvokeMember("ksSystemPath",
                                    BindingFlags.InvokeMethod, null, kompas.kompasApp, new object[1] { 5 }), "Pdf2d.dll")
                            });
                    }
                }
            }

            public static void QuitKompas(Kompas kompas)
            {
                lock (lockKompas)
                {
                    if (kompas != null && kompas.kompasApp != null)
                    {
                        try
                        {
                            kompas.Quit();
                            kompases.Remove(kompas);
                            kompas.kompasApp = null;
                            kompas.kompasApi7 = null;
                            kompas = null;
                            kompas = null;
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }

            public static void QuitKompases()
            {
                lock (lockKompas)
                {
                    try
                    {
                        kompases.Where(kompas => kompas != null && kompas.kompasApp != null).ForEachImmediate(kompas =>
                        {
                            try
                            {
                                kompas.Quit();
                                kompas.kompasApp = null;
                                kompas.kompasApi7 = null;
                                kompas = null;
                            }
                            catch { }
                        });
                        kompases.Clear();
                    }
                    catch { }
                }
            }

            public static dynamic RunAutoCAD(dynamic autocad)
            {
                lock (lockAutoCAD)
                {
                    if (autocad == null)
                    {
                        isLoadDLLAutoCad = false;
                        autocad = TryRun();
                        autocads.Add(autocad);
                        TryExecute(() => autocad.Application.Visible = false);
                        TryExecute(() => ShowWindow((int)autocad.HWND, 0));
                        dynamic acadDoc = null;
                        TryExecute(() =>
                        {
                            if (autocad.Documents.Count > 0)
                                acadDoc = autocad.ActiveDocument;
                        });
                        if (acadDoc != null)
                        {
                            TryExecute(() => acadDoc.SetVariable("FILEDIA", 0));
                            TryExecute(() => acadDoc.SendCommand($"TRUSTEDPATHS {Environment.CurrentDirectory}\\ \n"));
                            TryExecute(() => acadDoc.SendCommand($"_NETLOAD \"{Path.Combine(Environment.CurrentDirectory, "acPlt.dll")}\" \n"));
                            isLoadDLLAutoCad = true;
                            TryExecute(() => acadDoc.Close(false));
                            TryExecute(() => ShowWindow((int)autocad.HWND, 0));
                        }
                    }
                    return autocad;
                }
            }

            public static dynamic QuitAutoCAD(dynamic autocad)
            {
                lock (lockAutoCAD)
                {
                    if (autocad != null)
                    {
                        try
                        {
                            autocads.Remove(autocad);
                            TryExecute(() => autocad.Quit());
                            autocad = null;
                        }
                        catch (Exception ex)
                        { }
                    }
                    return null;
                }
            }

            public static void QuitAutoCADs()
            {
                lock (lockAutoCAD)
                {
                    try
                    {
                        autocads.Where(autocad => autocad != null).ForEachImmediate(autocad =>
                        {
                            try
                            {
                                TryExecute(() => autocad.Quit());
                                autocad = null;
                            }
                            catch { }
                        });
                        autocads.Clear();
                    }
                    catch { }
                }
            }

            static void TryExecute(Action action)
            {
                int i = 0;
                while (true)
                {
                    try
                    {
                        action();
                        return;
                    }
                    catch (Exception ex)
                    {

                        if (++i == 20)
                            throw ex;
                        Thread.Sleep(1000);
                    }
                }
            }

            static dynamic TryRun()
            {
                int i = 0;
                while (true)
                {
                    try
                    {
                        return Activator.CreateInstance(Type.GetTypeFromProgID("AutoCAD.Application"));
                    }
                    catch (Exception ex)
                    {
                        if (++i == 5)
                            throw ex;
                        else
                            Process.GetProcessesByName("acad").Where(p => p.MainWindowHandle == IntPtr.Zero).ToList().ForEach(p => p.Kill());
                    }
                }
            }

            public static void RunNanoCAD(ref dynamic nanocad)
            {
                lock (lockNanoCAD)
                {
                    if (nanocad == null)
                    {
                        
                        string version = ConfigurationManager.AppSettings["NanoCADVersion"];
                        nanocad = Activator.CreateInstance(Type.GetTypeFromProgID($"nanoCAD.Application{(version == "" ? "" : '.' + version)}"));
                        Thread.Sleep(100);
                        nanocad.Visible = false;
                        nanocads.Add(nanocad);
                        if (nanocad.Documents.Count > 0)
                        {
                            dynamic ncadDoc = nanocad.ActiveDocument;
                            ncadDoc.Close(false);
                        }
                    }
                }
            }

            public static void QuitNanoCAD(ref dynamic nanocad)
            {
                lock (lockNanoCAD)
                {

                    if (nanocad != null)
                    {
                        try
                        {
                            Thread.Sleep(4000);
                            nanocad.Visible = true;
                            nanocad.Quit();
                            nanocads.Remove(nanocad);
                            nanocad = null;
                        }
                        catch (Exception ex)
                        { }
                    }

                }
            }

            public static void QuitNanoCADs()
            {
                lock (lockNanoCAD)
                {
                    try
                    {
                        nanocads.Where(nanocad => nanocad != null).ForEachImmediate(nanocad =>
                        {
                            try
                            {
                                nanocad.Visible = true;
                                nanocad.Quit();
                                nanocad = null;
                            }
                            catch { }
                        });
                        nanocads.Clear();
                    }
                    catch { }
                }
            }

            public static void QuitApps()
            {
                QuitKompases();
                QuitWords();
                QuitExcels();
                QuitAutoCADs();
                QuitNanoCADs();
            }

            public static void ChangeApplicationVisibility()
            {
                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    lock (lockWord)
                    {
                        words.Where(word => word != null).ForEachImmediate(word =>
                        {
                            word.Visible = !ApplicationVisibility;
                            word.ScreenUpdating = !ApplicationVisibility;
                            word.DisplayAlerts = (ApplicationVisibility ? 0 : -1); //WdAlertLevel.wdAlertsNone : WdAlertLevel.wdAlertsAll
                        });
                    }
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    lock (lockExcel)
                    {
                        excels.Where(excel => excel != null).ForEachImmediate(excel =>
                        {
                            excel.Visible = !ApplicationVisibility;
                            excel.ScreenUpdating = !ApplicationVisibility;
                        });
                    }
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    lock (lockKompas)
                    {
                        kompases.Where(kompas => kompas != null && kompas.kompasApp != null && kompas.kompasApi7 != null).ForEachImmediate(kompas =>
                        {
                            try
                            {
                                kompas.kompasApi7.Visible = !ApplicationVisibility;
                            }
                            catch (Exception) { } // часто кидает ошибки, если приложение в процессе открытия или закрытия
                        });
                    }
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    lock (lockAutoCAD)
                    {
                        autocads.Where(autocad => autocad != null).ForEachImmediate(autocad =>
                        {
                            TryExecute(() => ShowWindow((int)autocad.HWND, Convert.ToInt32(!ApplicationVisibility)));
                            //TryExecute(() => autocad.Application.Visible = !ApplicationVisibility);
                        });
                    }
                }));
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    lock (lockNanoCAD)
                    {
                        nanocads.Where(nanocad => nanocad != null).ForEachImmediate(nanocad =>
                        {
                            nanocad.Visible = !ApplicationVisibility;
                        });
                    }
                }));
                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                ApplicationVisibility = !ApplicationVisibility;
            }
        }

        public static void CreateProject(object objectsToProject, ClientObject clientObject)
        {
            try
            {
                clientObject.SendMessage(objectsToProject);
                FilesToPDFSort filesToPDFSort = clientObject.ReceiveMessage();

                filesToPDFSort.CheckFilesToPDFSortToErrors();
                DoPDFFileUsingApps(filesToPDFSort);

                if (!token.IsCancellationRequested)
                    CheckSectionsIsDone(filesToPDFSort);
            }
            catch (Exception ex)
            {
                if (!isManuallyCancellToken)
                {
                    throw ex;
                }
                isManuallyCancellToken = false;
            }
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
            string errorMessage = null;
            filesToPDFSort.GetAllSectionsToProject()?
                .Where(section => section.FilesToProject.Count > 0)
                .Where(section => section.IsDone == false)
                .ForEachImmediate(section =>
                {
                    errorMessage =
                        $"Не удалось сформировать раздел:{Environment.NewLine}" +
                        $"{Path.GetFileName(section.Path) + Environment.NewLine}" +
                        $"из-за несконвертированных в PDF файлов:{Environment.NewLine}" +
                        String.Join($"{Environment.NewLine}      ",
                        section.FilesToProject
                        .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                        .Where(file => file.IsDone == false)
                        .Select(file => file.Path));
                });
            if (errorMessage != null)
                throw new Exception(errorMessage + Environment.NewLine + Environment.NewLine);
        }

        private static void DoPDFFileUsingApps(FilesToPDFSort filesToPDFSort)
        {
            try
            {
                cancelTokenSource = new CancellationTokenSource();
                token = cancelTokenSource.Token;
                isManuallyCancellToken = false;

                InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                List<System.Threading.Tasks.Task> localTasks = new List<System.Threading.Tasks.Task>();

                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    List<System.Threading.Tasks.Task> stackTasks = new List<System.Threading.Tasks.Task>();
                    var filesToProjectfromKompas = new ConcurrentQueue<FileToProject>(filesToPDFSort.FilesToProjectfromKompas);

                    for (int i = 0; i < Convert.ToInt32(ConfigurationManager.AppSettings["MaxCountKompas"]) && i < filesToProjectfromKompas.Count / 10 + 1; i++)
                    {
                        stackTasks.Add(System.Threading.Tasks.Task.Run(() =>
                        {
                            Apps.Kompas kompas = new Apps.Kompas();
                            bool sectionReadiness;
                            FileToProject file;
                            bool b;
                            while (b = filesToProjectfromKompas.TryDequeue(out file) && !token.IsCancellationRequested)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    kompas.Quit();
                                    return;
                                }
                                DoPDFfromKompas(file.Path, file.OutputFileName, kompas);
                                lock (_lock)
                                {
                                    file.IsDone = true;
                                    infoOfProcess.CompleteFormsFiles++;
                                    sectionReadiness = CheckSectionReadiness(file);
                                }
                                if (sectionReadiness && !token.IsCancellationRequested)
                                {
                                    SectionToProject section = file.SectionToProject;
                                    localTasks.Add(System.Threading.Tasks.Task.Run(() => FormSection(section)/*, token*/));
                                }
                            }
                            System.Threading.Tasks.Task.Run(() => kompas.Quit());
                        }/*, token*/));
                    }
                    System.Threading.Tasks.Task.WaitAll(stackTasks.ToArray());
                }/*, token*/));

                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    List<System.Threading.Tasks.Task> stackTasks = new List<System.Threading.Tasks.Task>();
                    var filesToProjectfromWord = new ConcurrentQueue<FileToProject>(filesToPDFSort.FilesToProjectfromWord);
                    for (int i = 0; i < Convert.ToInt32(ConfigurationManager.AppSettings["MaxCountWord"]) && i < filesToProjectfromWord.Count / 8 + 1; i++)
                    {
                        stackTasks.Add(System.Threading.Tasks.Task.Run(() =>
                        {
                            dynamic word = null;
                            bool sectionReadiness;
                            FileToProject file;
                            bool b;
                            while (b = filesToProjectfromWord.TryDequeue(out file) && !token.IsCancellationRequested)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    Apps.QuitWord(ref word);
                                    return;
                                }
                                DoPDFfromWord(file.Path, file.OutputFileName, ref word);
                                lock (_lock)
                                {
                                    file.IsDone = true;
                                    infoOfProcess.CompleteFormsFiles++;
                                    sectionReadiness = CheckSectionReadiness(file);
                                }
                                if (sectionReadiness && !token.IsCancellationRequested)
                                {
                                    SectionToProject section = file.SectionToProject;
                                    localTasks.Add(System.Threading.Tasks.Task.Run(() => FormSection(section)/*, token*/));
                                }
                            }
                            System.Threading.Tasks.Task.Run(() => Apps.QuitWord(ref word));
                        }/*, token*/));
                    }
                    System.Threading.Tasks.Task.WaitAll(stackTasks.ToArray());
                }/*, token*/));

                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    List<System.Threading.Tasks.Task> stackTasks = new List<System.Threading.Tasks.Task>();
                    var filesToProjectfromExcel = new ConcurrentQueue<FileToProject>(filesToPDFSort.FilesToProjectfromExcel);
                    for (int i = 0; i < Convert.ToInt32(ConfigurationManager.AppSettings["MaxCountExcel"]) && i < filesToProjectfromExcel.Count / 30 + 1; i++)
                    {
                        stackTasks.Add(System.Threading.Tasks.Task.Run(() =>
                        {
                            dynamic excel = null;
                            bool sectionReadiness;
                            FileToProject file;
                            bool b;
                            while (b = filesToProjectfromExcel.TryDequeue(out file) && !token.IsCancellationRequested)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    Apps.QuitExcel(ref excel);
                                    return;
                                }
                                DoPDFfromExcel(file.Path, file.OutputFileName, ref excel);
                                lock (_lock)
                                {
                                    file.IsDone = true;
                                    infoOfProcess.CompleteFormsFiles++;
                                    sectionReadiness = CheckSectionReadiness(file);
                                }
                                if (sectionReadiness && !token.IsCancellationRequested)
                                {
                                    SectionToProject section = file.SectionToProject;
                                    localTasks.Add(System.Threading.Tasks.Task.Run(() => FormSection(section)/*, token*/));
                                }
                            }
                            System.Threading.Tasks.Task.Run(() => Apps.QuitExcel(ref excel));
                        }/*, token*/));
                    }
                    System.Threading.Tasks.Task.WaitAll(stackTasks.ToArray());
                }/*, token*/));

                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    List<System.Threading.Tasks.Task> stackTasks = new List<System.Threading.Tasks.Task>();
                    var filesToProjectfromAutoCAD = new ConcurrentQueue<FileToProject>(filesToPDFSort.FilesToProjectfromAutoCAD);
                    for (int i = 0; i < Convert.ToInt32(ConfigurationManager.AppSettings["MaxCountAutoCAD"]) && i < filesToProjectfromAutoCAD.Count / 10 + 1; i++)
                    {
                        stackTasks.Add(System.Threading.Tasks.Task.Run(() =>
                        {
                            dynamic autocad = null;
                            bool sectionReadiness;
                            FileToProject file;
                            bool b;
                            while (b = filesToProjectfromAutoCAD.TryDequeue(out file) && !token.IsCancellationRequested)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    autocad = Apps.QuitAutoCAD(autocad);
                                    return;
                                }
                                DoPDFfromAutoCAD(file.Path, file.OutputFileName, ref autocad);
                                lock (_lock)
                                {
                                    file.IsDone = true;
                                    infoOfProcess.CompleteFormsFiles++;
                                    sectionReadiness = CheckSectionReadiness(file);
                                }
                                if (sectionReadiness && !token.IsCancellationRequested)
                                {
                                    SectionToProject section = file.SectionToProject;
                                    localTasks.Add(System.Threading.Tasks.Task.Run(() => FormSection(section)/*, token*/));
                                }
                            }
                            System.Threading.Tasks.Task.Run(() => autocad = Apps.QuitAutoCAD(autocad));
                        }/*, token*/));
                    }
                    System.Threading.Tasks.Task.WaitAll(stackTasks.ToArray());
                }/*, token*/));

                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    List<System.Threading.Tasks.Task> stackTasks = new List<System.Threading.Tasks.Task>();
                    var filesToProjectfromNanoCAD = new ConcurrentQueue<FileToProject>(filesToPDFSort.FilesToProjectfromNanoCAD);
                    for (int i = 0; i < Convert.ToInt32(ConfigurationManager.AppSettings["MaxCountNanoCAD"]) && i < filesToProjectfromNanoCAD.Count / 10 + 1; i++)
                    {
                        stackTasks.Add(System.Threading.Tasks.Task.Run(() =>
                        {
                            dynamic nanocad = null;
                            bool sectionReadiness;
                            FileToProject file;
                            bool b;
                            while (b = filesToProjectfromNanoCAD.TryDequeue(out file) && !token.IsCancellationRequested)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    Apps.QuitNanoCAD(ref nanocad);
                                    return;
                                }
                                DoPDFfromNanoCAD(file.Path, file.OutputFileName, ref nanocad);
                                lock (_lock)
                                {
                                    file.IsDone = true;
                                    infoOfProcess.CompleteFormsFiles++;
                                    sectionReadiness = CheckSectionReadiness(file);
                                }
                                if (sectionReadiness && !token.IsCancellationRequested)
                                {
                                    SectionToProject section = file.SectionToProject;
                                    localTasks.Add(System.Threading.Tasks.Task.Run(() => FormSection(section)/*, token*/));
                                }
                            }
                            System.Threading.Tasks.Task.Run(() => Apps.QuitNanoCAD(ref nanocad));
                        }/*, token*/));
                    }
                    System.Threading.Tasks.Task.WaitAll(stackTasks.ToArray());
                }/*, token*/));

                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    bool sectionReadiness;
                    filesToPDFSort.FilesToProjectfromPDF.ForEachImmediate(file =>
                    {
                        if (token.IsCancellationRequested)
                            return;
                        DoPDFfromPDF(file.Path, file.OutputFileName);
                        lock (_lock)
                        {
                            file.IsDone = true;
                            infoOfProcess.CompleteFormsFiles++;
                            sectionReadiness = CheckSectionReadiness(file);
                        }
                        if (sectionReadiness && !token.IsCancellationRequested)
                        {
                            SectionToProject section = file.SectionToProject;
                            localTasks.Add(System.Threading.Tasks.Task.Run(() => FormSection(section)/*, token*/));
                        }
                    });
                }/*, token*/));


                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                System.Threading.Tasks.Task.WaitAll(localTasks.ToArray());

                ApplicationVisibility = false;

                if (!token.IsCancellationRequested)
                {
                    List<string> messeges = new List<string>();

                    var emptySectionsFromFile = new List<string>();
                    filesToPDFSort.GetAllSectionsToProject()?
                        .Where(section => section.FilesToProject.Count == 0)
                        .ToList()
                        .ForEach(section => emptySectionsFromFile.Add(section.Path));

                    var emptySectionsFromPDF = new List<string>();
                    filesToPDFSort.GetAllSectionsToProject()?
                        .Where(section => section.FilesToProject.Count > 0)
                        .Where(section => section.FilesToProject.All(file => file.MethodPDFFile == MethodPDFFile.DontPDF))
                        .ToList()
                        .ForEach(section =>
                        {
                            emptySectionsFromPDF.Add(section.Path);
                            tasks.Add(System.Threading.Tasks.Task.Run(() => FormSection(section)));
                        });


                    if (emptySectionsFromPDF.Count == 1)
                    {
                        messeges.Add($"Обратите внивание, что следующий раздел не содержит файлов для PDF альбома:{Environment.NewLine}{Environment.NewLine}      " +
                            String.Join($"{Environment.NewLine}      ", emptySectionsFromPDF) +
                            $"{Environment.NewLine + Environment.NewLine}      " +
                            $"поэтому для него был сформирован только ZIP архив.");
                    }
                    else if (emptySectionsFromPDF.Count > 1)
                    {
                        messeges.Add($"Обратите внивание, что следующие разделы не содержат файлов для PDF альбома:{Environment.NewLine}{Environment.NewLine}      " +
                            String.Join($"{Environment.NewLine}      ", emptySectionsFromPDF) +
                            $"{Environment.NewLine + Environment.NewLine}      " +
                            $"поэтому для них были сформированы только ZIP архивы.");
                    }

                    if (emptySectionsFromFile.Count == 1)
                    {
                        messeges.Add($"Обратите внивание, что следующий раздел не содержит файлов для проекта:{Environment.NewLine}{Environment.NewLine}      " +
                            String.Join($"{Environment.NewLine}      ", emptySectionsFromFile) +
                            $"{Environment.NewLine + Environment.NewLine}      " +
                            $"поэтому он не был включён в проект.");
                    }
                    else if (emptySectionsFromFile.Count > 1)
                    {
                        messeges.Add($"Обратите внивание, что следующие разделы не содержат файлов для проекта:{Environment.NewLine}{Environment.NewLine}      " +
                            String.Join($"{Environment.NewLine}      ", emptySectionsFromFile) +
                            $"{Environment.NewLine + Environment.NewLine}      " +
                            $"поэтому они не были включены в проект."); ;
                    }
                    if (messeges.Count > 0)
                        System.Windows.MessageBox.Show(String.Join($"{Environment.NewLine}{Environment.NewLine}", messeges));
                }
                else { }
            }
            catch (Exception ex)
            {
                Apps.QuitApps();
                if (!isManuallyCancellToken)
                    throw ex;
            }

        }

        private static void DoPDFfromPDF(string fileName, string outputFileName)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFileName));
                }
                File.Copy(fileName, outputFileName, true);
            }
            catch (Exception ex)
            {
                CancelToken();
                //throw new Exception($"При копировании файла {fileName}{Environment.NewLine}произошла ошибка: {Environment.NewLine} {ex.Message}.");
            }
        }

        private static void DoPDFfromWord(object fileName, object outputFileName, ref dynamic word)
        {
            try
            {
                Apps.RunWord(ref word);
                if (!Directory.Exists(Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFileName.ToString()));
                }
                dynamic doc = word.Documents.Open(fileName);
                object fileFormat = 17;//WdSaveFormat.wdFormatPDF
                doc.SaveAs(outputFileName, fileFormat);
                doc.Close(saveChanges);
                doc = null;
            }
            catch (Exception ex)
            {
                Apps.QuitWord(ref word);
                CancelToken();
                throw new Exception($"При формировании файла PDF {fileName}{Environment.NewLine}произошла ошибка: {Environment.NewLine} {ex.Message}.");
            }
        }

        private static void DoPDFfromExcel(string fileName, object outputFileName, ref dynamic excel)
        {
            try
            {
                Apps.RunExcel(ref excel);
                if (!Directory.Exists(Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFileName.ToString()));
                }
                dynamic doc = excel.Workbooks.OpenXML(fileName, oMissing, oMissing);
                doc.Activate();
                doc.ExportAsFixedFormat(0/*XlFixedFormatType.xlTypePDF*/, outputFileName, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                doc.Close(false, oMissing, oMissing);
                doc = null;
            }
            catch (Exception ex)
            {
                Apps.QuitExcel(ref excel);
                CancelToken();
                throw new Exception($"При формировании файла PDF {fileName}{Environment.NewLine}произошла ошибка: {Environment.NewLine} {ex.Message}.");
            }
        }

        private static void DoPDFfromKompas(string fileName, string outputFileName, Apps.Kompas kompas)
        {
            try
            {
                Apps.RunKompas(kompas);
                if (!Directory.Exists(Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFileName));
                }
                kompas.ConverterPDF.Convert(fileName, outputFileName, 1, false);
            }
            catch (Exception ex)
            {
                Apps.QuitKompas(kompas);
                CancelToken();
                throw new Exception($"При формировании файла PDF {fileName}{Environment.NewLine}произошла ошибка: {Environment.NewLine} {ex.Message}.");
            }
        }

        private static void DoPDFfromAutoCAD(string fileName, string outputFileName, ref dynamic autocad)
        {
            try
            {
                autocad = Apps.RunAutoCAD(autocad);
                if (!Directory.Exists(Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFileName.ToString()));
                }
                autocad.Documents.Open(Path.Combine(fileName));
                dynamic acadDoc = null;
                acadDoc = autocad.ActiveDocument;
                acadDoc.SetVariable("FILEDIA", 0);
                if (!isLoadDLLAutoCad)
                {
                    acadDoc.SendCommand($"TRUSTEDPATHS {Environment.CurrentDirectory}\\ \n");
                    ShowWindow((int)autocad.HWND, 0);
                    acadDoc.SendCommand($"_NETLOAD \"{Path.Combine(Environment.CurrentDirectory, "acPlt.dll")}\" \n");
                    ShowWindow((int)autocad.HWND, 0);
                    isLoadDLLAutoCad = true;
                }
                acadDoc.SendCommand($"PLOTPDF \"{outputFileName}\" \n");
                acadDoc.SetVariable("FILEDIA", 1);
                acadDoc.Close(false);
            }
            catch (Exception ex)
            {
                autocad = Apps.QuitAutoCAD(autocad);
                CancelToken();
                throw new Exception($"При формировании файла PDF {fileName}{Environment.NewLine}произошла ошибка: {Environment.NewLine} {ex.Message}.");
            }

        }

        private static void DoPDFfromNanoCAD(string fileName, string outputFileName, ref dynamic nanocad)
        {
            try
            {
                Apps.RunNanoCAD(ref nanocad);
                if (!Directory.Exists(Path.GetDirectoryName(outputFileName.ToString())))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFileName.ToString()));
                }
                nanocad.Documents.Open(Path.Combine(fileName));
                dynamic ncadDoc = null;
                ncadDoc = nanocad.ActiveDocument;
                ncadDoc.SetVariable("FILEDIA", 0);
                Dictionary<int, string> layouts = new Dictionary<int, string>();
                dynamic plot = ncadDoc.Plot;
                string printName = "Встроенный PDF-принтер";

                //В качестве примера распечатаем содержимое документа кроме модели
                foreach (dynamic layout in ncadDoc.Layouts)
                {
                    if (layout.Name == @"Model")
                        continue;
                    layouts.Add(layout.TabOrder, layout.Name);
                    double Size1;
                    double Size2;
                    layout.GetPaperSize(out Size1, out Size2);

                    List<string> devices = ((string[])layout.GetPlotDeviceNames()).ToList();
                    if (devices.Contains(printName))
                        layout.ConfigName = printName;
                    else throw new Exception($"Не найден принтер: \"{printName}\"");


                    //Найдём подходящий формат листа
                    bool successfulSearch = false;
                    string[] MediaNames = (string[])layout.GetCanonicalMediaNames();
                    string marker = $"{String.Format("{0:f2}", Math.Round(Size1, 2)).Replace(',', '.')} x {String.Format("{0:f2}", Math.Round(Size2, 2)).Replace(',', '.')}";
                    foreach (string media in MediaNames)
                    {
                        if (media.Contains(marker))
                        {
                            layout.CanonicalMediaName = media;
                            successfulSearch = true;
                            break;
                        }
                    }
                    if (!successfulSearch)
                    {
                        throw new Exception($"Для принтера \"{printName}\" не найден формат листа с размерами {marker}");
                    }

                    //Специфика nanoCAD: Отключим автозапуск программы чтения PDF по умолчанию, в противном случае после печати получившийся файл будет открыт.
                    dynamic plot_params = plot.CustomPlotSettings[layout];
                    plot_params.RunPDFApp = false;

                }
                layouts = layouts.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
                string guid = Guid.NewGuid().ToString();
                int i = 0;
                List<string> tempFilePaths = new List<string>();
                string tempFilePath;
                string directoryPDF = Path.GetDirectoryName(outputFileName);
                foreach (var layout in layouts.Values)
                {
                    i++;
                    var obj = new object[] { layout };
                    plot.SetLayoutsToPlot(obj);
                    tempFilePath = Path.Combine(directoryPDF, '!' + guid + i + ".pdf");
                    tempFilePaths.Add(tempFilePath);
                    plot.PlotToFile(tempFilePath);
                }
                CombinePDF(tempFilePaths, outputFileName);
                tempFilePaths.ForEach(p => File.Delete(p));
                ncadDoc.SetVariable("FILEDIA", 1);
                ncadDoc.Close(false);
            }
            catch (Exception ex)
            {
                Apps.QuitNanoCAD(ref nanocad);
                CancelToken();
                throw new Exception($"При формировании файла PDF {fileName}{Environment.NewLine}произошла ошибка: {Environment.NewLine} {ex.Message}.");
            }

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
                CancelToken();
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
                                }, token));
                            }
                            else
                            {
                                CancelToken();
                                throw new Exception($"Для архива {Path.GetFileName(outputSectionPath.Key)} не удалось найти файлы:{Environment.NewLine}      "
                                    + String.Join(
                                        $"{Environment.NewLine}      ", sectionToProject.FilesToProject
                                        .Where(file => !File.Exists(file.Path))
                                        .Select(file => file.Path)));
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
                                            .Select(file => file.OutputFileName).ToList(), outputSectionPath.Key);
                                    }
                                    catch (AggregateException ae)
                                    {
                                        CancelToken();
                                        throw ae;
                                    }

                                }, token));
                            }
                            else
                            {
                                CancelToken();
                                throw new Exception($"Для альбома {Path.GetFileName(outputSectionPath.Key)} не удалось найти PDF файлы:{Environment.NewLine}      "
                                    + String.Join(
                                        $"{Environment.NewLine}      ", sectionToProject.FilesToProject
                                        .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                                        .Where(file => !File.Exists(file.OutputFileName))
                                        .Select(file => file.OutputFileName)));
                            }
                            break;
                        }
                    default:
                        {
                            CancelToken();
                            throw new Exception($"Неизвестный метод формирования секции: {outputSectionPath.Value}.");
                        }
                }

            }
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            sectionToProject.IsDone = true;
            infoOfProcess.CompleteFormsSections++;
        }

        #region DefaultFields
        private static object oMissing = System.Reflection.Missing.Value;
        private static object saveChanges = 0; //WdSaveOptions.wdDoNotSaveChanges
        #endregion

        public static void CancelToken(bool isManually = false)
        {
            cancelTokenSource.Cancel();
            isManuallyCancellToken = isManually;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(int hWnd, int nCmdShow);
    }
}
