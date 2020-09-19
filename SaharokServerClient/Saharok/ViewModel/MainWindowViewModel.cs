
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
using Saharok.Model.Client;
using System.Runtime.Remoting.Channels;
using static Saharok.Model.FormProject;
using System.Configuration;

namespace Saharok.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            CheckConfigFile();
            IsProcessed = false;
            ApplicationVisibility = Model.FormProject.ApplicationVisibility;
            PathDirectoryProject = "";
            ClientObject1 = new ClientObject("127.0.0.1", 9111, 1); /*109.68.215.3*/ /*127.0.0.1*/
            ClientObject1.PropertyChanged += (object sender, PropertyChangedEventArgs e) => IsServer1Connect = ClientObject1.IsServerConnect;
            ClientObject1.PropertyChanged += (object sender, PropertyChangedEventArgs e) => IsServer1Tip = $"Сервер №{ClientObject1.Number} {(ClientObject1.IsServerConnect ? "подключён" : "отключён")}.";
            ClientObject1.Connect(true);
            ClientObject2 = new ClientObject("127.0.0.1", 9112, 2); /*5.23.54.220*/
            ClientObject2.PropertyChanged += (object sender, PropertyChangedEventArgs e) => IsServer2Connect = ClientObject2.IsServerConnect;
            ClientObject2.PropertyChanged += (object sender, PropertyChangedEventArgs e) => IsServer2Tip = $"Сервер №{ClientObject2.Number} {(ClientObject2.IsServerConnect ? "подключён" : "отключён")}.";
            //ClientObject2.Connect(true);
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
            ClickCreateProject = new Command(arg => CreateProject(Path.Combine(PathDirectoryProject, NameProject), NameProject, CodeProject));
            ClickOpenProcessedPopup = new Command(arg => OpenProcessedPopup(), arg => OpenProcessedPopup_CanExecute());
            ClickChangeApplicationVisibility = new Command(arg => Task.Run(() => ChangeApplicationVisibility()), arg => ChangeApplicationVisibility_CanExecute());
            ClickAbortProcess = new Command(arg => Task.Run(() => AbortProcess()));
            ClickFormProject = new Command(arg => Task.Run(() => FormOnServerProject(arg)), arg => FormOnServerProject_CanExecute());
        }

        ClientObject ClientObject1;
        ClientObject ClientObject2;
        FoldersConfigInfo FoldersConfigInfo { get; set; }
        SectionNameTemplate SectionNameTemplate { get; set; }

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

        private string pathDirectoryProject;
        public string PathDirectoryProject
        {
            get => pathDirectoryProject;
            set
            {
                pathDirectoryProject = value;
                OnPropertyChanged(nameof(PathDirectoryProject));
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

        private bool isProcessed;
        public bool IsProcessed
        {
            get => isProcessed;
            set
            {
                isProcessed = value;
                OnPropertyChanged(nameof(IsProcessed));
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

        private bool applicationVisibility;
        public bool ApplicationVisibility
        {
            get => applicationVisibility;
            set
            {
                applicationVisibility = value;
                OnPropertyChanged(nameof(ApplicationVisibility));
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
        public ICommand ClickOpenProcessedPopup { get; set; }
        public ICommand ClickChangeApplicationVisibility { get; set; }
        public ICommand ClickAbortProcess { get; set; }

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

        public void LoadProject(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                using (StreamReader stream = new StreamReader(filePath, Encoding.Default))
                {
                    string name = stream.ReadLine();
                    string codeProject = stream.ReadLine();
                    Action<Action> Invoke = App.Current.Dispatcher.Invoke;
                    MyProject = new Project(System.IO.Path.GetDirectoryName(filePath), name, codeProject, Invoke);
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

        private void CreateProject(string fullPath, string name, string codeProject)
        {
            try
            {
                CreateProjectClass.CreateProjectDirectory(fullPath, name, codeProject, FoldersConfigInfo);
                LoadProject(Path.Combine(fullPath, name + ".srk"));
                createNewProjectWindow.Close();
            }
            catch (Exception ex)
            {
                ErrorHandling(ex);
            }
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
                PathDirectoryProject = dialog.FileName;
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
                    List<string> foldersForClear = new List<string>();
                    foldersForClear.AddRange(MyProject.FoldersConfigInfo.OutputFilesPDFDirectories);
                    foldersForClear.AddRange(MyProject.FoldersConfigInfo.OutputFilesZIPDirectories);
                    foldersForClear.Add(MyProject.FoldersConfigInfo.OutputPageByPagePDF);
                    foldersForClear.Distinct().ToList().ForEach(path => DirectoryMethods.ClearFolder(Path.Combine(MyProject.Path, path)));
                }
                
                try
                {
                    FormProject.CreateProject(objectToProject, ClientObject1);
                }
                catch (ServerException ex)
                {
                    try
                    {
                        FormProject.CreateProject(objectToProject, ClientObject2);
                    }
                    catch (ServerException e)
                    {
                        throw new AggregateException(new Exception[] { ex, e });
                    }
                }

                OnProcessOffEvent();

                App.window.Dispatcher.Invoke(() => App.window.ProcessedPopup.IsOpen = false);
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

        private void OpenProcessedPopup()
        {
            App.window.ProcessedPopup.IsOpen = true;
        }

        private bool OpenProcessedPopup_CanExecute()
        {
            if (IsProcessed)
                return true;
            else
                return false;
        }

        static bool ChangeApplicationVisibilityIsProcess;
        private void ChangeApplicationVisibility()
        {
            ChangeApplicationVisibilityIsProcess = true;
            Model.FormProject.Apps.ChangeApplicationVisibility();
            ApplicationVisibility = Model.FormProject.ApplicationVisibility;
            ChangeApplicationVisibilityIsProcess = false;
        }

        private bool ChangeApplicationVisibility_CanExecute()
        {
            if (ChangeApplicationVisibilityIsProcess)
                return false;
            else
                return true;
        }

        public void AbortProcess()
        {
            Model.FormProject.CancelToken(true);
            StatusBarText = StatusBarText.Replace("Формируется проект", "Отменяется процесс");
            while (IsProcessed)
            {
                Thread.Sleep(500);
            }
        }

        public void CloseConnection()
        {
            ClientObject1.client.Close();
            ClientObject2.client.Close();
            ClientObject1.Disconnect();
            ClientObject2.Disconnect();
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
            StatusBarColor = "ProcessWorks";
        }

        void ProcessOff(object source, EventArgs arg)
        {
            IsProcessed = false;
            OnLoadProjectEvent();
            ApplicationVisibility = false;
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
            StatusBarText = "Формируется проект";
            int i = 0;
            var Timer = new System.Timers.Timer();
            Timer.Interval = 1000;
            Timer.Elapsed += OnTimedEvent;
            Timer.Start();
            PropertyChanged += OnOffTimerEvent;

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
            void OnOffTimerEvent(Object source, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(IsProcessed) && IsProcessed == false)
                {
                    Timer.Enabled = false;
                    PropertyChanged -= OnOffTimerEvent;
                }
            }
        }

        void TimerAnimation()
        {
            StatusBarTimerText = "00:00";
            DateTime StartDateTime = DateTime.Now;
            DateTime ElapsedDateTime = new DateTime();
            StartDateTime = DateTime.Now; ;
            var Timer = new System.Timers.Timer();
            Timer.Interval = 100;
            Timer.Elapsed += OnTimedEvent;
            Timer.Start();
            PropertyChanged += OnOffTimerEvent;

            void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
            {
                StatusBarTimerText = string.Format("{0:mm:ss}", ElapsedDateTime.AddTicks(DateTime.Now.Ticks - StartDateTime.Ticks));
            }
            void OnOffTimerEvent(Object source, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(IsProcessed) && IsProcessed == false)
                {
                    Timer.Enabled = false;
                    PropertyChanged -= OnOffTimerEvent;
                }
            }
        }

        private void CheckConfigFile()
        {
            try
            {
                FoldersConfigInfo = new FoldersConfigInfo((ProjectFoldersConfigSection)ConfigurationManager.GetSection("ProjectFolders")); // для вызова возможных ошибок config файла  при открытии приложения
                new SectionNameTemplate((SectionNameTemplateConfigSection)ConfigurationManager.GetSection("SectionNameTemplate")); // для вызова возможных ошибок config файла  при открытии приложения
                int maxAppAutoCAD = 1;
                if (Convert.ToInt32(ConfigurationManager.AppSettings["MaxCountAutoCAD"]) > maxAppAutoCAD)
                {
                    System.Windows.MessageBox.Show($"Максимальное количество приложений AutoCAD не более {maxAppAutoCAD}"
                      + "\n\nРедактируйте App.config файл для корректной работы.", $"Ошибка в App.config файле");
                    Environment.Exit(1);
                }
            }
            catch( Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message
                      + "\n\nРедактируйте App.config файл для корректной работы.", $"Ошибка в App.config файле");
                Environment.Exit(1);
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
                if (e is AggregateException)
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