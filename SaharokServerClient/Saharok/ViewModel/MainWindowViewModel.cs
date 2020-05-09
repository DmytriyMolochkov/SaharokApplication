
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
using Saharok.Model.Client;
using System.Runtime.Remoting.Channels;
using Microsoft.Office.Interop.Word;

namespace Saharok.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            ClientObject1 = new ClientObject("109.68.215.3", 8888, 1); /*109.68.215.3*/ /*127.0.0.1*/
            ClientObject1.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>  IsServer1Connect = ClientObject1.IsServerConnect;
            ClientObject1.PropertyChanged += (object sender, PropertyChangedEventArgs e) => IsServer1Tip = $"Сервер №1 {(ClientObject1.IsServerConnect ? "подключён" : "отключён")}.";
            ClientObject1.Connect(true);
            ClientObject2 = new ClientObject("109.68.215.3", 8889, 2);
            ClientObject2.PropertyChanged += (object sender, PropertyChangedEventArgs e) => IsServer2Connect = ClientObject2.IsServerConnect;
            ClientObject1.PropertyChanged += (object sender, PropertyChangedEventArgs e) => IsServer2Tip = $"Сервер №2 {(ClientObject2.IsServerConnect ? "подключён" : "отключён")}.";
            ClientObject2.Connect(true);
            LoadProjectEvent += LoadProject;
            CloseProjectEvent += CloseProject;
            ProcessWorksEvent += ProcessWorks;
            ProcessOffEvent += ProcessOff;
            ExceptionEvent += ExceptionBorder;
            OnCloseProjectEvent();
            InfoOfProcess.GetInstance().PropertyChanged += (object sender, PropertyChangedEventArgs e) => UpDateFormedProgressBarText();
            ClickLoadProject = new Command(arg => LoadProject(), arg => LoadProject_CanExecute());
            ClickCloseProject = new Command(arg => CloseProject(), arg => CloseProject_CanExecute());
            ClickOpenCreateProjectWindow = new Command(arg => OpenCreateProjectWindow(), arg => OpenCreateProjectWindow_CanExecute());
            ClickOpenReferenceWindow = new Command(arg => OpenCreateReferenceWindow(), arg => OpenReferenceWindow_CanExecute());
            ClickChooseFolderOpenFileDialog = new Command(arg => ChooseFolderOpenFileDialog());
            ClickCreateProject = new Command(arg => CreateProject(PathProject, NameProject, CodeProject));
            ClickFormProject = new Command(arg =>
            {
                Thread threadFormOnServerProject = new Thread(() => FormOnServerProject(arg));
                threadFormOnServerProject.Priority = ThreadPriority.Highest;
                threadFormOnServerProject.IsBackground = true;
                threadFormOnServerProject.Start();
            }
            , arg => FormOnServerProject_CanExecute());
        }

        private static object syncRoot = new Object();
        ClientObject ClientObject1;
        ClientObject ClientObject2;


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

        public bool IsProcessed { get; private set; } = false;

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

        private bool isServer1Connect;
        public bool IsServer1Connect
        {
            get => isServer1Connect;
            set
            {
                isServer1Connect = value;
                OnPropertyChanged(nameof(IsServer1Connect));
            }
        }

        private bool isServer2Connect;
        public bool IsServer2Connect
        {
            get => isServer2Connect;
            set
            {
                isServer2Connect = value;
                OnPropertyChanged(nameof(IsServer2Connect));
            }
        }

        private string isServer1Tip;
        public string IsServer1Tip
        {
            get => isServer1Tip;
            set
            {
                isServer1Tip = value;
                OnPropertyChanged(nameof(IsServer1Tip));
            }
        }

        private string isServer2Tip;
        public string IsServer2Tip
        {
            get => isServer2Tip;
            set
            {
                isServer2Tip = value;
                OnPropertyChanged(nameof(IsServer2Tip));
            }
        }

        public ICommand ClickLoadProject { get; set; }
        public ICommand ClickCloseProject { get; set; }
        public ICommand ClickOpenCreateProjectWindow { get; set; }
        public ICommand ClickOpenReferenceWindow { get; set; }
        public ICommand ClickChooseFolderOpenFileDialog { get; set; }
        public ICommand ClickCreateProject { get; set; }
        public ICommand ClickFormProject { get; set; }
        public ICommand ClickCombinePDF { get; set; }
        public ICommand ClickFormOnServerTypeDocumentation { get; set; }
        public ICommand ClickFormOnServerSection { get; set; }
        public ICommand ClickFormPDFToPagesFile { get; set; }

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
            createNewProjectWindow.ShowDialog();
        }

        private void CreateProject(string pathProject, string nameProject, string codeProject)
        {

            CreateProjectClass.CreateProjectPath(pathProject, nameProject, codeProject);
            LoadProject(Path.Combine(pathProject, nameProject, nameProject + ".srk"));
            createNewProjectWindow.Close();
        }

        private bool OpenCreateProjectWindow_CanExecute()
        {
            if (MyProject != null || IsProcessed)
                return false;
            else
                return true;
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

        CreateReferenceWindow createReferenceWindow { get; set; }
        private void OpenCreateReferenceWindow()
        {
            createReferenceWindow = new CreateReferenceWindow();
            createReferenceWindow.Owner = App.window;
            createReferenceWindow.DataContext = ((MainWindowViewModel)App.window.DataContext);
            createReferenceWindow.ShowDialog();
        }

        private bool OpenReferenceWindow_CanExecute()
        {
            return true;
        }

        private void FormOnServerProject(object objectToProject)
        {
            try
            {
                OnProcessWorksEvent();

                if (objectToProject is Project)
                {
                    List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                    tasks.Add(System.Threading.Tasks.Task.Run(() =>
                    {
                        DirectoryMethods.ClearFolder(Path.Combine(MyProject.Path, "Готовый проект\\На сервер"));
                        DirectoryMethods.ClearFolder(Path.Combine(MyProject.Path, "Готовый проект\\На отправку"));
                    }));
                    tasks.Add(System.Threading.Tasks.Task.Run(() => DirectoryMethods.ClearFolder(Path.Combine(MyProject.Path, "PDF постранично"))));

                    System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                }

                try
                {
                    FormProject.CreateProject(objectToProject, ClientObject1);
                }
                catch(ServerException ex)
                {
                    try
                    {
                        //FormProject.CreateProject(objectToProject, ClientObject2);
                        throw ex;
                    }
                    catch (ServerException e)
                    {
                        throw new AggregateException(new Exception[] { ex, e });
                    }
                }

                OnProcessOffEvent();
            }
            catch (Exception ex)
            {
                ErrorHandling(ex);
            }
            finally
            {
                InfoOfProcess.RefreshInstance();
                InfoOfProcess.GetInstance().PropertyChanged += (object sender, PropertyChangedEventArgs e) => UpDateFormedProgressBarText();
            }
        }

        private bool FormOnServerProject_CanExecute()
        {
            if (MyProject == null || IsProcessed)
                return false;
            else
                return true;
        }

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
            FormedFilesText = "Сконвертировано PDF файлов: " + infoOfProcess.CompleteFormsFiles + " / " + infoOfProcess.TotalFormsFiles;
            FormedSectionsText = "Сформировано разделов: " + infoOfProcess.CompleteFormsSections + " / " + infoOfProcess.TotalFormsSections;
        }

        void ProcessWorks(object source, EventArgs arg)
        {
            IsProcessed = true;
            TimerAnimation();
            BootAnimation();
            
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();

            infoOfProcess.CompleteFormsFiles = 0;
            infoOfProcess.CompleteFormsSections = 0;
            infoOfProcess.TotalFormsSections = 0;
            infoOfProcess.TotalFormsFiles = 0;

            UpDateFormedProgressBarText();
            StatusBarText = "Формируется проект";
            StatusBarColor = "ProcessWorks";
            StatusBarTimerText = "00:00";
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
                await System.Threading.Tasks.Task.Delay(50);
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
                await System.Threading.Tasks.Task.Delay(50);
                if (!IsProcessed)
                {
                    Timer.Enabled = false;
                }
            }
            Timer.Enabled = false;

            void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
            {
                StatusBarTimerText = string.Format("{0:mm:ss}", ElapsedDateTime.AddSeconds(i++));
            }
        }

        private void ErrorHandling(Exception ex)
        {
            OnExceptionEvent();
            IEnumerable<string> messages = GetErrorMessages(ex);

            System.Windows.MessageBox.Show(String.Join(Environment.NewLine + Environment.NewLine, messages), $"Упс... {GetRandomSmile()} что - то пошло не так    ");
        }
        private IEnumerable<string> GetErrorMessages(Exception ex)
        {
            if (!(ex is AggregateException))
                return new string[] { ex.Message };

            List<string> messages = new List<string>();

            foreach (var e in ((AggregateException)ex).InnerExceptions)
            {
                if(e is AggregateException)
                    messages.AddRange(GetErrorMessages(e));
                else
                    messages.Add(e.Message);
            }

            return messages;
        }

        static Random rnd = new Random();
        private static string GetRandomSmile()
        {
            return Smiles[rnd.Next(Smiles.Length)];
        }
        private static string[] Smiles = {
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}