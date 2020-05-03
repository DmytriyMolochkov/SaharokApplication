
using System.Windows;
using System.Windows.Input;
using Saharok.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saharok.View;
using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Globalization;
using System.Collections.Concurrent;
using ObjectsProjectClient;

namespace Saharok.ViewModel
{
    class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel()
        {
            LoadProjectEvent += LoadProject;
            CloseProjectEvent += CloseProject;
            ProcessWorksEvent += ProcessWorks;
            ProcessOffEvent += ProcessOff;
            ExceptionEvent += ExceptionBorder;
            OnCloseProjectEvent();
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            InfoOfProcess.PropertyChanged += (object sender, PropertyChangedEventArgs e) => UpDateFormedProgressBarText();
            ClickLoadProject = new Command(arg => LoadProject(), arg => LoadProject_CanExecute());
            ClickCloseProject = new Command(arg => CloseProject(), arg => CloseProject_CanExecute());
            ClickOpenCreateProjectWindow = new Command(arg => OpenCreateProjectWindow(), arg => OpenCreateProjectWindow_CanExecute());
            ClickChooseFolderOpenFileDialog = new Command(arg => ChooseFolderOpenFileDialog());
            ClickCreateProject = new Command(arg => CreateProject(PathProject, NameProject, CodeProject));
            ClickFormProject = new Command(arg =>
            {
                if (arg == null)
                {
                    Thread threadFormOnServerProject = new Thread(() => FormOnServerProject());
                    threadFormOnServerProject.Priority = ThreadPriority.Highest;
                    threadFormOnServerProject.IsBackground = true;
                    threadFormOnServerProject.Start();
                }
                if (arg is TypeDocumentation)
                {
                    Thread threadFormOnServerProject = new Thread(() => FormOnServerProject(arg as TypeDocumentation));
                    threadFormOnServerProject.Priority = ThreadPriority.Highest;
                    threadFormOnServerProject.IsBackground = true;
                    threadFormOnServerProject.Start();
                }
                if (arg is Section)
                {
                    Thread threadFormOnServerProject = new Thread(() => FormOnServerProject(arg as Section));
                    threadFormOnServerProject.Priority = ThreadPriority.Highest;
                    threadFormOnServerProject.IsBackground = true;
                    threadFormOnServerProject.Start();
                }
                if (arg is FileSection)
                {
                    Thread threadFormOnServerProject = new Thread(() => FormOnServerProject(arg as FileSection));
                    threadFormOnServerProject.Priority = ThreadPriority.Highest;
                    threadFormOnServerProject.IsBackground = true;
                    threadFormOnServerProject.Start();
                }
            }
            , arg => FormOnServerProject_CanExecute());
        }

        private static object syncRoot = new Object();

        private Project myProject;
        public Project MyProject
        {
            get => myProject;
            set
            {
                myProject = value;
                OnPropertyChanged(nameof(MyProject));
            }
        }

        private string statusBarColor;
        public string StatusBarColor
        {
            get => statusBarColor;
            set
            {
                statusBarColor = value;
                OnPropertyChanged(nameof(StatusBarColor));
            }
        }
        private string statusBarText;
        public string StatusBarText
        {
            get => statusBarText;
            set
            {
                statusBarText = value;
                OnPropertyChanged(nameof(StatusBarText));
            }
        }
        private string statusBarTimerText;
        public string StatusBarTimerText
        {
            get => statusBarTimerText;
            set
            {
                statusBarTimerText = value;
                OnPropertyChanged(nameof(StatusBarTimerText));
            }
        }
        private string formedFilesText;
        public string FormedFilesText
        {
            get => formedFilesText;
            set
            {
                formedFilesText = value;
                OnPropertyChanged(nameof(FormedFilesText));
            }
        }

        private string formedSectionsText;
        public string FormedSectionsText
        {
            get => formedSectionsText;
            set
            {
                formedSectionsText = value;
                OnPropertyChanged(nameof(FormedSectionsText));
            }
        }

        public bool IsProcessed { get; private set; } = false;

        public void LoadProject()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text documents (*.srk)|*.srk|All files (*.*)|*.*";
            dialog.FilterIndex = 1;
            dialog.ShowDialog();

            if (!string.IsNullOrWhiteSpace(dialog.FileName))
            {
                string fileName = dialog.FileName;
                using (StreamReader stream = new StreamReader(fileName, Encoding.Default))
                {
                    string name = stream.ReadLine();
                    string codeProject = stream.ReadLine();
                    Action<Action> Invoke = App.Current.Dispatcher.Invoke;
                    MyProject = new Project(System.IO.Path.GetDirectoryName(fileName), name, codeProject, Invoke);
                    App.window.MyTabControl.Visibility = Visibility.Visible;
                }
            }
            OnLoadProjectEvent();
        }

        public void LoadProject(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                using (StreamReader stream = new StreamReader(fileName, Encoding.Default))
                {
                    string name = stream.ReadLine();
                    string codeProject = stream.ReadLine();
                    Action<Action> Invoke = App.Current.Dispatcher.Invoke;
                    MyProject = new Project(System.IO.Path.GetDirectoryName(fileName), name, codeProject, Invoke);
                    App.window.MyTabControl.Visibility = Visibility.Visible; // костыль для костыля в CloseProject()
                }
            }
            OnLoadProjectEvent();
        }

        public bool LoadProject_CanExecute()
        {
            if (MyProject != null || IsProcessed)
                return false;
            else
                return true;
        }

        public void CloseProject()
        {
            MyProject = null;
            App.window.MyTabControl.Visibility = Visibility.Hidden; // костыль чтобы убрать полоску фокуса у дерева файлов при закрытии проекта
            OnCloseProjectEvent();
        }


        public bool CloseProject_CanExecute()
        {
            if (MyProject == null || IsProcessed)
                return false;
            else
                return true;
        }

        CreateNewProjectWindow createNewProjectWindow { get; set; }
        private void OpenCreateProjectWindow()
        {
            createNewProjectWindow = new CreateNewProjectWindow();
            createNewProjectWindow.Owner = App.window;
            createNewProjectWindow.DataContext = ((MainWindowViewModel)App.window.DataContext);
            createNewProjectWindow.Show();
        }

        private bool OpenCreateProjectWindow_CanExecute()
        {
            if (MyProject != null || IsProcessed)
                return false;
            else
                return true;
        }

        private string nameProject;
        public string NameProject
        {
            get => nameProject;
            set
            {
                nameProject = value;
                OnPropertyChanged(nameof(NameProject));
            }
        }

        private string pathProject;
        public string PathProject
        {
            get => pathProject;
            set
            {
                pathProject = value;
                OnPropertyChanged(nameof(PathProject));
            }
        }

        private string codeProject;
        public string CodeProject
        {
            get => codeProject;
            set
            {
                codeProject = value;
                OnPropertyChanged(nameof(CodeProject));
            }
        }

        private void ChooseFolderOpenFileDialog()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;


            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PathProject = dialog.FileName;
            }
        }

        private void CreateProject(string pathProject, string nameProject, string codeProject)
        {

            CreateProjectClass.CreateProjectPath(pathProject, nameProject, codeProject);
            LoadProject(Path.Combine(pathProject, nameProject, nameProject + ".srk"));
            createNewProjectWindow.Close();
        }

        private void ErrorHandling(AggregateException ae)
        {
            List<string> messeges = new List<string>();
            foreach (var ex in ae.InnerExceptions)
            {
                if (ex is AggregateException)
                {
                    foreach (var e in ((AggregateException)ex).InnerExceptions)
                    {
                        messeges.Add(e.Message);
                    }
                }
                else
                {
                    messeges.Add(ex.Message);
                }
            }
            //System.Windows.MessageBox.Show($"Во время работы программы произошли следующие ошибки:{Environment.NewLine}" +
            //    $"{Environment.NewLine}      " +
            //    String.Join($"{Environment.NewLine}      ", messeges), $"Упс... {GetRandomSmile()} что - то пошло не так    ");
            System.Windows.MessageBox.Show(String.Join($"{Environment.NewLine}", messeges), $"Упс... {GetRandomSmile()} что - то пошло не так    ");
            OnExceptionEvent();
        }

        static Random rnd = new Random();
        private static string GetRandomSmile()
        {
            string[] smiles = {
                @"¯\_(ツ)_ /¯",

                @"̿ ̿ ̿ ̿ ̿'̿'\̵͇̿̿\з=(͠° ͟ʖ ͡°)=ε/̵͇̿̿/'̿̿ ̿ ̿ ̿ ̿ ̿ ",

                @"(╯°□°)╯┻┻",

                @"(╯°□°）╯︵(.o.)",

                @"╰(°益°)╯",

                @"(˘▽˘)っ♨",

                @"｀、ヽ｀ヽ｀、ヽ(ノ＞＜)ノ ｀、ヽ｀☂ヽ｀、ヽ",

                @"(ง ͠° ͟ل͜ ͡°)ง",

                @"◉_◉",

                @"(☞ﾟヮﾟ)☞",

                @"༼ຈل͜ຈ༽ﾉ",

                @"†(•̪●)†",

                @"(╬ Ò﹏Ó)",

                @"٩(╬ʘ益ʘ╬)۶",

                @"(ิ_ิ)?",

                @"(҂｀ﾛ´)︻/̵͇̿̿/'̿̿ ̿ ̿ ̿ ̿ ̿ ",

                @"(╯°益°)╯彡┻━┻",

                @"┻━┻ ︵ヽ(`Д´)ﾉ︵ ┻━┻",

                @"╚(ಠ_ಠ)=┐",

                @"(⌐■_■)>¸,ø¤º°`°º¤ø,¸¸",

                @"(ಥ﹏ಥ)",

                @"＼(º □ º l|l)/",

                @"(╥﹏╥)"};

            return smiles[rnd.Next(smiles.Length)];
        }

        private void FormOnServerProject()
        {
            try
            {
                OnProcessWorksEvent();

                List<Task> tasks1 = new List<Task>();
                tasks1.Add(Task.Run(() =>
                {
                    DirectoryMethods.ClearFolder(Path.Combine(MyProject.Path, "Готовый проект\\На сервер"));
                    DirectoryMethods.ClearFolder(Path.Combine(MyProject.Path, "Готовый проект\\На отправку"));
                }));
                tasks1.Add(Task.Run(() => DirectoryMethods.ClearFolder(Path.Combine(MyProject.Path, "PDF постранично"))));

                try
                {
                    Task.WaitAll(tasks1.ToArray());
                }
                catch (AggregateException ae)
                {
                    throw ae;
                }

                FormProject.CreateProject(MyProject);

                OnProcessOffEvent();
            }
            catch (AggregateException ae)
            {
                ErrorHandling(ae);
            }
        }

        private void FormOnServerProject(TypeDocumentation typeDocumentation)
        {
            try
            {
                OnProcessWorksEvent();

                FormProject.CreateProject(typeDocumentation);

                OnProcessOffEvent();
            }
            catch (AggregateException ae)
            {
                ErrorHandling(ae);
            }
        }
        private void FormOnServerProject(Section section)
        {
            try
            {
                OnProcessWorksEvent();

                FormProject.CreateProject(section);

                OnProcessOffEvent();
            }
            catch (AggregateException ae)
            {
                ErrorHandling(ae);
            }
        }

        private bool FormOnServerProject_CanExecute()
        {
            if (MyProject == null || IsProcessed)
                return false;
            else
                return true;
        }

        private void FormOnServerProject(FileSection fileSection)
        {
            try
            {
                OnProcessWorksEvent();
                FormProject.CreateProject(fileSection);
                OnProcessOffEvent();
            }
            catch (AggregateException ae)
            {
                ErrorHandling(ae);
            }
        }

        //private bool FormPDFToPagesFile_CanExecute()
        //{
        //    if (MyProject == null || IsProcessed)
        //        return false;
        //    else
        //        return true;
        //}

        #region Commands

        /// <summary>
        /// Get or set ClickCommand.
        /// </summary>
        public ICommand ClickLoadProject { get; set; }
        public ICommand ClickCloseProject { get; set; }
        public ICommand ClickOpenCreateProjectWindow { get; set; }
        public ICommand ClickChooseFolderOpenFileDialog { get; set; }
        public ICommand ClickCreateProject { get; set; }
        public ICommand ClickFormProject { get; set; }
        public ICommand ClickCombinePDF { get; set; }
        public ICommand ClickFormOnServerTypeDocumentation { get; set; }
        public ICommand ClickFormOnServerSection { get; set; }
        public ICommand ClickFormPDFToPagesFile { get; set; }



        public ICommand ClickMethod { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Click method.
        /// </summary>
        private void Method()
        {

        }

        #endregion

        public event EventHandler LoadProjectEvent;
        public event EventHandler CloseProjectEvent;

        public void OnLoadProjectEvent()
        {
            if (LoadProjectEvent != null)
                LoadProjectEvent(this, EventArgs.Empty);
        }

        public void OnCloseProjectEvent()
        {
            if (CloseProjectEvent != null)
                CloseProjectEvent(this, EventArgs.Empty);
        }

        void LoadProject(object source, EventArgs arg)
        {
            StatusBarColor = "Ready";
            StatusBarText = "Проект загружен";
            FormedFilesText = "";
            FormedSectionsText = "";
            StatusBarTimerText = "";
        }

        void CloseProject(object source, EventArgs arg)
        {
            StatusBarColor = "NoProject";
            StatusBarText = "Нет проекта";
            FormedFilesText = "";
            FormedSectionsText = "";
            StatusBarTimerText = "";
        }

        public event EventHandler ProcessWorksEvent;
        public event EventHandler ProcessOffEvent;

        public void OnProcessWorksEvent()
        {
            if (ProcessWorksEvent != null)
                ProcessWorksEvent(this, EventArgs.Empty);
        }

        public void OnProcessOffEvent()
        {
            if (ProcessOffEvent != null)
                ProcessOffEvent(this, EventArgs.Empty);
        }

        public void UpDateFormedProgressBarText()
        {
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            lock (syncRoot)
            {
                FormedFilesText = "Сконвертировано PDF файлов: " + infoOfProcess.CompleteFormsFiles + " / " + infoOfProcess.TotalFormsFiles;
                FormedSectionsText = "Сформировано разделов: " + infoOfProcess.CompleteFormsSections + " / " + infoOfProcess.TotalFormsSections;
            }

        }

        void ProcessWorks(object source, EventArgs arg)
        {
            Task.Run(() => TimerAnimation());
            Task.Run(() => BootAnimation());
            
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            UpDateFormedProgressBarText();
            infoOfProcess.CompleteFormsFiles = 0;
            infoOfProcess.CompleteFormsSections = 0;
            IsProcessed = true;
            StatusBarText = "Формируется проект";
            StatusBarColor = "ProcessWorks";
        }

        void ProcessOff(object source, EventArgs arg)
        {
            IsProcessed = false;
            OnLoadProjectEvent();
        }

        public event EventHandler ExceptionEvent;

        public void OnExceptionEvent()
        {
            if (ExceptionEvent != null)
                ExceptionEvent(this, EventArgs.Empty);
        }

        void ExceptionBorder(object source, EventArgs arg)
        {
            IsProcessed = false;
            StatusBarColor = "Exception";
            StatusBarText = "Произошла ошибка";
        }
        async void BootAnimation()
        {
            int i = 0;
            var Timer = new System.Timers.Timer();
            Timer.Interval = 1000;
            Timer.Elapsed += OnTimedEvent;
            Timer.AutoReset = true;
            Timer.Enabled = true;

            while (IsProcessed)
            {
                await Task.Delay(50);
                if (!IsProcessed)
                {
                    Timer.Enabled = false;
                }
            }
            Timer.Enabled = false;

            void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
            {
                if (i < 3)
                {
                    StatusBarText += '.';
                    i++;
                }
                else
                {
                    StatusBarText = StatusBarText.Remove(StatusBarText.Length - i);
                    i = 0;
                }
            }
        }

        async void TimerAnimation()
        {
            int i = 0;
            DateTime ElapsedDateTime = new DateTime();
            var Timer = new System.Timers.Timer();
            Timer.Interval = 1000;
            Timer.Elapsed += OnTimedEvent;
            Timer.AutoReset = true;
            Timer.Enabled = true;

            while (IsProcessed)
            {
                await Task.Delay(50);
                if (!IsProcessed)
                    Timer.Enabled = false;
            }
            Timer.Enabled = false;

            void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
            {
                StatusBarTimerText = string.Format("{0:mm:ss}", ElapsedDateTime.AddSeconds(i++));
            }
        }
    }
}