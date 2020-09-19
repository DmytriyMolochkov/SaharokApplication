using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectsProjectServer
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
    }
}
