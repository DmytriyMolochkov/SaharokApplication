using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectsProjectClient;

namespace ObjectsToFormProjectClient
{
    public class SectionToProject
    {
        public string Path;
        public Dictionary<string, MethodFormFile> OutputSectionPaths;
        public List<FileToProject> FilesToPDF;
        public bool IsDone;
    }
}
