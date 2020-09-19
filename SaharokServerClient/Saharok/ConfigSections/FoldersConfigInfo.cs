using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Saharok.Model.Client;
using System.Threading;
using System.Xml;
using System.Windows;
using Saharok.View;
using Saharok.ViewModel;
using Saharok.Model;
using System.Configuration;

namespace Saharok
{
    [Serializable]
    public class FoldersConfigInfo
    {
        public List<string> FolderPaths { get; set; } = new List<string>();
        public string WorkingDirectory { get; set; }
        public string OutputPageByPagePDF { get; set; }
        public List<string> OutputFilesPDFDirectories { get; set; } = new List<string>();
        public List<string> OutputFilesZIPDirectories { get; set; } = new List<string>();

        private List<string> ErrorMessages { get; set; } = new List<string>();

        public FoldersConfigInfo(ProjectFoldersConfigSection section)
        {
            GetInfo(section.Folders);

            if (WorkingDirectory == null)
                ErrorMessages.Add("Не указан WorkingDirectory параметр. Присвойте его папке, где будут хранится редактируемые файлы.");
            if (OutputPageByPagePDF == null)
                ErrorMessages.Add("Не указан OutputPageByPagePDF параметр. Присвойте его папке, где будут хранится сконвентированные постранично в PDF файлы.");

            if (ErrorMessages.Count > 0)
            {
                ErrorMessages = ErrorMessages.Distinct().ToList();
                MessageBox.Show(String.Join(Environment.NewLine, ErrorMessages)
                   + "\n\nРедактируйте Saharok_protected.exe.config файл для корректной работы.", $"Ошибка в Saharok_protected.exe.config файле");
                Environment.Exit(1);
            }
        }

        public void GetInfo(FoldersCollection folders)
        {
            foreach (FolderElement folder in folders)
            {
                ErrorMessages.AddRange(CheckFolder(folder));
                FolderPaths.Add(folder.Path);
                if (folder.WorkingDirectory)
                    WorkingDirectory = folder.Path;
                if (folder.OutputPageByPagePDF)
                    OutputPageByPagePDF = folder.Path;
                if (folder.OutputFilesPDF)
                    OutputFilesPDFDirectories.Add(folder.Path);
                if (folder.OutputFilesZIP)
                    OutputFilesZIPDirectories.Add(folder.Path);
                GetInfo(folder.Folders);
            }
        }

        private IEnumerable<string> CheckFolder(FolderElement folder)
        {
            List<string> errorMessages = new List<string>();
            if (folder.WorkingDirectory && (folder.OutputFilesPDF || folder.OutputFilesZIP || folder.OutputPageByPagePDF))
            {
                errorMessages.Add("Нельзя папке одновременно присвоить WorkingDirectory и Output параметр.");
            }
            if (folder.WorkingDirectory && WorkingDirectory != null)
            {
                errorMessages.Add("Нельзя нескольким папкам присвоить параметр WorkingDirectory.");
            }
            if (folder.OutputPageByPagePDF && OutputPageByPagePDF != null)
            {
                errorMessages.Add("Нельзя нескольким папкам присвоить параметр OutputPageByPagePDF.");
            }
            return errorMessages;
        }
    }

    public class ProjectFoldersConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("Folders")]
        public FoldersCollection Folders
        {
            get { return ((FoldersCollection)(base["Folders"])); }
            set { base["Folders"] = value; }
        }
    }

    [ConfigurationCollection(typeof(FolderElement), AddItemName = "Folder")]
    public class FoldersCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FolderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FolderElement)(element)).Name;
        }

        public FolderElement this[int idx]
        {
            get { return (FolderElement)BaseGet(idx); }
        }
    }

    public class FolderElement : ConfigurationElement
    {
        private string path;
        public string Path
        {
            get
            {
                if (path == null)
                    path = Name;
                return path;
            }
            set => path = value;
        }

        [ConfigurationProperty("Name", DefaultValue = " ", IsKey = true, IsRequired = true)]
        [StringValidator(InvalidCharacters = @"<>*|?/:\", MinLength = 1, MaxLength = 255)]
        public string Name
        {
            get { return ((string)(base["Name"])); }
            set { base["Name"] = value; }
        }

        [ConfigurationProperty("WorkingDirectory", DefaultValue = false, IsKey = false, IsRequired = false)]
        public bool WorkingDirectory
        {
            get { return (Convert.ToBoolean((base["WorkingDirectory"]))); }
            set { base["WorkingDirectory"] = value; }
        }

        [ConfigurationProperty("OutputPageByPagePDF", DefaultValue = false, IsKey = false, IsRequired = false)]
        public bool OutputPageByPagePDF
        {
            get { return (Convert.ToBoolean((base["OutputPageByPagePDF"]))); }
            set { base["OutputPageByPagePDF"] = value; }
        }

        [ConfigurationProperty("OutputFilesPDF", DefaultValue = false, IsKey = false, IsRequired = false)]
        public bool OutputFilesPDF
        {
            get { return (Convert.ToBoolean((base["OutputFilesPDF"]))); }
            set { base["OutputFilesPDF"] = value; }
        }

        [ConfigurationProperty("OutputFilesZIP", DefaultValue = false, IsKey = false, IsRequired = false)]
        public bool OutputFilesZIP
        {
            get { return (Convert.ToBoolean((base["OutputFilesZIP"]))); }
            set { base["OutputFilesZIP"] = value; }
        }

        [ConfigurationProperty("Folders", DefaultValue = null, IsKey = false, IsRequired = false)]
        public FoldersCollection Folders
        {
            get
            {
                FoldersCollection folders = (FoldersCollection)(base["Folders"]);
                for (int i = 0; i < folders.Count; i++)
                {
                    folders[i].Path = System.IO.Path.Combine(Path, folders[i].Name);
                }
                return folders;
            }
            set { base["Folders"] = value; }
        }
    }
}
